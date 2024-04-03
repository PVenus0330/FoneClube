using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieloLib
{
    public interface ILogger
    {
        void InsertLog(MessageLevel messageLevel, string messageSummary, string messageDetail);
        void Error(string message, Exception ex);
        void Info(string message);
    }

    class Logger : ILogger
    {
        public void Error(string message, Exception ex)
        {
        }

        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        public void InsertLog(MessageLevel messageLevel, string messageSummary, string messageDetail)
        {
        }
    }
}
