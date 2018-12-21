using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SimpleHTTPServer
{
    class HTTPServer
    {
        private ServerProperties properties;

        public ServerProperties Properties
        {
            get { return properties; }
            private set { properties = value; }
        }

        HttpListener listener;
        private bool isLive = false;

        public HTTPServer(List<string> aliases,string rootPath, string indexFile, string error404File = "", bool afi = false)
        {
            Properties = new ServerProperties(aliases, rootPath, indexFile, error404File, afi);
        }

        public void StartListening()
        {
            if (isLive)
            {
                Logger.Log("Listener Already Running!");
                throw new Exception("Listener Already Running!");
            }
            else if (!HttpListener.IsSupported)
            {
                Logger.Log("OS Not Supported!");
                throw new Exception("OS Not Supported!");
            }

            if (Properties.Aliases == null || Properties.Aliases.Count() == 0)
            {
                Logger.Log("You must add an Alias/Prefix!");
                throw new ArgumentException("You must add an Alias/Prefix!");
            }

            listener = new HttpListener();
            RequestHandler requestHandler = new RequestHandler(listener, this);
            
            try
            {
                foreach (string alias in Properties.Aliases)
                {
                    listener.Prefixes.Add(alias);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
                Logger.Log("Invalid Alias(es)/Prefix(es), Example: http://localhost:8080");
                throw new ArgumentException("Invalid Alias(es)/Prefix(es), Example: http://localhost:8080");
            }

            try
            {
                listener.Start();
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
                Logger.Log("Generic Access Error: One of three things happened:");
                Logger.Log("    1: Firewall blocked this process.");
                Logger.Log("    2: Bound IP:Port is reserved by another process/service.");
                Logger.Log("    3: Something else.");
                throw;
            }


            Logger.Log("Listening...");
            Logger.Log("Root Path: " + Properties.RootPath);
            isLive = true;

            IAsyncResult Result = listener.BeginGetContext(requestHandler.OnContextRequest, listener);

        }

        public void StopListening()
        {
            if (isLive)
            {
                isLive = false;
                listener.Stop();
            }
            else
            {
                Logger.Log("Listener has not been started!");
            }
        }
        
    }
}
