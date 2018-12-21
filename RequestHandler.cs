using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Web;

namespace SimpleHTTPServer
{
    class RequestHandler
    {
        private HTTPServer serverReference;
        private HttpListener listener;
        private HttpListenerContext context;

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

        private HttpListenerContext Context
        {
            get { return context; }
            set { context = value; }
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
                Context = Listener.EndGetContext(result);
                Listener.BeginGetContext(OnContextRequest, Listener);

                //Get the specifics of the request and extract the target file/directory.
                string target = context.Request.RawUrl;

                string path = TargetDecoder(target);

                DetermineResponse(path);


            }
            catch (HttpListenerException e)
            {
                // If a connection is terminated mid-download, this will cause an exception.
                // We generally want to avoid that, as this is not fatal.
                Logger.Log(e.Message);
                Logger.Log("Data Stream Terminated early!");
            }
            catch (Exception)
            {
                // Othervise douse everything in gasoline and light a match.
                Logger.Log("Listener Stopped, Stopping Thread");
                throw;
            }
        }



        // This "Decodes" the requested page, it looks at the path of the address, and returns the index if it's empty.
        private string TargetDecoder(string target)
        {
            // Lmao, how did I forget this?
            target = HttpUtility.UrlDecode(target);

            //If no file/path is requested, serve the index file.
            string finalPath = "";

            if (target == "/")
            {
                finalPath = ServerReference.Properties.IndexPath;
            }
            else
            {
                finalPath = ServerReference.Properties.RootPath + target;
            }

            Logger.Log("Target Path: " + target);
            Logger.Log("Final Path: " + finalPath);

            return finalPath;
        }



        // Determine what kind of Response to send.
        private void DetermineResponse(string path)
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                //responseString = FetchFile(path);
                FileAttributes attr = File.GetAttributes(path);

                if (attr.HasFlag(FileAttributes.Directory) && ServerReference.Properties.AllowFolderInexing)
                {
                    // Dir
                    Logger.Log("Requesting Folder: " + path);
                    FolderIndexer myIndexer = new FolderIndexer();
                    string response = myIndexer.GenerateHTML(path);
                    ServeFileFromString(response);
                    return;
                }
                else
                {
                    // File
                    Logger.Log("Requesting File: " + path);
                    ServeFileFromPath(path);
                    return;
                }
            }

            ServeError404();
        }

        // Send the Error 404 page, default or overridden
        private void ServeError404()
        {
            string path = ServerReference.Properties.ErrorPath;
            if (File.Exists(path) && File.GetAttributes(path).HasFlag(FileAttributes.Directory) == false)
            {
                ServeFileFromPath(ServerReference.Properties.ErrorPath, true);
            }
            else
            {
                ServeFileFromString(Properties.Resources.Page_404);
            }

        }
        
        
        
        // Send a File or String as response.
        private void ServeFileFromPath(string path, bool forceHTML = false)
        {
            Logger.Log("Sending File...");

            string fileType = path.Split('.').Last().ToLower();

            var response = Context.Response;

            using (FileStream fs = File.OpenRead(path))
            {
                string filename = Path.GetFileName(path);

                response.ContentLength64 = fs.Length;
                response.SendChunked = false;
                response.KeepAlive = true;

                if (forceHTML)
                {
                    fileType = "html";
                }
                response.ContentType = DetermineMime(fileType);
                //response.AddHeader("Content-disposition", "attachment; filename=" + filename);

                byte[] buffer = new byte[64 * 1024];
                int read;
                using (BinaryWriter bw = new BinaryWriter(response.OutputStream))
                {
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        Thread.Sleep(200);
                        bw.Write(buffer, 0, read);
                        bw.Flush();
                    }

                    bw.Close();
                }

                response.StatusCode = (int)HttpStatusCode.OK;
                response.StatusDescription = "OK";
                response.OutputStream.Close();
            }

            Logger.Log("Sending File Complete!");
        }

        private void ServeFileFromString(string responseString)
        {
            Logger.Log("Sending File...");
            HttpListenerResponse response = Context.Response;

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();

            Logger.Log("Sending File Complete!");

        }



        // Determine the MIME type string of various file-formats, for convinence.
        private string DetermineMime(string fileType)
        {
            switch (fileType)
            {
                case("html"): { return System.Net.Mime.MediaTypeNames.Text.Html; }
                case ("txt"): { return System.Net.Mime.MediaTypeNames.Text.Plain; }
                case ("rtf"): { return System.Net.Mime.MediaTypeNames.Text.RichText; }
                case ("xml"): { return System.Net.Mime.MediaTypeNames.Text.Xml; }

                case ("gif"): { return System.Net.Mime.MediaTypeNames.Image.Gif; }
                case ("jpg"): { return System.Net.Mime.MediaTypeNames.Image.Jpeg; }
                case ("jpeg"): { return System.Net.Mime.MediaTypeNames.Image.Jpeg; }
                case (".tiff"): { return System.Net.Mime.MediaTypeNames.Image.Tiff; }
                case ("png"): { return System.Net.Mime.MediaTypeNames.Image.Jpeg; }
                case ("svg"): { return System.Net.Mime.MediaTypeNames.Image.Jpeg; }

                case ("mp4"): { return "video/mp4"; }
                case ("webm"): { return "video/webm"; }

                case ("pdf"): { return System.Net.Mime.MediaTypeNames.Application.Pdf; }
                case ("rtfa"): { return System.Net.Mime.MediaTypeNames.Application.Rtf; }
                case ("zip"): { return System.Net.Mime.MediaTypeNames.Application.Zip; }
                case ("soap"): { return System.Net.Mime.MediaTypeNames.Application.Soap; }

                default: { return System.Net.Mime.MediaTypeNames.Application.Octet; }
            }

        }
    }
}
