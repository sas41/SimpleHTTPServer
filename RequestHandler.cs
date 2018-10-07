using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SimpleHTTPServer
{
    class RequestHandler
    {
        private HTTPServer serverReference;
        private HttpListener listener;

        public HttpListener Listener
        {
            get { return listener; }
            private set { listener = value; }
        }

        private HTTPServer ServerReference
        {
            get { return serverReference; }
            set { serverReference = value; }
        }

        public RequestHandler(HttpListener l, HTTPServer sr)
        {
            Listener = l;
            ServerReference = sr;
        }


        // This task is ran Asynchronously each time a request is made
        public void OnContextRequest(IAsyncResult result)
        {
            try
            {
                //The GetContext method blocks while waiting for a request.
                //We use EndGetContext and BeginGetContext instead.
                //As soon as we are done with the last context, we should be listening for the next.
                //That way we can reply to many at the same time.
                HttpListenerContext context = Listener.EndGetContext(result);
                Listener.BeginGetContext(OnContextRequest, Listener);
                
                //Get the specifics of the request and extract the target file/directory.
                HttpListenerRequest request = context.Request;
                string target = request.RawUrl;

                string path = TargetDecoder(target);

                string response = ResponseBuilder(path);

                SendResponse(context, response);
            }
            catch (Exception)
            {
                Logger.Log("Listener Stopped, Stopping Thread");
                return;
            }
        }


        // This "Decodes" the requested page, it looks at the path of the address, and returns the index if it's empty.
        private string TargetDecoder(string target)
        {
            //If no file/path is requested, serve the index file.
            string finalPath = "";
            if (target == "/")
            {
                finalPath = ServerReference.Properties.IndexFile;
            }
            else
            {
                finalPath = ServerReference.Properties.RootPath + target;
            }

            Logger.Log("Target Path: " + target);
            Logger.Log("Final Path: " + finalPath);

            return finalPath;
        }


        // If the path is not Empty, it get's categorized here, it returns a webpage, error404, or folder index.
        private string ResponseBuilder(string path)
        {

            if (File.Exists(path))
            {
                //responseString = FetchFile(path);
                FileAttributes attr = File.GetAttributes(path);

                if (attr.HasFlag(FileAttributes.Directory) && ServerReference.Properties.AllowFolderInexing)
                {
                    // Dir
                    Logger.Log("Requesting Folder: " + path);

                }
                else
                {
                    // File
                    Logger.Log("Requesting File: " + path);
                    return FetchFile(path);
                }
            }

            return GetError404();
            
        }

        private string FetchFile(string pathToFile)
        {
            string file = "";

            try
            {
                Logger.Log("Opening File...");
                StreamReader sr = new StreamReader(pathToFile);

                Logger.Log("Reading File...");
                while (!sr.EndOfStream)
                {
                    file += sr.ReadLine() + Environment.NewLine;
                }
                Logger.Log("Reading File Complete!");

                sr.Close();
                sr.Dispose();
            }
            catch (Exception)
            {
                Logger.Log("File Read Error: " + pathToFile);
            }

            return file;
        }

        public string GetError404()
        {
            string path = ServerReference.Properties.ErrorPath;
            if (File.Exists(path) && File.GetAttributes(path).HasFlag(FileAttributes.Directory) == false)
            {
                return FetchFile(ServerReference.Properties.ErrorPath);
            }
            else
            {
                return "Error Code: 404, Page not Found!";
            }

        }


        // This one actually streams the response message over the internet.
        public void SendResponse(HttpListenerContext context, string responseString)
        {
            // Obtain a response object.
            Logger.Log("Sending Response...");
            HttpListenerResponse response = context.Response;

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
            Logger.Log("Sending Response Complete!");
        }

    }
}
