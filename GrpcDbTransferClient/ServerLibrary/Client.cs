﻿using System.Security.Cryptography;

namespace ExchangeLibrary
{
    public abstract class Client
    {
        protected byte[] EncodeSymmetricKey(byte[] symmetricKey, byte[] publicKey)
        {
            byte[] encryptedKey = null;
            using (var rsa = new RSACryptoServiceProvider(4096))
            {
                rsa.ImportCspBlob(publicKey);
                encryptedKey = rsa.Encrypt(symmetricKey, RSAEncryptionPadding.Pkcs1);
            }
            return encryptedKey;
        }

        public abstract void Send(byte[] data);
        public abstract void Stop();
    }
}
