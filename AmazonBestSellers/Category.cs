/* Copyright (c) David T Robertson 2016 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Text;

namespace AmazonBestSellers
{
    /// <summary>
    /// Represents a category
    /// </summary>
    public class Category
    {
        private string _name;
        private string _URL;
        private Book[] _books;

        private static readonly string XPathItemLinks = "//div[@class='zg_title']//a";
        private static readonly string XPathPrice = "..//..//*[@class='price']";
        private static readonly string XPathAvailability = "..//..//div[@class='zg_availability']";

        private static readonly string OutOfStock = "Currently out of stock";
        private static readonly string CurrentlyUnavailable = "Currently unavailable";
        private static readonly string OutOfStockJPN = "現在在庫切れです。";
        private static readonly string CurrentlyUnavailableJPN = "現在お取り扱いできません";

        public string Name
        {
            get
            {
                return _name;
            }
        }
        public string URL
        {
            get
            {
                return _URL;
            }
        }
        public Book[] Books
        {
            get
            {
                return _books;
            }
        }

        public Category(string name, string url)
        {
            _name = name;
            _URL = url;
            _books = new Book[100]; // this assumes there are never more than 100 books
        }

        /// <summary>
        /// Scrapes all book data from a page within this category. For each category, books are collected from 9 total URLs. 1 URL for page 1 and 8 URLs for pages 2 thru 5. Even  
        /// though more URLs are requested for pages 2 thru 5, the total bandwidth consumed is still lower because each ajax page is very small compared to a normal page.
        /// </summary>
        /// <param name="qPage">The page number used in the query string.</param>
        /// <param name="qAboveFold">Pages 2 through 5 are retrieved via Amazon's ajax urls. But they are divided into two sub pages per page. This query string
        /// indicates which sub page is being retrieved.</param>
        /// <returns>An enumerable list of sub categories found on the page. Subcategories are only checked on page 1. For the other pages, null is returned.</returns>
        public async Task<IEnumerable<Category>> RetrieveCategoryData(int qPage, int? qAboveFold = null)
        {
            try
            {
                string url;
                if(qPage == 1)
                {
                    url = string.Format("{0}?pg=1", _URL); // page 1 we get full page so we can get the sub categories
                }
                else
                {
                    url = string.Format("{0}?_encoding=UTF8&pg={1}&ajax=1&isAboveTheFold={2}", _URL, qPage, qAboveFold); // ajax page
                }

                HtmlDocument doc = null;
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
                                // get encoding. todo: consider setting the encoding based on the url, instead of doing all this work
                                Encoding encoding = null;
                                try
                                {
                                    var responseHeaders = gZipWebClient.ResponseHeaders.GetValues("Content-Type");
                                    var encodingString = responseHeaders[0].Split('=').LastOrDefault();
                                    if (!string.IsNullOrWhiteSpace(encodingString))
                                    {
                                        encoding = Encoding.GetEncoding(encodingString);
                                    }
                                }
                                catch {}
                                if(encoding == null)
                                {
                                    encoding = Encoding.GetEncoding("ISO-8859-1");
                                }
                                doc = new HtmlDocument();
                                doc.Load(stream, encoding);
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
                var itemLinks = doc.DocumentNode.SelectNodes(XPathItemLinks); // determine all the books on the page by checking for this html

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
                        string price = "N/A";
                        HtmlNode priceNode = node.SelectSingleNode(XPathPrice);
                        if(priceNode != null)
                        {
                            price = priceNode.InnerText;
                        }
                        else
                        {
                            // no price displayed, check availability
                            HtmlNode availNode = node.SelectSingleNode(XPathAvailability);
                            if (availNode != null)
                            {
                                // translate Japanese text
                                if (availNode.InnerText == OutOfStockJPN)
                                {
                                    price = OutOfStock;
                                }
                                else if (availNode.InnerText == CurrentlyUnavailableJPN)
                                {
                                    price = CurrentlyUnavailable;
                                }
                                else
                                {
                                    price = availNode.InnerText;
                                }
                            }
                        }
                        string link = node.GetAttributeValue("href", "").Trim();
                        string ISBN = link.Split(new string[] { "/dp/" }, StringSplitOptions.None)[1].Split('/')[0]; // parse the link to get the ISBN
                        string title = node.InnerText;

                        Books[rank - 1] = new Book(title, ISBN, price);
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
                            select new Category(string.Format("{0} > {1}", _name, aElement.InnerText), aElement.GetAttributeValue("href", "").Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Failed to process page {0} of URL: {1}", qPage, _URL), ex);
            }

            return null;
        }
    }
}
