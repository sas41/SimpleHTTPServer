using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleHTTPServer
{
    class FolderIndexer
    {
        public string GenerateHTML(string path, string rootPath = "")
        {
            string template = @"<!DOCTYPE HTML>
<html>
    <head>
    </head>
    <body>
        $$$TABLE$$$
    </body>
</html>";
            string table = BuildDisplayTable(IndexFolder(path, rootPath));

            return template.Replace("$$$TABLE$$$", table);
        }

        private List<string> IndexFolder(string path, string rootPath)
        {
            if (Directory.Exists(path))
            {
                List<string> dirs = Directory.GetDirectories(path)
                                    .Select(p => p += "/")          // Add a forward slash to end of files
                                    .ToList();

                List<string> files = Directory.GetFiles(path).ToList();

                dirs.AddRange(files);

                dirs = dirs.Select(p => p = p.Replace(rootPath, "./")).ToList();

                return dirs;
            }

            return new List<string>();
        }

        private string BuildDisplayTable(List<string> paths)
        {
            string table = @"
<table>
    <thead>
        <tr>
            <td>
            </td>
            <td>
                Name
            </td>
            <td>
                Size
            </td>
            <td>
                Modification Date
            </td>
        </tr>
    </thead>
    <tbody>";

            foreach (var path in paths)
            {
                bool isFolder = File.GetAttributes(path).HasFlag(FileAttributes.Directory);
                string iconClass = "file";
                if (isFolder) { iconClass = "folder"; }

                table += $@"
        <tr>
            <td>
                <div class='{iconClass}'>
                    {iconClass}
                </div>
            </td>
            <td>
                {path}
            </td>
            <td>
                SIZE
            </td>
            <td>
                DM
            </td>
        </tr>";
            }

            table += @"
    </tbody>
</table>";

            return table;
        }
    }
}
