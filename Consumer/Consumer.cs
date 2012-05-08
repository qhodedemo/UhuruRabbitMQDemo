using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Content;
using System.Web.Configuration;
using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json;
using RabbitConsumer;

namespace RabbitConsumer
{
    public class Consumer : IDisposable
    {
        protected IModel Model { get; set; }
        protected IConnection Connection { get; set; }
        public string Exchange { get; set; }
        public string ExchangeTypeName { get; set; }
        public string QueueName { get; set; }

        protected string connString = WebConfigurationManager.AppSettings["dbConnectionString"];
        protected string vcapConn = WebConfigurationManager.AppSettings["VCAP_SERVICES"];

        protected SqlConnection conn;

        public Consumer(string exchange, string exchangeType)
        {
            Exchange = exchange;
            ExchangeTypeName = exchangeType;
        }

        //Create the connection, Model and Exchange(if one is required)
        public virtual bool ConnectToRabbitMQ()
        {
            try
            {
                var connectionFactory = new ConnectionFactory();
                Dictionary<string, Service[]> connParams = JsonConvert.DeserializeObject<Dictionary<string, Service[]>>(vcapConn);

                if (connParams.ContainsKey("rabbitmq-2.4"))
                {
                    Service s = connParams["rabbitmq-2.4"][0];
                    connectionFactory.HostName = s.Credential.Hostname;
                    connectionFactory.UserName = s.Credential.UserName;
                    connectionFactory.Password = s.Credential.Password;
                    connectionFactory.Port = s.Credential.Port;
                    connectionFactory.VirtualHost = s.Credential.VHost;
                }

                Connection = connectionFactory.CreateConnection();
                Model = Connection.CreateModel();

                if (!String.IsNullOrEmpty(Exchange)) Model.ExchangeDeclare(Exchange, ExchangeTypeName,true);

                Model.BasicQos(0, 1, false);
                QueueName = Model.QueueDeclare("incidentQueue", true, false, false, null);
                Model.QueueBind(QueueName, Exchange, "");

                //SQL Connection
                conn = new SqlConnection(connString);
                conn.Open();

                return true;
            }
            catch (BrokerUnreachableException e)
            {
                return false;
            }
        }

        public void ReceiveMessage()
        {
            bool noAck = false;
            IBasicProperties basicProperties = Model.CreateBasicProperties();
            byte[] body;
            BasicGetResult result = Model.BasicGet(QueueName, noAck);

            if (result == null)
            {
                body = System.Text.Encoding.UTF8.GetBytes("");
                // No message available at this time.
            }
            else
            {
                IBasicProperties props = result.BasicProperties;
                body = result.Body;
                Model.BasicAck(result.DeliveryTag, false);

                string jsonText = System.Text.Encoding.UTF8.GetString(body);
                Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);

                try
                {
                    SqlCommand command = conn.CreateCommand();
                    command.CommandText = "IF OBJECT_ID('dbo.tblRabbitQueueMsg','U') IS NULL BEGIN CREATE TABLE dbo.tblRabbitQueueMsg (ClientName varchar(100), QName varchar(50) , QDesc varchar(50), QPriority varchar(10)) END";
                    command.ExecuteNonQuery();

                    command.CommandText = "insert into dbo.tblRabbitQueueMsg (ClientName, QName, QDesc, QPriority) values (\'.NetReceiver\', \'" + values["name"] + "\', \'" + values["description"] + "\', \'" + values["priority"] + "\' )";
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void Dispose()
        {
            if (Connection != null) Connection.Close();
            if (Model != null) Model.Abort();
        }
    }
}