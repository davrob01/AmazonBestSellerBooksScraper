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
        private static object locker = new object();

        static Logger()
        {
            DateTime datetime = DateTime.Now;
            File.WriteAllText("log.txt", string.Format("****** Log for {0} {1} *******", datetime.ToLongDateString(), datetime.ToLongTimeString()));
        }

        public static void Log(string message, Exception ex)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine();
            strBuilder.AppendLine(message);
            strBuilder.AppendLine(FormatException(ex));

            lock (locker)
            {
                File.AppendAllText("log.txt", strBuilder.ToString());
            }
        }

        public static void Log(Exception ex)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine();
            strBuilder.AppendLine(FormatException(ex));

            lock (locker)
            {
                File.AppendAllText("log.txt", strBuilder.ToString());
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
