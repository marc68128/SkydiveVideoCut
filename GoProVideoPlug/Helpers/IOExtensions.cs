using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoProVideoPlug.Helpers
{
    public class IOExtensions
    {

        public static string[] VideoExtensions = new[] { "*.avi", "*.AVI", "*.mkv", "*.MKV", "*.mp4", "*.MP4" };
        public static List<string> GetFilesRecursiv(string path, string searchPattern)
        {
            var files = Directory.GetFiles(path, searchPattern).ToList();
            var folders = Directory.GetDirectories(path);
            foreach (var folder in folders)
            {
                files.AddRange(GetFilesRecursiv(folder, searchPattern));
            }
            return files;
        }

        public static List<string> GetDirectoriesRecursiv(string path, string searchPattern)
        {
            var folders = Directory.GetDirectories(path, searchPattern).ToList();
            var allFolders = Directory.GetDirectories(path);
            foreach (var folder in allFolders)
            {
                folders.AddRange(GetDirectoriesRecursiv(folder, searchPattern));
            }
            return folders;
        }
    }
}
