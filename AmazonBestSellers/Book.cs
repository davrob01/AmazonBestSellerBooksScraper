using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBestSellers
{
    public class Book
    {
        public string Title { get; set; }
        public string ISBN { get; set; }
        public string URL { get; set; }

        public Book(string title, string isbn, string url)
        {
            Title = title;
            ISBN = isbn;
            URL = url;
        }
    }
}
