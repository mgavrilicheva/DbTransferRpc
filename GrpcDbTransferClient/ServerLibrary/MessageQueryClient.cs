using RabbitMQ.Client;
using System;

namespace ExchangeLibrary
{
    public class MessageQueryClient : Client
    {
        private readonly ConnectionFactory factory;
        private readonly Guid guid;

        public MessageQueryClient(string hostName, int port, string user, string password)
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
            guid = new Guid();
        }

        public override void Send(byte[] data)
        {
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    channel.QueueDeclare("initialize", false, false, false, null);
                    channel.BasicPublish("", "initialize", null, guid.ToByteArray());
                }
            }
        }
    }
}
