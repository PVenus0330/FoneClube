using System;
using System.Text;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace FoneClube.DataAccess.Utilities
{
    public static class PdfHelper
    {
        public static string ExtractTextFromPdf(string path)
        {
            try
            {
                using (PdfReader reader = new PdfReader(path))
                {
                    StringBuilder text = new StringBuilder();

                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                    }

                    return text.ToString();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(1, "ExtractTextFromPdf error:" + ex.ToString());
            }
            return string.Empty;
        }
    }
}
