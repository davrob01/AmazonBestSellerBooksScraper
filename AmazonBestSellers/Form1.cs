using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace AmazonBestSellers
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ServicePointManager.DefaultConnectionLimit = 30;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
            btnStart.Enabled = false;
            var watch = Stopwatch.StartNew();

            Thread thread = new Thread(new ThreadStart(WorkThreadFunction));
            thread.Start();

            while(Counter.Finished != true)
            {
                Thread.Sleep(250);
                lblBooksValue.Text = Counter.BooksAdded.ToString();
                this.Refresh();
            }
            lblBooksValue.Text = Counter.BooksAdded.ToString();

            long time = watch.ElapsedMilliseconds / 1000;

            panel2.Visible = true;
            lblTimeValue.Text = string.Format("{0} seconds", time.ToString());
            btnStart.Enabled = true;
            Counter.Reset();
        }

        public async void WorkThreadFunction()
        {
            try
            {
                // do any background work
                await StartScrape();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                Counter.Finished = true;
            }
        }

        private async Task StartScrape()
        {
            //string url = "http://www.amazon.com/gp/bestsellers/books";
            //string url = "http://www.amazon.com/Best-Sellers-Books-Arts-Photography/zgbs/books/1/ref=zg_bs_unv_b_2_173508_1";
            string url = "http://www.amazon.com/Best-Sellers-Books-Architectural-Buildings/zgbs/books/266162/ref=zg_bs_nav_b_3_173508";
            Domain amazonUS = new Domain(url, "Books");

            await amazonUS.ProcessCategory();

            using (StreamWriter writer = new StreamWriter("output.csv"))
            {
                writer.WriteLine("Category,Rank,ISBN,Title");
                foreach (Category category in amazonUS.Categories.OrderBy(x => x.Name))
                {
                    foreach (Book book in category.Books.OrderBy(x => x.Rank))
                    {
                        writer.WriteLine("\"{0}\",=\"{1}\",=\"{2}\",\"{3}\"", category.Name, book.Rank, book.ISBN, book.Title);
                    }
                }
            }
        }
    }
}
