using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBestSellers
{
    static class Counter
    {
        public static int BooksAdded { get; set; }
        public static bool Finished { get; set; }

        static Counter()
        {
            Finished = false;
            BooksAdded = 0;
        }
        public static void Reset()
        {
            Finished = false;
            BooksAdded = 0;
        }
    }
}
