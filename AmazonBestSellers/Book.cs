/* Copyright (c) David Robertson 2016 */
using System;

namespace AmazonBestSellers
{
    public class Book
    {
        private string _title;
        private string _ISBN;
        private string _URL;
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
        public string URL
        {
            get
            {
                return _URL;
            }
        }
        public string Price
        {
            get
            {
                return _price;
            }
        }

        public Book(string title, string isbn, string url, string price)
        {
            _title = title;
            _ISBN = isbn;
            _URL = url;
            _price = price;
        }
    }
}
