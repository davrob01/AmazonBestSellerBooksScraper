using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AmazonBestSellers
{
    static class Counter
    {
        private static long _booksAdded;
        public static long BooksAdded 
        { 
            get
            {
                return _booksAdded;
            }
        }
        public static bool Finished { get; set; }

        static Counter()
        {
            Finished = false;
            _booksAdded = 0;
        }
        public static void Reset()
        {
            Finished = false;
            _booksAdded = 0;
        }
        public static void IncrementBooksAdded()
        {
            Interlocked.Increment(ref _booksAdded);
        }
    }
}
