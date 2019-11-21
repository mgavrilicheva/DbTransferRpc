using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace ExchangeLibrary
{
    public class SocketServer : Server, IDisposable
    {
        private readonly IPAddress ipAddress;
        protected readonly int port;

        public SocketServer(int port, IPAddress ipAddress)
        {
            this.port = port;
            this.ipAddress = ipAddress;
        }

        protected override void DoWork()
        {
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            using (Socket serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = 600000,
                SendTimeout = 600000
            })
            {
                serverSocket.Bind(localEndPoint);
                serverSocket.Listen(100);
                while (true)
                {
                    allDone.Reset();
                    serverSocket.BeginAccept(HandleConnection, serverSocket);
                    allDone.WaitOne();
                }
            }
        }

        private void HandleConnection(IAsyncResult result)
        {
            allDone.Set();
            Socket listener = (Socket)result.AsyncState;
            Socket handler = listener.EndAccept(result);
            byte[] receivedData = ReceiveData(handler);
            ReceivedDataHandler?.Invoke(receivedData, handler.RemoteEndPoint.ToString());
            handler.Shutdown(SocketShutdown.Both);
        }

        private byte[] ReceiveData(Socket handler)
        {
            RSACryptoServiceProvider cryptoProvider = InitializeAsymmetricCyphering();
            byte[] publicKey = cryptoProvider.ExportCspBlob(false);
            handler.Send(publicKey);
            byte[] encryptedSymmetricKey = new byte[512];
            handler.Receive(encryptedSymmetricKey);
            byte[] encryptedData = DataExtensions.ReceiveMessageBySocket(handler);
            byte[] symmetricKey = cryptoProvider.Decrypt(encryptedSymmetricKey, RSAEncryptionPadding.Pkcs1);
            cryptoProvider.Dispose();
            return encryptedData.Decrypt(symmetricKey).Decompress();
        }
    }
}
