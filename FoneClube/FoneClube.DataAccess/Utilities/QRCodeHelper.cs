using System;
using QRCoder;
using System.IO;
using System.Drawing;

namespace FoneClube.DataAccess.Utilities
{
    public class QRCodeHelper
    {
        public static string GenerateQRCode(string message)
        {
            string imgUrl = string.Empty;
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(message, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap bitMap = qrCode.GetGraphic(20))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        byte[] byteImage = ms.ToArray();
                        imgUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);
                    }
                }
            }
            catch (Exception ex) { }
            return imgUrl;
        }
    }
}
