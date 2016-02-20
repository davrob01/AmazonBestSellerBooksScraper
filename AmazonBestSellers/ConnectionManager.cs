using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace AmazonBestSellers
{
    public static class ConnectionManager
    {
        private static int connectionsPerDomain;
        private static List<ServicePoint> servicePoints;
        private static object locker = new object();
        private const int maxConnections = 100;
 
        static ConnectionManager()
        {
            // establish general settings
            ServicePointManager.SetTcpKeepAlive(true, 10000, 10000);
            ServicePointManager.MaxServicePointIdleTime = 10000;
            ServicePointManager.UseNagleAlgorithm = true;
            //ServicePointManager.DefaultConnectionLimit = 1000;

            servicePoints = new List<ServicePoint>();
            connectionsPerDomain = 40;
        }

        public static void AddConnection(Uri uri)
        {
            lock (locker)
            {
                ServicePoint sp = ServicePointManager.FindServicePoint(uri);
                sp.ConnectionLimit = connectionsPerDomain;
                servicePoints.Add(sp);
            }
        }

        public static void RemoveAndDistributeConnections(Uri uri)
        {
            try
            {
                lock (locker)
                {
                    ServicePoint sp = servicePoints.First(x => x.Address == uri);

                    int connectionsFree = sp.ConnectionLimit;

                    servicePoints.Remove(sp);
                    sp.ConnectionLimit = 1;

                    if(servicePoints.Count != 0 && connectionsFree != maxConnections)
                    {
                        int availableConnections = connectionsFree / servicePoints.Count;

                        foreach (ServicePoint servicePoint in servicePoints)
                        {
                            if (connectionsFree + availableConnections >= maxConnections)
                            {
                                servicePoint.ConnectionLimit = maxConnections; //set to maximum
                            }
                            else
                            {
                                servicePoint.ConnectionLimit += availableConnections;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Log("Error re-distributing connections", ex);
            }
        }

        public static void Reset()
        {
            servicePoints.Clear();
        }
    }
}
