using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AmazonBestSellers
{
    public class Category
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public List<Book> Books { get; set; }

        public Category(string name, string url)
        {
            Name = name;
            URL = url;
            Books = new List<Book>();
        }

        public async Task<List<Category>> RetrieveCategoryData(int qAboveFold, int qPage)
        {
            List<Category> subCategories = new List<Category>();
            try
            {
                string url = string.Format("{0}?_encoding=UTF8&pg={1}&ajax=1&isAboveTheFold={2}", URL, qPage, qAboveFold);

                string html = await DownloadHtmlPage(url);

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

                doc.LoadHtml(html);
                var root = doc.DocumentNode;
                var itemLinks = root.Descendants("a").Where(n => n.ParentNode.GetAttributeValue("class", "").Equals("zg_title"));

                int rank = ((qPage - 1) * 20) + 1;

                if(qAboveFold == 0)
                {
                    rank += 3; // there are only 3 items on the first ajax page, hopefully that is true for every category
                    // consider using the rank number field on the html page
                }

                foreach (HtmlNode node in itemLinks)
                {
                    string link = node.GetAttributeValue("href", "").Trim();
                    string ISBN = link.Split(new string[] { "/dp/" }, StringSplitOptions.None)[1].Split(new string[] { "/" }, StringSplitOptions.None)[0];
                    string title = node.InnerText;
                    Book book = new Book(rank, title, ISBN, link);

                    Books.Add(book);
                    Counter.IncrementBooksAdded();

                    rank++;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Failed to process page {0} of URL: {1}", qPage, URL), ex);
            }

            return subCategories;
        }
        public async Task<List<Category>> RetrieveCategoryData(int qPage)
        {
            List<Category> subCategories = new List<Category>();
            try
            {
                string url = string.Format("{0}?pg={1}", URL, qPage);

                string html = await DownloadHtmlPage(url);

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

                doc.LoadHtml(html);
                var root = doc.DocumentNode;
                var itemLinks = root.Descendants("a").Where(n => n.ParentNode.GetAttributeValue("class", "").Equals("zg_title"));

                int rank = 1;

                foreach (HtmlNode node in itemLinks)
                {
                    string link = node.GetAttributeValue("href", "").Trim();
                    string ISBN = link.Split(new string[] { "/dp/" }, StringSplitOptions.None)[1].Split(new string[] { "/" }, StringSplitOptions.None)[0];
                    string title = node.InnerText;
                    Book book = new Book(rank, title, ISBN, link);

                    Books.Add(book);
                    Counter.IncrementBooksAdded();

                    rank++;
                }

                HtmlNode categoryElement = doc.GetElementbyId("zg_browseRoot");

                HtmlNode lastUlElement = categoryElement.Descendants("ul").Last();

                bool hasSubCategories = !lastUlElement.Descendants().Any(n => n.GetAttributeValue("class", "").Equals("zg_selected"));

                if (hasSubCategories)
                {
                    IEnumerable<HtmlNode> aElements = lastUlElement.Descendants().Where(n => n.OriginalName == "a");

                    foreach (HtmlNode aElement in aElements)
                    {
                        string link = aElement.GetAttributeValue("href", "").Trim();
                        string name = string.Format("{0} > {1}", Name, aElement.InnerText);
                        Category category = new Category(name, link);
                        subCategories.Add(category);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Log(string.Format("Failed to process page {0} of URL: {1}", qPage, URL), ex);
            }

            return subCategories;
        }
        private async Task<string> DownloadHtmlPage(string url)
        {
            string text = null;
            int attempts = 0;
            while (attempts < 5 && text == null)
            {
                try
                {
                    attempts++;
                    using (GZipWebClient gZipWebClient = new GZipWebClient())
                    {
                        text = await gZipWebClient.DownloadStringTaskAsync(url);
                    }
                }
                catch (Exception ex)
                {
                    if (attempts == 1)
                    {
                        Logger.Log(ex);
                    }
                }
                if (attempts == 5)
                {
                    throw new Exception("Attempts exceeded 5");
                }
            }
            return text;
        }
    }
}
