using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBestSellers
{
    public class Book
    {
        public int Rank { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public string URL { get; set; }

        public Book(int rank, string title, string isbn, string url)
        {
            Rank = rank;
            Title = title;
            ISBN = isbn;
            URL = url;
        }
    }
}
