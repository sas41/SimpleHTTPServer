using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleHTTPServer
{
    class ServerProperties
    {
        private List<string> aliases;
        private string rootPath;
        private string indexFile;
        private string error404File;
        private bool allowFolderIndexing;
        private List<string> whitelistedExtentions;

        public List<string> Aliases { get => aliases; private set => aliases = value; }
        public string RootPath { get => rootPath; private set => rootPath = value; }
        public string IndexFile { get => indexFile; private set => indexFile = value; }
        public string Error404File { get => error404File; private set => error404File = value; }
        public bool AllowFolderInexing { get => allowFolderIndexing; private set => allowFolderIndexing = value; }
        public List<string> WhitelistedExtentions { get => whitelistedExtentions; private set => whitelistedExtentions = value; }

        public ServerProperties(List<string> a, string ropa, string indf, string errf, bool afi = false)
        {
            Aliases = a;
            RootPath = ropa;
            IndexFile = indf;
            Error404File = errf;
            AllowFolderInexing = afi;
            WhitelistedExtentions = new List<string>() { "html", "png", "jpg", "jpeg", "mp4", "mp3" };
        }

        public string IndexPath => RootPath + IndexFile;
        public string ErrorPath => rootPath + Error404File;
    }
}
