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
        static Logger()
        {
            DateTime datetime = DateTime.Now;
            File.WriteAllText("log.txt", string.Format("****** Log for {0} {1} *******", datetime.ToLongDateString(), datetime.ToLongTimeString()));
        }

        public static void Log(Exception ex)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine(ex.GetType().Name);
            strBuilder.AppendLine(ex.Message);
            strBuilder.AppendLine(ex.StackTrace);
            strBuilder.AppendLine();

            File.AppendAllText("log.txt", strBuilder.ToString());
        }
    }
}
