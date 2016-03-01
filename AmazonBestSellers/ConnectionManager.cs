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
    /// <summary>
    /// This class will setup service points and will also redistribute connections to other service points once a service point is no longer in use.
    /// </summary>
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

        /// <summary>With this logic, the number of total connections in use (or at least the limit) will remain the same
        /// even after a domain is finished processing. This is not a vital part of the overall process but it can help.
        /// </summary>
        /// <param name="uri">The Uri of the domain no longer in use.</param>
        public static void RemoveAndDistributeConnections(Uri uri)
        {
            try
            {
                lock (locker)
                {
                    ServicePoint sp = servicePoints.First(x => x.Address == uri);

                    int connectionsFree = sp.ConnectionLimit; // determine how many connections are no longer in use

                    servicePoints.Remove(sp);
                    sp.ConnectionLimit = 1;

                    if(servicePoints.Count != 0 && connectionsFree != maxConnections)
                    {
                        int availableConnections = connectionsFree / servicePoints.Count; // determine how many we can add to each service point still in use

                        foreach (ServicePoint servicePoint in servicePoints)
                        {
                            if (connectionsFree + availableConnections >= maxConnections)
                            {
                                servicePoint.ConnectionLimit = maxConnections; // set to maximum
                            }
                            else
                            {
                                servicePoint.ConnectionLimit += availableConnections; // add connections
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
