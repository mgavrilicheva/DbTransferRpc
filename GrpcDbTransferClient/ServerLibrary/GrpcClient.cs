using Dbtransferservice;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Buffers.Text;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ExchangeLibrary
{
    public class GrpcClient : Client
    {
        private readonly string serverAddress;
        private readonly int serverPort;
        private readonly ChannelCredentials credentials;

        public GrpcClient(string serverAddress, int serverPort, ChannelCredentials credentials)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
            this.credentials = credentials;
        }

        private static byte[] Encrypt(byte[] symmetricKey, X509Certificate2 certificate) 
        {
            byte[] encryptedKey = null;
            using (var rsa = (RSACryptoServiceProvider)certificate.PublicKey.Key)
            {
                encryptedKey = rsa.Encrypt(symmetricKey, false);
            }
            return encryptedKey;
        } 

        private static X509Certificate2 GetCertificateFromBytes(byte[] cert)
        {
            string certFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                File.WriteAllBytes(certFile, cert);

                X509Store store = new X509Store(StoreLocation.CurrentUser);
                try
                {
                    store.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection certCollection = store.Certificates;
                    return certCollection[0];
                }
                finally
                {
                    store.Close();
                }
            }
            finally
            {
                File.Delete(certFile);
            }
        }

        public override void Send(byte[] data)
        {
            Channel channel = new Channel(serverAddress, serverPort, credentials);
            try
            {
                var client = new DbTransferService.DbTransferServiceClient(channel);
                byte[] encryptedData = data.Compress().Encrypt(out byte[] symmetricKey);
                //Token token = client.GetToken(new Empty());
                //X509Certificate2 certificate = GetCertificateFromBytes(token.PublicKey.ToByteArray());
                //byte[] encryptedSymmetricKey = Encrypt(symmetricKey, certificate);
                Console.WriteLine(Convert.ToBase64String(symmetricKey));
                DataResponse response =
                    client.AcceptData(new DataParams()
                    {
                        SymmetricKey = ByteString.CopyFrom(symmetricKey),
                        Data = ByteString.CopyFrom(encryptedData),
                    });
                if (!response.Status)
                    throw new Exception(response.Message);
            }
            finally
            {
                channel.ShutdownAsync().Wait();
            }
        }
    }
}