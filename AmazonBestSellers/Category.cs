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

        private static readonly string XPathItemLinks = "//a[@class='a-link-normal' and not(contains(@href,'/product-reviews/'))]"; // we have to exclude links for product review pages
        private static readonly string XPathPrice = "..//*[contains(@class,'price')]";
        private static readonly string XPathAvailability = "..//*[contains(@class,'avail')]";
        private static readonly string XPathAuthor = "./following-sibling::*[1][@class='a-row a-size-small' and ./*[not(contains(@class,'a-color-secondary'))]]";

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
        /// December 2016 Update: Ajax URLs used for pages 2 thru 5 now display all 20 items - Amazon is no longer using the 'isAboveTheFold' query string to divide items into
        /// sub pages for the ajax pages, except in the Japan domain.
        /// </summary>
        /// <param name="qPage">The page number used in the query string.</param>
        /// <param name="qAboveFold">Pages 2 through 5 are retrieved via Amazon's ajax urls. But they are divided into two sub pages per page (only applies to Japan domain). This query string
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
                else if (qAboveFold == null)
                {
                    url = string.Format("{0}?_encoding=UTF8&pg={1}&ajax=1", _URL, qPage); // ajax page
                }
                else
                {
                    url = string.Format("{0}?_encoding=UTF8&pg={1}&ajax=1&isAboveTheFold={2}", _URL, qPage, qAboveFold); // ajax page
                }

                HtmlDocument doc = null;
                bool loaded = false;
                int attempts = 0;
                HtmlNodeCollection itemLinks = null;
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
                        // delete nodes with extra links we do not want
                        // they are links from the "More to Explore" section of the page found in the div with id='zg_col2'
                        if(qPage == 1)
                        {
                            var removeNode = doc.DocumentNode.SelectSingleNode("//div[@id='zg_col2']");
                            if(removeNode != null)
                            {
                                removeNode.Remove();
                            }
                            else
                            {
                                var removeNodes = doc.DocumentNode.SelectNodes("//div[@class='zg_more']"); // alternative method in case 'zg_col2' is not found
                                foreach (HtmlNode node in removeNodes)
                                {
                                    node.Remove();
                                }
                            }
                        }

                        itemLinks = doc.DocumentNode.SelectNodes(XPathItemLinks); // determine all the books on the page by checking for this html
                        if(itemLinks == null)
                        {
                            // no books found... did we land on a Captcha page?
                            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
                            if (titleNode != null && (titleNode.InnerText.ToUpper().Contains("AMAZON CAPTCHA") || titleNode.InnerText.ToUpper().Contains("BOT CHECK")))
                                throw new Exception("No books found. Landed on Captcha");
                            else
                            {
                                // we only throw an exception if the page is 1
                                // because on other pages it is possible that there really are no books because not all sub categories have 100 books
                                if(qPage == 1)
                                    throw new Exception("No books found on page");
                                else
                                    return null; // do not attempt to retry or show error if the page greater than 1
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
                    string[] split = link.Split(new string[] { "/ref=" }, StringSplitOptions.None)[0].Split('/');
                    string ISBN = split[split.Length - 1]; // parse the link to get the ISBN
                    string title = "";
                    var fullTitleNode = node.SelectSingleNode(".//span[@title]");
                    if(fullTitleNode != null)
                    {
                        title = fullTitleNode.GetAttributeValue("title", "").Trim();
                    }
                    else
                    {
                        title = node.InnerText.Trim();
                    }
                    // get author if possible
                    string author = "N/A";
                    var authorNode = node.SelectSingleNode(XPathAuthor);
                    if (authorNode != null)
                    {
                        author = authorNode.InnerText;
                    }
                    Books[rank - 1] = new Book(title, author, ISBN, price);
                    tempBooks++;

                    rank++;
                }
                Counter.IncrementBooksAdded(tempBooks);
                
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
