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
        private List<string> aliases;
        private string rootPath;
        private string indexFile;
        private string error404File;

        public List<string> Aliases { get => aliases; set => aliases = value; }
        public string RootPath { get => rootPath; set => rootPath = value; }
        public string IndexFile { get => indexFile; set => indexFile = value; }
        public string Error404File { get => error404File; set => error404File = value; }

        HttpListener listener;
        private bool isLive = false;

        public HTTPServer(List<string> aliases,string rootPath, string indexFile, string error404File = "")
        {
            Aliases = new List<string>();
            foreach (var alias in aliases)
            {
                Aliases.Add(alias);
            }
            RootPath = rootPath;
            IndexFile = indexFile;
            Error404File = error404File;
        }

        public void StartListening()
        {
            if (isLive)
            {
                throw new Exception("Listener Already Running!");
            }

            else if (!HttpListener.IsSupported)
            {
                throw new Exception("OS Not Supported!");
            }

            listener = new HttpListener();

            if (Aliases == null || Aliases.Count() == 0)
            {
                throw new ArgumentException("You must add an Alias/Prefix!");
            }

            try
            {
                foreach (string alias in aliases)
                {
                    listener.Prefixes.Add(alias);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid Alias(es)/Prefix(es), Example: http://localhost:8080");
            }

            try
            {
                listener.Start();
                Log("Listening...");
                Log("Root Path: " + RootPath);
                isLive = true;

                IAsyncResult Result = listener.BeginGetContext(new AsyncCallback(OnContextRequest), listener);
            }
            catch (Exception)
            {
                
            }
        }

        public void OnContextRequest(IAsyncResult result)
        {
            try
            {
                //The GetContext method blocks while waiting for a request.
                //We use EndGetContext and BeginGetContext instead.
                //As soon as we are done with the last context, we should be listening for the next.
                //That way we can reply to many at the same time.
                HttpListenerContext context = listener.EndGetContext(result);
                listener.BeginGetContext(OnContextRequest, listener);

                //Simulate Delay
                //Thread.Sleep(5000);

                //Get the specifics of the request and extract the target file/directory.
                HttpListenerRequest request = context.Request;
                string target = request.RawUrl;

                // Obtain a response object.
                HttpListenerResponse response = context.Response;

                //If no file/path is requested, serve the index file.
                if (target == "/")
                {
                    target = IndexFile;
                }

                string path = RootPath + target;

                Log("Target Path: " + target);
                Log("Final Path: " + path);

                string responseString = "";

                if (File.Exists(path))
                {
                    responseString = FetchFile(path);
                }
                else if(File.Exists(RootPath + Error404File))
                {
                    response.Redirect("//" + request.UserHostName + Error404File);
                    response.Close();
                    return;
                }
                else
                {
                    responseString = "Error Code: 404, Page not Found!";
                }

                // Get a response stream and write the response to it.
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception)
            {
                Log("Listener Stopped, Stopping Thread");
                return;
            }
        }

        private string FetchFile(string pathToFile, bool default404 = false)
        {
            string file = "";

            try
            {
                Log("Opening File...");
                StreamReader sr = new StreamReader(pathToFile);

                Log("Reading File...");
                while (!sr.EndOfStream)
                {
                    file += sr.ReadLine() + Environment.NewLine;
                }
                Log("Done!");

                sr.Close();
                sr.Dispose();
            }
            catch (Exception)
            {
                Log("File Read Error: " + pathToFile);
            }

            return file;
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
                Log("Listener has not been started!");
            }
        }

        public void Log(string s)
        {
            string time = DateTime.UtcNow.ToString("HH:mm:ss.fff");
            string thread = Thread.CurrentThread.ManagedThreadId.ToString();
            Console.WriteLine(time + " - " + thread + " - " + s);
        }
    }
}
