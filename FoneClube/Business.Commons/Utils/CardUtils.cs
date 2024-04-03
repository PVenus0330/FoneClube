using FoneClube.Business.Commons.Entities.FoneClube;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Utils
{
    public class CardUtils
    {

        public bool isValidCard(string encriptedCard)
        {
            var decripted = Base64Decode(DecryptString(encriptedCard)).Split('_');
            var card = new CardFoneclube
            {
                HolderName = decripted[0],
                Number = decripted[1],
                Cvv = decripted[2],
                ExpirationMonth = decripted[3],
                ExpirationYear = decripted[4],
                Flag = decripted[5]
            };

            return
                !string.IsNullOrEmpty(card.HolderName)
                && !string.IsNullOrEmpty(card.Number)
                && !string.IsNullOrEmpty(card.Cvv)
                && !string.IsNullOrEmpty(card.ExpirationMonth)
                && !string.IsNullOrEmpty(card.ExpirationYear)
                && !string.IsNullOrEmpty(card.Flag)
                && IsDigitsOnly(card.Number)
                && IsDigitsOnly(card.Cvv)
                && IsDigitsOnly(card.ExpirationMonth)
                && IsDigitsOnly(card.ExpirationYear)
                && IsOnlyLetters(card.HolderName.Replace(" ", string.Empty));
        }

        public bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public bool IsOnlyLetters(string str)
        {
            return Regex.IsMatch(str, @"^[a-zA-Z]+$");
        }

        public CardFoneclube GetCard(string encriptedCard)
        {
            var decripted = Base64Decode(DecryptString(encriptedCard)).Split('_');
            return new CardFoneclube
            {
                HolderName = decripted[0],
                Number = decripted[1],
                Cvv = decripted[2],
                ExpirationMonth = decripted[3],
                ExpirationYear = decripted[4],
                Flag = decripted[5]
            };
        }

        public string PrepareCard(CardFoneclube card)
        {
            return card.HolderName + "_" +
                   card.Number + "_" +
                   card.Cvv + "_" +
                   card.ExpirationMonth + "_" +
                   card.ExpirationYear + "_" +
                   card.Flag;
        }

        public string EncryptFC(string card)
        {
            return EncryptString(Base64Encode(card));
        }

        public string DecryptFC(string card)
        {
            return Base64Decode(DecryptString(card));
        }

        public string EncryptString(string value)
        {
            MemoryStream stream = null;
            try
            {
                byte[] key = { };
                byte[] IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
                string encryptKey = "aXb2uy4z";
                key = Encoding.UTF8.GetBytes(encryptKey);
                byte[] byteInput = Encoding.UTF8.GetBytes(value);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                stream = new MemoryStream();
                ICryptoTransform transform = provider.CreateEncryptor(key, IV);
                CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(byteInput, 0, byteInput.Length);
                cryptoStream.FlushFinalBlock();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Convert.ToBase64String(stream.ToArray());
        }

        public string DecryptString(string value)
        {
            MemoryStream stream = null;
            try
            {

                byte[] key = { };
                byte[] IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
                string encryptKey = "aXb2uy4z";
                key = Encoding.UTF8.GetBytes(encryptKey);
                byte[] byteInput = new byte[value.Length];
                byteInput = Convert.FromBase64String(value);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                stream = new MemoryStream();
                ICryptoTransform transform = provider.CreateDecryptor(key, IV);
                CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(byteInput, 0, byteInput.Length);
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Encoding encoding1 = Encoding.UTF8;
            return encoding1.GetString(stream.ToArray());
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
