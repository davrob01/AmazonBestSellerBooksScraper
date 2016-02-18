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
            bool result;
            var mutex = new System.Threading.Mutex(true, "D-ROB Software/ AmazonBestSellers", out result);

            if (!result)
            {
                MessageBox.Show("Another instance is already running.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            GC.KeepAlive(mutex);                // mutex shouldn't be released - important line
        }
    }
}
