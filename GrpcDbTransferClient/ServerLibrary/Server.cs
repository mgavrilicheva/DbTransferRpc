using System;
using System.Security.Cryptography;
using System.Threading;

namespace ExchangeLibrary
{
    public abstract class Server
    {
        protected readonly ManualResetEvent allDone;
        public bool IsRunning { get; protected set; }
        public Action<byte[], string> ReceivedDataHandler { get; set; }
        protected readonly Thread serverThread;

        public Server()
        {
            allDone = new ManualResetEvent(false);
            serverThread = new Thread(DoWork)
            {
                IsBackground = true
            };
        }

        public void Dispose()
        {
            if (IsRunning)
                Stop();
            allDone.Dispose();
        }

        protected abstract void DoWork();

        protected RSACryptoServiceProvider InitializeAsymmetricCyphering()
        {
            return new RSACryptoServiceProvider(4096,
                new CspParameters(1)
                {
                    KeyContainerName = "KeyContainer",
                    Flags = CspProviderFlags.UseMachineKeyStore,
                    ProviderName = "Microsoft Strong Cryptographic Provider"
                });
        }

        public void Start()
        {
            if (IsRunning)
                return;
            serverThread.Start();
        }

        public void Stop()
        {
            if (!IsRunning)
                return;
            serverThread.Abort();
            IsRunning = false;
        }
    }
}
