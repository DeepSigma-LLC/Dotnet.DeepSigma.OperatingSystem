using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSigma.OperatingSystem.Models
{
    /// <summary>
    /// Represents a collection of file system filter items.
    /// </summary>
    public class FileSystemFilterCollection
    {
        /// <summary>
        /// A list of file system filter items.
        /// </summary>
        public List<FileSystemFilterItem> Items { get; set; } = [];
    }
}
