/* Copyright (c) David T Robertson 2016 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace AmazonBestSellers
{
    /// <summary>
    /// A custom version of the WebClient class that uses GZip compression and other settings.
    /// </summary>
    public class GZipWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.ReadWriteTimeout = 20000;
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            request.AllowAutoRedirect = false;
            request.Pipelined = true;
            request.Timeout = 50000;
            request.Proxy = null;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.109 Safari/537.36";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return request;
        }
    }
}
