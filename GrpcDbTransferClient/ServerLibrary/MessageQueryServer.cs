using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace ExchangeLibrary
{
    public class MessageQueryServer : Server
    {
        private readonly ConnectionFactory factory;

        public MessageQueryServer(string hostName, int port, string user, string password)
        {
            factory = new ConnectionFactory()
            {
                HostName = hostName,
                Password = password,
                Port = port,
                SocketReadTimeout = 10000,
                SocketWriteTimeout = 10000,
                UserName = user,
            };
        }

        protected override void DoWork()
        {
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("initialize", false, false, false, null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += HandleConnection;
                    while(true)
                    {
                        allDone.Reset();
                        channel.BasicConsume("initialize", true, consumer);
                        allDone.WaitOne();
                    }
                }
            }
        }

        private void HandleConnection(object channel, BasicDeliverEventArgs eventArgs)
        {
            IModel tmpChannel = (IModel)channel;
            Guid associatedGuid = new Guid(eventArgs.Body);
            allDone.Set();
        }
    }
}
