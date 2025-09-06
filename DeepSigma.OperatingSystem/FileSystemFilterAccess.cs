using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeepSigma.OperatingSystem
{
    internal class FileSystemFilterAccess
    {
        private string directory_path { get; set; }
        private string file_name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemFilterAccess"/> class.
        /// </summary>
        /// <param name="directory_path">Directory path of the ignore file.</param>
        /// <param name="file_name">File name of the ignore file.</param>
        internal FileSystemFilterAccess(string directory_path, string file_name)
        {
            this.directory_path = directory_path;
            this.file_name = file_name;
        }

        /// <summary>
        /// Gets ignore filters from designated file if it exists.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<string> GetIgnoreFilters()
        {
            string FullPath = Path.Combine(directory_path, file_name);
            if (!File.Exists(FullPath)) { yield break; }

            string[] fileLines = File.ReadAllLines(FullPath);
            foreach (string line in fileLines)
            {
                string[] words = line.Split(" ");
                foreach (string word in words)
                {
                    if (IsComment(word))
                    {
                        continue;
                    }
                    yield return word.Trim();
                }
            }
        }

        /// <summary>
        /// Determines if text is a comment as I define it.
        /// Comments start with # and last the entire line.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool IsComment(string text)
        {
            return text.Trim().StartsWith("#");
        }
    }
}
