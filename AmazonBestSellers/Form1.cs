﻿using System;
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
            /*
            ServicePoint amazonUS = ServicePointManager.FindServicePoint(new Uri("http://www.amazon.com"));
            amazonUS.ConnectionLimit = 10;
            ServicePoint amazonJPN = ServicePointManager.FindServicePoint(new Uri("http://www.amazon.co.jp"));
            amazonJPN.ConnectionLimit = 10;
            */
            ServicePointManager.DefaultConnectionLimit = 2;
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
            btnStart.Enabled = false;
            var watch = Stopwatch.StartNew();

            /* Test code */
            /*
            //string url = "http://www.amazon.com/gp/bestsellers/books";
            //string url2 = "http://www.amazon.co.jp/gp/bestsellers/english-books/";
            string url = "http://www.amazon.com/Best-Sellers-Books-Arts-Photography/zgbs/books/1/ref=zg_bs_unv_b_2_173508_1";
            //string url = "http://www.amazon.com/Best-Sellers-Books-Engineering-Transportation/zgbs/books/173507/ref=zg_bs_nav_b_1_b";
            //string url = "http://www.amazon.com/Best-Sellers-Books-Architectural-Buildings/zgbs/books/266162/ref=zg_bs_nav_b_3_173508";
            string url2 = "http://www.amazon.co.jp/gp/bestsellers/english-books/2604956051/ref=zg_bs_nav_fb_1_fb";

            Thread USA_Thread = new Thread(() => WorkThreadFunction(url, "US Books"));
            USA_Thread.Priority = ThreadPriority.Highest;
            USA_Thread.Start();

            Thread JPN_Thread = new Thread(() => WorkThreadFunction(url2, "JPN Books"));
            JPN_Thread.Priority = ThreadPriority.Highest;
            JPN_Thread.Start();
            */
            /* Real code */

            string[,] urls = new string[,]{
                {"http://www.amazon.com", "/best-sellers-books-Amazon/zgbs/books/", "US Books"},
                {"http://www.amazon.co.jp", "/gp/bestsellers/english-books/", "JPN Books"},
                {"http://www.amazon.co.uk", "/gp/bestsellers/books/", "UK Books"},
                {"http://www.amazon.it", "/gp/bestsellers/books/", "IT Books"},
                {"http://www.amazon.fr", "/gp/bestsellers/english-books/", "FR Books"},
                {"http://www.amazon.de", "/gp/bestsellers/books-intl-de/", "DE Books"},
                {"http://www.amazon.es", "/gp/bestsellers/foreign-books/", "ES Books"}
            };

            int count = urls.GetLength(0);

            Thread[] threads = new Thread[count];
            ServicePoint[] servicePoints = new ServicePoint[count];

            for (int i = 0; i < count; i++)
            {
                int index = i;
                servicePoints[index] = ServicePointManager.FindServicePoint(new Uri(urls[index, 0]));
                servicePoints[index].ConnectionLimit = 5;

                string bookCategoryURL = string.Join("", urls[index, 0], urls[index, 1]);

                threads[index] = new Thread(() => WorkThreadFunction(bookCategoryURL, urls[index, 2]));
                threads[index].Priority = ThreadPriority.Highest;
                threads[index].Start();
            }

            await Task.Run(() => refreshStatus(count));

            foreach(Thread thread in threads)
            {
                thread.Join();
            }

            //USA_Thread.Join(); // make sure thread is completely finished
            //JPN_Thread.Join(); // make sure thread is completely finished

            lblBooksValue.Text = Counter.BooksAdded.ToString();

            long time = watch.ElapsedMilliseconds / 1000;

            panel2.Visible = true;
            lblTimeValue.Text = string.Format("{0} seconds", time.ToString());
            btnStart.Enabled = true;
            Counter.Reset();
        }

        private void refreshStatus(int numberOfThreads)
        {
            try
            {
                while (Counter.Finished < numberOfThreads && !this.IsDisposed)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        lblBooksValue.Text = Counter.BooksAdded.ToString(); // runs on UI thread
                        lblBooksValue.Refresh();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async void WorkThreadFunction(string url, string name)
        {
            try
            {
                // do any background work
                await StartScrape(url, name);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                Counter.IncrementFinished();
            }
        }

        private async Task StartScrape(string url, string name)
        {
            Domain domain = new Domain(url, name);

            await domain.ProcessCategory();

            try
            {
                using (StreamWriter writer = new StreamWriter(string.Format("{0}.csv", name)))
                {
                    writer.WriteLine("Category,Rank,ISBN,Title");
                    IEnumerable<Category> categoriesByName = domain.Categories.OrderBy(x => x.Name);
                    foreach (Category category in categoriesByName)
                    {
                        IEnumerable<Book> booksByRank = category.Books.OrderBy(x => x.Rank);
                        foreach (Book book in booksByRank)
                        {
                            writer.WriteLine("\"{0}\",=\"{1}\",=\"{2}\",\"{3}\"", category.Name, book.Rank, book.ISBN, book.Title);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Log(string.Format("Error creating output file for {0}", name), ex);
            }
        }
    }
}
