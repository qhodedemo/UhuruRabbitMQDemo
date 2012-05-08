using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Events;
using System.Web.Configuration;
using Newtonsoft.Json;
using RabbitProducer;

namespace RabbitProducer
{
    public class Producer : IDisposable
    {
        protected IModel Model { get; set; }
        protected IConnection Connection { get; set; }
        public string Exchange { get; set; }
        public string ExchangeTypeName { get; set; }
        public string QueueName { get; set; }

        protected string vcapConn = WebConfigurationManager.AppSettings["VCAP_SERVICES"];

        public Producer(string exchange, string exchangeType)
        {
            Exchange = exchange;
            ExchangeTypeName = exchangeType;
        }

        //Create the connection, Model and Exchange(if one is required)
        public virtual bool ConnectToRabbitMQ()
        {
            try
            {
                Dictionary<string, Service[]> connParams = JsonConvert.DeserializeObject<Dictionary<string, Service[]>>(vcapConn);
                var connectionFactory = new ConnectionFactory();

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
                bool durable = true;
                if (!String.IsNullOrEmpty(Exchange))
                    Model.ExchangeDeclare(Exchange, ExchangeTypeName, durable);
                QueueName = Model.QueueDeclare ("incidentQueue", true, false, false, null);
                Model.QueueBind(QueueName, Exchange,"");
 
                return true;
            }
            catch (BrokerUnreachableException e)
            {
                return false;
            }
        }

        public void SendMessage(byte[] message)
        {
            Model.BasicPublish(Exchange,"", null, message);
        }

        public void Dispose()
        {
            if (Connection != null)
                Connection.Close();

            if (Model != null)
                Model.Abort();
        }
    }
}