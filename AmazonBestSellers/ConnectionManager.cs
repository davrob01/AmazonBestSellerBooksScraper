/* Copyright (c) David T Robertson 2016 */
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
        private static List<ServicePoint> servicePoints;
        private static object locker = new object();
        private static int connectionsPerDomain = 10;
        private const int maxConnections = 100;
 
        static ConnectionManager()
        {
            try
            {
                servicePoints = new List<ServicePoint>();

                // establish general settings
                ServicePointManager.SetTcpKeepAlive(true, 5000, 5000);
                ServicePointManager.MaxServicePointIdleTime = 5000;
                ServicePointManager.Expect100Continue = false;
                //ServicePointManager.DefaultConnectionLimit = 1000;

                try
                {
                    var connections_config = System.Configuration.ConfigurationManager.AppSettings["Connections_Per_Domain"];
                    if (!string.IsNullOrWhiteSpace(connections_config))
                    {
                        int connections;
                        if (int.TryParse(connections_config, out connections))
                        {
                            if (connections >= 2)
                            {
                                if (connections >= maxConnections)
                                {
                                    connectionsPerDomain = maxConnections;
                                }
                                else
                                {
                                    connectionsPerDomain = connections;
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.Log("Error reading config setting for Connections_Per_Domain. Using default instead.", ex);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error in connection manager initialization.", ex);
            }
        }

        public static void AddConnection(Uri uri)
        {
            lock (locker)
            {
                ServicePoint sp = ServicePointManager.FindServicePoint(uri);
                sp.ConnectionLimit = connectionsPerDomain;
                //sp.ConnectionLeaseTimeout = 30000;
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
