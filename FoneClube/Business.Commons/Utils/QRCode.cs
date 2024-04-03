using IronBarCode;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Utils
{
    public class QRCode
    {
        //IronBarCode
        public void GetQRCode()
        {
            /******     WRITE     *******/
            // Create A Barcode in 1 Line of Code
            BarcodeWriter.CreateBarcode("https://ironsoftware.com/csharp/barcode", BarcodeWriterEncoding.QRCode).SaveAsJpeg("QuickStart.jpg");
            /******    READ    *******/
            // Read A Barcode in 1 Line of Code.  Gets Text, Numeric Codes, Binary Data and an Image of the barcode
            BarcodeResult Result = BarcodeReader.QuicklyReadOneBarcode("QuickStart.jpg");

            // Assert that IronBarCode Works :-)
            if (Result != null && Result.Text == "https://ironsoftware.com/csharp/barcode")
            {
                System.Console.WriteLine("Success");
            }
        }

        public Bitmap GetQRCodeBitMap()
        {
            /******     WRITE     *******/
            // Create A Barcode in 1 Line of Code
            BarcodeWriter.CreateBarcode("https://ironsoftware.com/csharp/barcode", BarcodeWriterEncoding.QRCode).SaveAsJpeg("QuickStart.jpg");
            /******    READ    *******/
            // Read A Barcode in 1 Line of Code.  Gets Text, Numeric Codes, Binary Data and an Image of the barcode
            BarcodeResult Result = BarcodeReader.QuicklyReadOneBarcode("QuickStart.jpg");

            
            // Assert that IronBarCode Works :-)
            if (Result != null && Result.Text == "https://ironsoftware.com/csharp/barcode")
            {
                return Result.BarcodeImage;
            }

            throw new Exception();
        }
    }
}
