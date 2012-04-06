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

namespace AsyncRabbitWeb
{
    public class Consumer : IDisposable
    {
        
        protected IModel Model { get; set; }
        protected IConnection Connection { get; set; }
        public string Exchange { get; set; }
        public string ExchangeTypeName { get; set; }
        public string QueueName { get; set; }

        protected string connString = WebConfigurationManager.AppSettings["dbConnectionString"];
        protected SqlConnection conn;


        //This is the delete for internal calling
        private delegate void ConsumeDelegate();
        protected bool isConsuming;

        // used to pass messages back to UI for processing
        public delegate void onReceiveMessage(byte[] message);
        public event onReceiveMessage onMsgRec;


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
                //connectionFactory.HostName = "192.168.1.187";
                //connectionFactory.UserName = "ubFMlFC1cLN8E";
                //connectionFactory.Password = "phWo9XIq9uMQk";
                //connectionFactory.Port = 5672;
                //connectionFactory.VirtualHost = "v5c1f0be1bb7a4ba9a389a8bba32b0b69";

                connectionFactory.HostName = "192.168.1.187";
                connectionFactory.UserName = "ubsVNgAxn9y9b";
                connectionFactory.Password = "pTbuZC7vz8r0m";
                connectionFactory.Port = 5672;
                connectionFactory.VirtualHost = "ve4eb67d61617422786d03fa801f55e55";

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

        public void StartConsuming()
        {
            isConsuming = true;
            ConsumeDelegate c = new ConsumeDelegate(Consume);
            c.BeginInvoke(null, null);
        }

        public void Consume()
        {
            QueueingBasicConsumer consumer = new QueueingBasicConsumer(Model);
            String consumerTag = Model.BasicConsume(QueueName, true, consumer);

            while (isConsuming)
            {
                try
                {
                    BasicDeliverEventArgs e = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    IBasicProperties props = e.BasicProperties;
                    byte[] body = e.Body;
                    // ... process the message
                    try
                    {
                        SqlCommand command = conn.CreateCommand();
                        command.CommandText = "IF OBJECT_ID('dbo.tblRabbitQueueMsg','U') IS NULL BEGIN CREATE TABLE dbo.tblRabbitQueueMsg (ClientName varchar(100), QName varchar(50) , QDesc varchar(50), QPriority varchar(10)) END";
                        command.ExecuteNonQuery();

                        string[] strValues = System.Text.Encoding.UTF8.GetString(body).Split(' ');

                        command.CommandText = "insert into dbo.tblRabbitQueueMsg (ClientName, QName, QDesc, QPriority) values (\'Client1\', \'" + strValues[0] + "\', \'" + strValues[1] + "\', \'" + strValues[2] + "\' )";
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {

                    }

//                    Model.BasicAck(e.DeliveryTag, false);
                }
                catch (OperationInterruptedException ex)
                {
                    // The consumer was removed, either through
                    // channel or connection closure, or through the
                    // action of IModel.BasicCancel().
                    break;
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