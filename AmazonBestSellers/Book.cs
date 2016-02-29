/* Copyright (c) David Robertson 2016 */
using System;

namespace AmazonBestSellers
{
    public class Book
    {
        private string _title;
        private string _ISBN;
        private string _price;

        public string Title
        {
            get
            {
                return _title;
            }
        }
        public string ISBN
        {
            get
            {
                return _ISBN;
            }
        }
        public string Price
        {
            get
            {
                return _price;
            }
        }

        public Book(string title, string isbn, string price)
        {
            _title = title;
            _ISBN = isbn;
            _price = price;
        }
    }
}
