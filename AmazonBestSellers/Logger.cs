using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AmazonBestSellers
{
    static class Logger
    {
        private const string fileName = "log.txt";
        private static object locker = new object();

        static Logger()
        {
            DateTime datetime = DateTime.Now;
            StringBuilder heading = new StringBuilder();
            heading.AppendFormat("********** Log for {0} {1} **********", datetime.ToLongDateString(), datetime.ToLongTimeString());
            heading.AppendLine();
            File.WriteAllText(fileName, heading.ToString());
        }

        public static void Log(string message, Exception ex)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine();
            strBuilder.AppendLine(message);
            strBuilder.Append(FormatException(ex));

            lock (locker)
            {
                File.AppendAllText(fileName, strBuilder.ToString());
            }
        }

        public static void Log(Exception ex)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine();
            strBuilder.Append(FormatException(ex));

            lock (locker)
            {
                File.AppendAllText(fileName, strBuilder.ToString());
            }
        }

        private static string FormatException(Exception ex)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine(ex.GetType().Name);
            strBuilder.AppendLine(ex.Message);
            strBuilder.AppendLine(ex.StackTrace);
            if(ex.InnerException != null)
            {
                strBuilder.AppendLine();
                strBuilder.AppendFormat("Inner Exception: {0}", ex.InnerException.GetType().Name);
                strBuilder.AppendLine(ex.InnerException.Message);
                strBuilder.AppendLine(ex.InnerException.StackTrace);
            }
            return strBuilder.ToString();
        }
    }
}
