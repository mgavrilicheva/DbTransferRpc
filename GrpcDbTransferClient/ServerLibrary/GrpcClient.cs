using Dbtransferservice;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;

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

        public override void Send(byte[] data)
        {
            Channel channel = new Channel(serverAddress, serverPort, credentials);
            try
            {
                var client = new DbTransferService.DbTransferServiceClient(channel);
                byte[] encryptedData = data.Compress().Encrypt(out byte[] symmetricKey);
                Token token = client.GetToken(new Empty());
                byte[] encryptedSymmetricKey =
                    EncodeSymmetricKey(symmetricKey, token.PublicKey.ToByteArray());
                DataResponse response =
                    client.AcceptData(new DataParams()
                    {
                        Token = token,
                        SymmetricKey = ByteString.CopyFrom(encryptedSymmetricKey),
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