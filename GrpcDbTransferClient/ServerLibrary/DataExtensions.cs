using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace ExchangeLibrary
{
    static class DataExtensions
    {
        public static byte[] Compress(this byte[] data)
        {
            using (MemoryStream originalDataStream = new MemoryStream(data))
            {
                using (MemoryStream compressedDataStream = new MemoryStream())
                {
                    using (GZipStream compressionStream = new GZipStream(compressedDataStream, CompressionMode.Compress))
                    {
                        originalDataStream.CopyTo(compressionStream);
                    }
                    return compressedDataStream.ToArray();
                }
            }
        }

        public static byte[] Decompress(this byte[] data)
        {
            using (MemoryStream sourceStream = new MemoryStream(data))
            {
                using (MemoryStream targetStream = new MemoryStream())
                {
                    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(targetStream);
                    }
                    return targetStream.ToArray();
                }
            }
        }

        public static byte[] Decrypt(this byte[] data, byte[] symmetricKey)
        {
            using (DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider())
            {
                cryptoProvider.Key = symmetricKey;
                cryptoProvider.IV = symmetricKey;
                using (MemoryStream decryptedDataStream = new MemoryStream())
                {
                    using (CryptoStream decryptionStream = new CryptoStream(decryptedDataStream,
                            cryptoProvider.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        decryptionStream.Write(data, 0, data.Length);
                    }
                    return decryptedDataStream.ToArray();
                }
            }
        }

        public static byte[] Encrypt(this byte[] data, out byte[] symmetricKey)
        {
            byte[] encryptedData = null;
            using (DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider())
            {
                cryptoProvider.GenerateKey();
                cryptoProvider.IV = cryptoProvider.Key;
                using (MemoryStream encryptedDataStream = new MemoryStream())
                {
                    using (CryptoStream encryptionStream = new CryptoStream(encryptedDataStream,
                            cryptoProvider.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        encryptionStream.Write(data, 0, data.Length);
                    }
                    encryptedData = encryptedDataStream.ToArray();
                }
                symmetricKey = cryptoProvider.Key;
            }
            return encryptedData;
        }

        public static byte[] ReceiveMessageBySocket(Socket handler)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[1024];
                int bytesReceived = 0;
                do
                {
                    bytesReceived = 0;
                    try
                    {
                        bytesReceived = handler.Receive(buffer);
                    }
                    catch { }
                    ms.Write(buffer, 0, bytesReceived);
                } while (bytesReceived > 0);
                return ms.ToArray();
            }
        }
    }
}
