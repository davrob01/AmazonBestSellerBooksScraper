using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;

namespace AmazonBestSellers
{
    public class Category
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public Book[] Books { get; set;}
        private static object locker = new object();

        public Category(string name, string url)
        {
            Name = name;
            URL = url;
            Books = new Book[100]; // this assumes there are never more than 100 books
        }

        public async Task<IEnumerable<Category>> RetrieveCategoryData(int qPage, int? qAboveFold = null)
        {
            try
            {
                string url;
                if(qPage == 1)
                {
                    url = string.Format("{0}?pg=1", URL); // page 1 we get full page so we can get the sub categories
                }
                else
                {
                    url = string.Format("{0}?_encoding=UTF8&pg={1}&ajax=1&isAboveTheFold={2}", URL, qPage, qAboveFold); // ajax page
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
                            using(Stream stream = await gZipWebClient.OpenReadTaskAsync(url))
                            {
                                doc.Load(stream, System.Text.Encoding.GetEncoding("ISO-8859-1"));
                            }
                        }
                        loaded = true; // if we get here with no exception, then it was loaded successfully
                    }
                    catch (Exception ex)
                    {
                        if (attempts == 1)
                        {
                            Logger.Log(string.Format("Error downloading page. Attemping to retry... URL: {0}", url), ex);
                        }
                    }
                    if (attempts == 5 && loaded == false)
                    {
                        throw new Exception("Attempts exceeded 5");
                    }
                }
                var itemLinks = doc.DocumentNode.SelectNodes("//div[@class='zg_title']//a");

                if(itemLinks != null)
                {
                    int rank = 1;
                    if (qPage > 1) // rank starting number will be different for other pages
                    {
                        rank = ((qPage - 1) * 20) + 1;

                        if (qAboveFold == 0)
                        {
                            rank += 3; // there are only 3 items on the first ajax page, hopefully that is true for every category
                            // consider using the rank number field on the html page
                        }
                    }
                    int tempBooks = 0;

                    foreach (HtmlNode node in itemLinks)
                    {
                        string link = node.GetAttributeValue("href", "").Trim();
                        string ISBN = link.Split(new string[] { "/dp/" }, StringSplitOptions.None)[1].Split('/')[0];
                        string title = node.InnerText;

                        Books[rank - 1] = new Book(title, ISBN, link);
                        tempBooks++;

                        rank++;
                    }
                    Counter.IncrementBooksAdded(tempBooks);
                }
                
                /*
                // this code checks for missing books. Amazon will sometimes incorrectly deliver a page in which there will be a rank number but no item.
                List<HtmlNode> rankDivs = root.Descendants("span").Where(n => n.GetAttributeValue("class", "").Equals("zg_rankNumber")).ToList<HtmlNode>();
                if(rankDivs.Count != tempBooks.Count)
                {
                    System.Diagnostics.Debug.WriteLine("Missing book(s) at " + url);
                }
                */
                if(qPage == 1) // check for subcategories if page is 1
                {
                    HtmlNode lastUlElement = doc.GetElementbyId("zg_browseRoot").Descendants("ul").Last();

                    bool hasSubCategories = !lastUlElement.Descendants().Any(n => n.GetAttributeValue("class", "").Equals("zg_selected"));

                    if (hasSubCategories)
                    {
                        IEnumerable<HtmlNode> aElements = lastUlElement.Descendants().Where(n => n.OriginalName == "a");

                        return
                            from aElement in aElements
                            select new Category(string.Format("{0} > {1}", Name, aElement.InnerText), aElement.GetAttributeValue("href", "").Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Failed to process page {0} of URL: {1}", qPage, URL), ex);
            }

            return null;
        }
    }
}
