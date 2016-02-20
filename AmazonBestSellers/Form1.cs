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
        private object locker = new object();

        private DateTime datetime;
        private string fileName1;
        private string fileName2;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                string[,] urls = new string[,]{
                    {"http://www.amazon.com", "/best-sellers-books-Amazon/zgbs/books/", "US Books"},
                    {"http://www.amazon.co.jp", "/gp/bestsellers/english-books/", "JPN Books"},
                    {"http://www.amazon.co.uk", "/gp/bestsellers/books/", "UK Books"},
                    {"http://www.amazon.it", "/gp/bestsellers/books/", "IT Books"},
                    {"http://www.amazon.fr", "/gp/bestsellers/english-books/", "FR Books"},
                    {"http://www.amazon.de", "/gp/bestsellers/books-intl-de/", "DE Books"},
                    {"http://www.amazon.es", "/gp/bestsellers/foreign-books/", "ES Books"}
                };

                await StartProcess(urls);
            }
            catch (Exception ex)
            {
                Logger.Log("Error in UI thread", ex);
                MessageBox.Show(string.Format("An Error has occured. {0}", ex.Message));
            }
        }

        private void refreshStatus(int numberOfThreads)
        {
            try
            {
                while (Counter.Finished < numberOfThreads && !this.IsDisposed)
                {
                    Thread.Sleep(1000);
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

        private async void StartScrape(string url, string name, Uri domainUri)
        {
            try
            {
                Domain domain = new Domain(url, name);

                await domain.ProcessCategory();
            
                lock (locker)
                {
                    using (StreamWriter writerISBN = new StreamWriter(fileName1, true))
                    using (StreamWriter writer = new StreamWriter(fileName2, true))
                    {
                        IEnumerable<Category> categoriesByName = domain.Categories.OrderBy(x => x.Name);
                        foreach (Category category in categoriesByName)
                        {
                            for (int index = 0; index < 100; index++ )
                            {
                                Book currentBook = category.Books[index];
                                if (currentBook != null)
                                {
                                    writer.WriteLine("\"{0}\",=\"{1}\",=\"{2}\",\"{3}\"", category.Name, index + 1, currentBook.ISBN, currentBook.Title);
                                    writerISBN.WriteLine(currentBook.ISBN);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Log(string.Format("Error creating output file for {0}", name), ex);
            }
            finally
            {
                Counter.IncrementFinished();
                ConnectionManager.RemoveAndDistributeConnections(domainUri);
            }
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dialogResult = MessageBox.Show("This test will just get books from a subcateogory of each domain. Not all the books will be in the output files. Continue?", "Test Run", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    string[,] urls = new string[,]{
                        {"http://www.amazon.com", "/Best-Sellers-Books-Arts-Photography/zgbs/books/1/", "US Books"},
                        {"http://www.amazon.co.jp", "/gp/bestsellers/english-books/2634770051/", "JPN Books"},
                        {"http://www.amazon.co.uk", "/Best-Sellers-Books-Sports-Hobbies-Games/zgbs/books/55/", "UK Books"},
                        {"http://www.amazon.it", "/gp/bestsellers/books/508745031/", "IT Books"},
                        {"http://www.amazon.fr", "/gp/bestsellers/english-books/80179011/", "FR Books"},
                        {"http://www.amazon.de", "/gp/bestsellers/books-intl-de/65108011/", "DE Books"},
                        {"http://www.amazon.es", "/gp/bestsellers/foreign-books/903313031/", "ES Books"}
                    };

                    await StartProcess(urls);
                }
            }
            catch(Exception ex)
            {
                Logger.Log("Error in UI thread", ex);
                MessageBox.Show("An Error has occured");
            }
        }

        private async Task StartProcess(string[,] urls)
        {
            panel2.Visible = false;
            DisableButtons();

            try
            {
                PrepareOutputFiles();
                var watch = Stopwatch.StartNew();

                int count = urls.GetLength(0);

                Task[] tasks = new Task[count];

                for (int i = 0; i < count; i++ )
                {
                    int index = i;
                    Uri domainUri = new Uri(urls[index, 0]);
                    ConnectionManager.AddConnection(domainUri);

                    string bookCategoryURL = string.Join("", urls[index, 0], urls[index, 1]);
                    tasks[index] = Task.Factory.StartNew(() => StartScrape(bookCategoryURL, urls[index, 2], domainUri),TaskCreationOptions.LongRunning);
                }

                await Task.Run(() => refreshStatus(count));

                await Task.WhenAll(tasks);

                lblBooksValue.Text = Counter.BooksAdded.ToString();

                long time = watch.ElapsedMilliseconds / 1000;

                panel2.Visible = true;
                lblTimeValue.Text = string.Format("{0} seconds", time.ToString());
                Counter.Reset();
            }
            catch (Exception ex)
            {
                Logger.Log("Error in UI thread", ex);
                MessageBox.Show(string.Format("An Error has occured. {0}", ex.Message));
            }
            EnableButtons();
            Counter.Reset();
            ConnectionManager.Reset();
        }
        
        private void DisableButtons()
        {
            btnStart.Enabled = false;
            btnTest.Enabled = false;
        }
        private void EnableButtons()
        {
            btnStart.Enabled = true;
            btnTest.Enabled = true;
        }
        private void PrepareOutputFiles()
        {
            datetime = DateTime.Now;
            string formatedDate = datetime.ToString("MM.dd.yy H.mm.ss");
            fileName1 = string.Format("Results\\All_ISBN_{0}.txt", formatedDate);
            fileName2 = string.Format("Results\\Books_Detailed_{0}.csv", formatedDate);
            (new FileInfo(fileName1)).Directory.Create();
            File.WriteAllText(fileName1, "");
            File.WriteAllText(fileName2, string.Format("Category,Rank,ISBN,Title{0}", System.Environment.NewLine));
        }
    }
}
