/* Copyright (c) David Robertson 2016 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;

namespace AmazonBestSellers
{
    public class DomainSlim
    {
        private string _URL;
        private List<string> _ISBNs;
        private object locker = new object();

        public DomainSlim(string url)
        {
            _URL = url;
            _ISBNs = new List<string>();
        }

        public async Task ProcessCategory()
        {
            try
            {
                List<Task<IEnumerable<string>>> downloadTasks = new List<Task<IEnumerable<string>>>();

                for (int page = 1; page <= 5; page++)
                {
                    if (page == 1)
                    {
                        downloadTasks.Add(RetrieveCategoryData(_URL, page));
                    }
                    else
                    {
                        downloadTasks.Add(RetrieveCategoryData(_URL, page, 0));
                        downloadTasks.Add(RetrieveCategoryData(_URL, page, 1));
                    }
                }

                while (downloadTasks.Count > 0)
                {
                    Task<IEnumerable<string>> firstFinishedTask = await Task.WhenAny(downloadTasks);

                    downloadTasks.Remove(firstFinishedTask);

                    var result = firstFinishedTask.Result;
                    firstFinishedTask.Dispose();

                    if (result != null)
                    {
                        var subCategories = result.ToList();

                        for (int page = 5; page >= 1; --page)
                        {
                            foreach (string categoryURL in subCategories)
                            {
                                if (page == 1)
                                {
                                    downloadTasks.Add(RetrieveCategoryData(categoryURL, page));
                                }
                                else
                                {
                                    downloadTasks.Add(RetrieveCategoryData(categoryURL, page, 0));
                                    downloadTasks.Add(RetrieveCategoryData(categoryURL, page, 1));
                                }
                            }
                        }
                    }
                    // flush output
                    if(_ISBNs.Count > 500)
                    {
                        lock (locker)
                        {
                            Writer.WriteToFile(_ISBNs.GetRange(0, 500));
                            _ISBNs.RemoveRange(0, 500);
                        }
                    }
                }
                if (_ISBNs.Count > 0)
                {
                    lock (locker)
                    {
                        Writer.WriteToFile(_ISBNs);
                    }
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Error retrieving categories for {0}", _URL), ex);
            }
        }

        private async Task<IEnumerable<string>> RetrieveCategoryData(string categoryURL, int qPage, int? qAboveFold = null)
        {
            try
            {
                string url;
                if (qPage == 1)
                {
                    url = string.Format("{0}?pg=1", categoryURL); // page 1 we get full page so we can get the sub categories
                }
                else
                {
                    url = string.Format("{0}?_encoding=UTF8&pg={1}&ajax=1&isAboveTheFold={2}", categoryURL, qPage, qAboveFold); // ajax page
                }

                HtmlDocument doc = new HtmlDocument();
                bool loaded = false;
                int attempts = 0;
                while (attempts < 5 && loaded == false)
                {
                    try
                    {
                        attempts++;
                        using (GZipWebClient gZipWebClient = new GZipWebClient())
                        {
                            using (Stream stream = await gZipWebClient.OpenReadTaskAsync(url))
                            {
                                doc.Load(stream, System.Text.Encoding.GetEncoding("ISO-8859-1"));
                            }
                        }
                        loaded = true; // if we get here with no exception, then it was loaded successfully
                    }
                    catch (Exception ex)
                    {
#if DEBUG               
                        if (attempts == 1)
                        {
                            Logger.Log(string.Format("Error downloading page. Attemping to retry... URL: {0}", url), ex);
                        }
#endif
                        if (attempts == 5 && loaded == false)
                        {
                            throw new Exception("Attempts exceeded 5", ex);
                        }
                    }
                }
                var itemLinks = doc.DocumentNode.SelectNodes("//div[@class='zg_title']//a");

                if (itemLinks != null)
                {
                    int tempBooks = 0;
                    StringBuilder tempStrBuilder = new StringBuilder();

                    foreach (HtmlNode node in itemLinks)
                    {
                        string link = node.GetAttributeValue("href", "").Trim();
                        string ISBN = link.Split(new string[] { "/dp/" }, StringSplitOptions.None)[1].Split('/')[0];

                        tempStrBuilder.AppendLine(ISBN);
                        tempBooks++;
                    }
                    Counter.IncrementBooksAdded(tempBooks);
                    lock (locker)
                    {
                        _ISBNs.Add(tempStrBuilder.ToString());
                    }
                }

                /*
                // this code checks for missing books. Amazon will sometimes incorrectly deliver a page in which there will be a rank number but no item.
                List<HtmlNode> rankDivs = root.Descendants("span").Where(n => n.GetAttributeValue("class", "").Equals("zg_rankNumber")).ToList<HtmlNode>();
                if(rankDivs.Count != tempBooks.Count)
                {
                    System.Diagnostics.Debug.WriteLine("Missing book(s) at " + url);
                }
                */
                if (qPage == 1) // check for subcategories if page is 1
                {
                    HtmlNode lastUlElement = doc.GetElementbyId("zg_browseRoot").Descendants("ul").Last();

                    bool hasSubCategories = !lastUlElement.Descendants().Any(n => n.GetAttributeValue("class", "").Equals("zg_selected"));

                    if (hasSubCategories)
                    {
                        IEnumerable<HtmlNode> aElements = lastUlElement.Descendants().Where(n => n.OriginalName == "a");

                        return
                            from aElement in aElements
                            select aElement.GetAttributeValue("href", "").Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Failed to process page {0} of URL: {1}", qPage, categoryURL), ex);
            }

            return null;
        }
    }
}
