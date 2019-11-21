using Dbtransferservice;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;

namespace ExchangeLibrary
{
    public class GrpcClient : Client
    {
        private readonly Channel channel;
        private readonly DbTransferService.DbTransferServiceClient client;

        public GrpcClient(string serverAddress, int serverPort, ChannelCredentials credentials)
        {
            channel = new Channel(serverAddress, serverPort, ChannelCredentials.Insecure);
            client = new DbTransferService.DbTransferServiceClient(channel);
        }

        public override void Send(byte[] data)
        {
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

        public override void Stop()
        {
            channel.ShutdownAsync().Wait();
        }
    }
}