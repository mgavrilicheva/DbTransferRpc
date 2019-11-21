using System.Net;
using System.Net.Sockets;

namespace ExchangeLibrary
{
    public class SocketClient : Client
    {
        private readonly IPAddress serverIpAddress;
        private readonly int serverPort;

        public SocketClient(IPAddress serverIpAddress, int serverPort)
        {
            this.serverIpAddress = serverIpAddress;
            this.serverPort = serverPort;
        }

        public override void Send(byte[] data)
        {
            IPEndPoint remoteEP = new IPEndPoint(serverIpAddress, serverPort);
            byte[] encryptedData = data.Compress().Encrypt(out byte[] symmetricKey);
            using (Socket client = new Socket(serverIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = 10000,
                SendTimeout = 10000
            })
            {
                client.Connect(remoteEP);
                byte[] publicKey = DataExtensions.ReceiveMessageBySocket(client);
                if (publicKey.Length == 0)
                    throw new ExchangeException();
                byte[] encryptedSymmetricKey = EncodeSymmetricKey(symmetricKey, publicKey);
                client.Send(encryptedSymmetricKey);
                client.Send(encryptedData);
                client.Shutdown(SocketShutdown.Both);
            }
        }

        public override void Stop()
        {
            
        }
    }
}
