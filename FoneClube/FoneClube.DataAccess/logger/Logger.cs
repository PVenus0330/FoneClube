using CieloLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggerNet
{
    public class Logger : ILogger
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Error(string message, Exception ex)
        {
            log.Error(message, ex);
        }

        public void Info(string message)
        {
            log.Info(message);
        }

        public void InsertLog(MessageLevel messageLevel, string messageSummary, string messageDetail)
        {
            switch (messageLevel)
            {
                case MessageLevel.Information:
                    log.Info($"summary: {messageSummary}, \t detail: {messageDetail}");
                    break;
                case MessageLevel.Error:
                    log.Error($"summary: {messageSummary}, \t detail: {messageDetail}");
                    break;
                default:
                    break;
            }
        }
    }
}
