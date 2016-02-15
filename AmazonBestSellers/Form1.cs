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

        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            var watch = Stopwatch.StartNew();
            await StartScrape();
            long time = watch.ElapsedMilliseconds / 1000;

            lblTimeValue.Text = time.ToString();
            btnStart.Enabled = true;
        }

        private async Task StartScrape()
        {
            //string url = "http://www.amazon.com/gp/bestsellers/books";
            string url = "http://www.amazon.com/Best-Sellers-Books-Arts-Photography/zgbs/books/1/ref=zg_bs_unv_b_2_173508_1";
            //string url = "http://www.amazon.com/Best-Sellers-Books-Architectural-Buildings/zgbs/books/266162/ref=zg_bs_nav_b_3_173508";
            Domain amazonUS = new Domain(url, "Books");

            await amazonUS.ProcessCategory();

            StringBuilder strBuilder = new StringBuilder();

            foreach (Category category in amazonUS.Categories.OrderBy(x => x.Name))
            {
                strBuilder.AppendLine();
                strBuilder.AppendLine(category.Name);
                strBuilder.AppendLine();
                foreach (Book book in category.Books.OrderBy(x => x.Rank))
                {
                    strBuilder.AppendFormat("{0} - {1} - {2} - {3}", category.Name, book.Rank, book.ISBN, book.Title);
                    strBuilder.AppendLine();
                }
            }

            //textBox1.Text = strBuilder.ToString();

            File.WriteAllText("output.txt", strBuilder.ToString());
        }
    }
}
