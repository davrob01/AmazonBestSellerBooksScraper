/* Copyright (c) David T Robertson 2016 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmazonBestSellers
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool autoStart = false;

            string[] args = Environment.GetCommandLineArgs();

            try
            {
                foreach (string arg in args)
                {
                    if (arg.ToUpper().Equals("AUTOSTART"))
                    {
                        autoStart = true;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format("Error parsing command-line arguments. {0}", ex.Message));
            }
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(autoStart));

        }
    }
}
