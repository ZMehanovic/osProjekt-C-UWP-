using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace OsProjekt
{
    class CryptographyHelper
    {
        private static Chilkat.Rsa signatureRsa;

        public static byte[] EncryptAes(string originText, Aes aes)
        {
            byte[] encrypted;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryEncrypt = new MemoryStream())
            {
                using (CryptoStream cryptoEncrypt = new CryptoStream(memoryEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamEncrypt = new StreamWriter(cryptoEncrypt))
                    {
                        streamEncrypt.Write(originText);
                    }
                    encrypted = memoryEncrypt.ToArray();
                }
            }
            return encrypted;
        }

        public static string DecryptAes(byte[] cryptText, Aes aes)
        {
            string decrypted = null;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryDecryptor = new MemoryStream(cryptText))
            {
                using (CryptoStream cryptoDecryptor = new CryptoStream(memoryDecryptor, decryptor, CryptoStreamMode.Read))
                {
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoDecryptor))
                        {

                            decrypted = streamReader.ReadToEnd();
                        }

                    }
                    catch
                    {
                       decrypted = "Dekrpicija nije uspjela...";
                    }
                }
            }
            return decrypted;
        }
        public static byte[] EncryptRSA(byte[] original, RSAParameters RSAKeyInfo)
        {
            byte[] encryptedData;

            RSA rsa = RSA.Create();
            rsa.ImportParameters(RSAKeyInfo);
            encryptedData = rsa.Encrypt(original, RSAEncryptionPadding.Pkcs1);

            return encryptedData;
        }
        public static String DecryptRSA(byte[] encryptedData, RSAParameters RSAKeyInfo, bool padding)
        {
            byte[] decryptedData;

            RSA rsa = RSA.Create();
            rsa.ImportParameters(RSAKeyInfo);
            try
            {
                decryptedData = rsa.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1);
            }
            catch
            {
                return "Dekrpicija nije uspjela...";
            }
            return Encoding.UTF8.GetString(decryptedData);
        }
        public static string Sha256Hash(string data)
        {
            IBuffer input = CryptographicBuffer.ConvertStringToBinary(data,
            BinaryStringEncoding.Utf8);

            IBuffer hashed = HashAlgorithmProvider.OpenAlgorithm("SHA256").HashData(input);

            return CryptographicBuffer.EncodeToBase64String(hashed);
        }
        public static String CreateSignature(String fileContent)
        {
            signatureRsa = new Chilkat.Rsa();

            signatureRsa.UnlockComponent("30-day trial");

            signatureRsa.GenerateKey(1024);


            signatureRsa.EncodingMode = "hex";
            signatureRsa.LittleEndian = false;

            return signatureRsa.SignStringENC(fileContent, "sha-256");
        }
        public static bool VerifySignature(String fileContent, String signature)
        {

            return signatureRsa.VerifyStringENC(fileContent, "sha-256", signature);
        }
    }
}
