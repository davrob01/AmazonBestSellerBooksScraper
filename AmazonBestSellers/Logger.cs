/* Copyright (c) David T Robertson 2016 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace AmazonBestSellers
{
    /// <summary>
    /// Logs errors to a file
    /// </summary>
    static class Logger
    {
        private static string fileName = "Results\\log.txt";
        private static object locker = new object();
        private static bool errorState = false;

        static Logger()
        {
            try
            {
                DateTime datetime = DateTime.Now;
                StringBuilder heading = new StringBuilder();
                heading.AppendFormat("********** Log for {0} {1} **********", datetime.ToLongDateString(), datetime.ToLongTimeString());
                heading.AppendLine();
                (new FileInfo(fileName)).Directory.Create();
                File.WriteAllText(fileName, heading.ToString());
            }
            catch(Exception ex)
            {
                errorState = true; // stop all future logs
                MessageBox.Show(string.Format("Error creating log file. {0}", ex.Message));
            }
        }

        public static void Log(string message, Exception ex)
        {
            if(!errorState)
            {
                try
                {
                    StringBuilder strBuilder = new StringBuilder();
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(message);
                    strBuilder.Append(FormatException(ex));
                    string output = strBuilder.ToString();

                    lock (locker)
                    {
                        File.AppendAllText(fileName, output);
                    }
                }
                catch(Exception logException)
                {
                    errorState = true; // stop all future logs
                    MessageBox.Show(string.Format("Error writing to log file. {0}", logException.Message));
                }
            }
        }

        public static void Log(Exception ex)
        {
            if (!errorState)
            {
                try
                {
                    StringBuilder strBuilder = new StringBuilder();
                    strBuilder.AppendLine();
                    strBuilder.Append(FormatException(ex));
                    string output = strBuilder.ToString();

                    lock (locker)
                    {
                        File.AppendAllText(fileName, output);
                    }
                }
                catch (Exception logException)
                {
                    errorState = true; // stop all future logs
                    MessageBox.Show(string.Format("Error writing to log file. {0}", logException.Message));
                }
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
                strBuilder.AppendLine();
                strBuilder.AppendLine(ex.InnerException.Message);
                strBuilder.AppendLine(ex.InnerException.StackTrace);
            }
            return strBuilder.ToString();
        }
    }
}
