using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otsol.Support.Json;
using System.Configuration;

namespace Mobiera
{
    class Program
    {
        protected static ITextMessage message = null;

        static void Main(string[] args)
        {

            receiver();
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }


        private static void receiver()
        {
            string URI = ConfigurationManager.AppSettings["activeMqUri"];

            Uri connecturi = new Uri(URI);
            string strDestination = ConfigurationManager.AppSettings["optInQueue"];
            Console.WriteLine("About to connect to " + connecturi);
            IConnectionFactory connectionFactory = new NMSConnectionFactory(connecturi);
            IConnection _connection = connectionFactory.CreateConnection();
            _connection.Start();

            Console.WriteLine("Using destination: " + strDestination);

            ISession _session = _connection.CreateSession();
            IDestination destination = _session.GetDestination(strDestination);
            IMessageConsumer consumer = _session.CreateConsumer(destination);
            consumer.Listener += new MessageListener(OnMessage);
        }
        protected static void OnMessage(IMessage receivedMsg)
        {
            message = ((Apache.NMS.ActiveMQ.Commands.ActiveMQTextMessage)(receivedMsg));
            JObject data = JObject.Parse(((Apache.NMS.ActiveMQ.Commands.ActiveMQTextMessage)(receivedMsg)).Text);
            BOSubsOptin BO = new BOSubsOptin();
            BO.RegisterOptin(data);

        }

    }
}
