/* Copyright (c) David T Robertson 2016 */
using System;

namespace AmazonBestSellers
{
    /// <summary>
    /// Represents a book
    /// </summary>
    public class Book
    {
        private string _title;
        private string _author;
        private string _ISBN;
        private string _price;
        private string _edition;
        private string _imageURL;

        public string Title
        {
            get
            {
                return _title;
            }
        }
        public string Author
        {
            get
            {
                return _author;
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

        public string Edition
        {
            get
            {
                return _edition;
            }
        }

        public string ImageURL
        {
            get
            {
                return _imageURL;
            }
        }

        public Book(string title, string author, string isbn, string price, string edition, string imageURL)
        {
            _title = title;
            _author = author;
            _ISBN = isbn;
            _price = price;
            _edition = edition;
            _imageURL = imageURL;
        }
    }
}
