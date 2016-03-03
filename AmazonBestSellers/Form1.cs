/* Copyright (c) David T Robertson 2016 */
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
    /// <summary>
    /// The main form used by the program.
    /// </summary>
    public partial class Form1 : Form
    {
        private object locker = new object();

        private DateTime datetime;
        private string fileName1;
        private string fileName2;
        private bool autoStart;

        public static string outputDirectory;

        public Form1(bool autoStart)
        {
            InitializeComponent();
            outputDirectory = "Results\\";
            this.autoStart = autoStart;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (autoStart)
            {
                btnStart.PerformClick();
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                outputDirectory = "Results\\";
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
                MessageBox.Show(string.Format("An Error has occured. {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates the UI to reflect the status of the overall process. Status is expressed by displaying the number of books currently added.
        /// </summary>
        private void refreshStatus(int numberOfThreads)
        {
            try
            {
                while (Counter.Finished < numberOfThreads && !this.IsDisposed)
                {
                    new System.Threading.ManualResetEvent(false).WaitOne(1000); // wait 1 second
                    this.Invoke((MethodInvoker)delegate
                    {
                        lblBooksValue.Text = Counter.BooksAdded.ToString(); // runs on UI thread
                        lblBooksValue.Refresh(); // update label
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        /// <summary>
        /// Starts the scraping process for a domain. Also outputs details to a CSV if that option is checked in the form.
        /// </summary>
        private async Task StartScrape(string url, string name, Uri domainUri)
        {
            try
            {
                if (chkDetail.Checked)
                {
                    Domain domain = new Domain(url, name);

                    await domain.ProcessCategory();

                    // set encodings and currency symbols
                    string priceColumnHeader = "Price";
                    string symbolToFind = "";
                    Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
                    switch (domainUri.Host)
                    {
                        case "www.amazon.co.jp":
                            priceColumnHeader = "Price ¥"; // proper yen character, yes there is a difference
                            symbolToFind = "￥ ";
                            break;
                        case "www.amazon.fr":
                        case "www.amazon.de":
                        case "www.amazon.es":
                        case "www.amazon.it":
                            priceColumnHeader = "Price EUR";
                            symbolToFind = "EUR ";
                            encoding = Encoding.GetEncoding("ISO-8859-15");
                            break;
                        case "www.amazon.co.uk":
                            priceColumnHeader = "Price £";
                            symbolToFind = "£";
                            break;
                    }

                    lock (locker)
                    {
                        using (StreamWriter writerISBN = new StreamWriter(fileName1, true))
                        using (StreamWriter writer = new StreamWriter(string.Format("{0}{1}{2}", outputDirectory, name, fileName2), false, encoding))
                        {
                            writer.WriteLine(string.Format("Category,Rank,ISBN,{0},Title", priceColumnHeader));
                            IEnumerable<Category> categoriesByName = domain.Categories.OrderBy(x => x.Name);
                            foreach (Category category in categoriesByName)
                            {
                                for (int index = 0; index < 100; index++)
                                {
                                    Book currentBook = category.Books[index];
                                    if (currentBook != null)
                                    {
                                        string price = currentBook.Price;
                                        // remove currency symbols
                                        if (symbolToFind != "")
                                        {
                                            price = price.Replace(symbolToFind, "");
                                            if(symbolToFind == "EUR ")
                                            {
                                                // show as US decimal
                                                var lastCommaIndex = price.LastIndexOf(',');
                                                price = price.Replace(".", ","); // replace periods with commas (Europe uses dots where US normally uses commas)
                                                // replace last comma with period (decimal point)
                                                if(lastCommaIndex != -1)
                                                {
                                                    var charArray = price.ToCharArray();
                                                    charArray[lastCommaIndex] = '.';
                                                    price = new string(charArray);
                                                }
                                            }
                                        }
                                        writer.WriteLine("\"{0}\",\"{1}\",=\"{2}\",\"{3}\",\"{4}\"", category.Name, index + 1, currentBook.ISBN, price, currentBook.Title);

                                        writerISBN.WriteLine(currentBook.ISBN);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // short output with just ISBNs
                    DomainSlim domainSlim = new DomainSlim(url);
                    await domainSlim.ProcessCategory();
                }
            }
            catch(FileNotFoundException ex)
            {
                Logger.Log(ex);
                throw ex;
            }
            catch(Exception ex)
            {
                Logger.Log(string.Format("Error creating output for {0}", name), ex);
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
                    outputDirectory = "Results\\Test\\";
                    string[,] urls = new string[,]{
                        {"http://www.amazon.com", "/Best-Sellers-Books-Arts-Photography/zgbs/books/1/", "US Books - Arts & Photography"},
                        {"http://www.amazon.co.jp", "/gp/bestsellers/english-books/2634770051/", "JPN Books - Computers & Technology"},
                        {"http://www.amazon.co.uk", "/Best-Sellers-Books-Sports-Hobbies-Games/zgbs/books/55/", "UK Books - Sports, Hobbies & Games"},
                        {"http://www.amazon.it", "/gp/bestsellers/books/508745031/", "IT Books - Religione e spiritualita"},
                        {"http://www.amazon.fr", "/gp/bestsellers/english-books/80179011/", "FR Books - Health, Mind & Body"},
                        {"http://www.amazon.de", "/gp/bestsellers/books-intl-de/65108011/", "DE Books - Outdoor, Umwelt & Natur"},
                        {"http://www.amazon.es", "/gp/bestsellers/foreign-books/903313031/", "ES Books - Deporte"}
                    };

                    await StartProcess(urls);
                }
            }
            catch(Exception ex)
            {
                Logger.Log("Error in UI thread", ex);
                MessageBox.Show(string.Format("An Error has occured. {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Starts all the scraping tasks, one for each domain.
        /// </summary>
        /// <param name="qPage">The list of the urls of the domains to scrape.</param>
        /// <returns>A task that completes when all the scraping tasks have completed and all data has been outputted.</returns>
        private async Task StartProcess(string[,] urls)
        {
            panel2.Visible = false;
            DisableButtons();
            Mutex mutex = null;
            try
            {
                // only allow one process at a time
                bool result;
                mutex = new Mutex(true, "D-ROB Software/ AmazonBestSellerBooksScraper", out result);
                if (!result)
                {
                    lblStatus.Text = "Closing...";
                    MessageBox.Show("Another instance of the Amazon Best Seller Books Scraper is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    Application.Exit();
                    return;
                }
            
                if(chkDetail.Checked)
                {
                    PrepareOutputFiles();
                }
                var watch = Stopwatch.StartNew();

                int count = urls.GetLength(0); // determine number of domains

                Task[] tasks = new Task[count];

                for (int i = 0; i < count; i++ )
                {
                    int index = i;
                    Uri domainUri = new Uri(urls[index, 0]);
                    ConnectionManager.AddConnection(domainUri);

                    string bookCategoryURL = string.Join("", urls[index, 0], urls[index, 1]);
                    tasks[index] = Task.Factory.StartNew(() => StartScrape(bookCategoryURL, urls[index, 2], domainUri)).Result;
                }

                await Task.Run(() => refreshStatus(count));

                await Task.WhenAll(tasks);

                lblBooksValue.Text = Counter.BooksAdded.ToString();

                long time = watch.ElapsedMilliseconds / 1000;

                panel2.Visible = true;
                lblTimeValue.Text = string.Format("{0} seconds", time.ToString());
                Counter.Reset();
                GC.KeepAlive(mutex);                // mutex shouldn't be released - important line
            }
            catch (Exception ex)
            {
                Logger.Log("Error in UI thread", ex);
                MessageBox.Show(string.Format("An Error has occured. {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.Close();
                }
            }
            EnableButtons();
            Counter.Reset();
            ConnectionManager.Reset();
        }
        
        private void DisableButtons()
        {
            chkDetail.Enabled = false;
            btnStart.Enabled = false;
            btnTest.Enabled = false;
            lblStatus.Text = "Scraping in progress...";
            lblStatus.Refresh();
        }
        private void EnableButtons()
        {
            chkDetail.Enabled = true;
            btnStart.Enabled = true;
            btnTest.Enabled = true;
            lblStatus.Text = "";
            lblStatus.Refresh();
        }
        private void PrepareOutputFiles()
        {
            try
            {
                datetime = DateTime.Now;
                string formatedDate = datetime.ToString("MM.dd.yy H.mm.ss");
                fileName1 = string.Format("{0}All_ISBN_{1}.txt", outputDirectory, formatedDate);
                fileName2 = string.Format("_{0}.csv", formatedDate);
                (new FileInfo(fileName1)).Directory.Create();
            }
            catch(Exception ex)
            {
                throw new Exception("Could not prepare output files. Be sure you have write permissions to the starting directory.", ex);
            }
        }
    }
}
