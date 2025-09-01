using DeepSigma.OperatingSystem.Models;
using DeepSigma.OperatingSystem.Enums;

namespace DeepSigma.OperatingSystem.IgnoreFile
{
    /// <summary>
    /// Engine to process ignore files and generate filters.
    /// </summary>
    public class FileSystemSelectionEngine
    {
        private FileSystemFilterAccess filterController { get; set; }
        private List<string> filters { get; set; } = [];
        private FileSystemFilterCollection filterCollection { get; set; } = new();
        public FileSystemSelectionEngine(string file_path, string file_name)
        {
            filterController = new FileSystemFilterAccess(file_path, file_name);
            filters = filterController.GetIgnoreFilters().ToList();
            SaveFilters();
        }

        /// <summary>
        /// Returns true if the file object passes matches a filter.
        /// </summary>
        /// <param name="file_system_item"></param>
        /// <returns></returns>
        public bool MatchingFilterFound(string file_system_item)
        {
            foreach(FileSystemFilterItem filter in filterCollection.Items)
            {
                string search_text =  GetCleanedFilterText(filter.OriginalFilter);
                if (filter.StartsWithAnything && filter.EndsWithAnything)
                {
                    if (file_system_item.Contains(search_text)) return true;
                }
                else if (filter.StartsWithAnything)
                {
                    if (file_system_item.StartsWith(search_text)) return true;
                }
                else if (filter.EndsWithAnything)
                {
                    if (file_system_item.EndsWith(search_text)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Save the filters.
        /// </summary>
        /// <returns></returns>
        private void SaveFilters()
        {
            FileSystemFilterCollection results = new FileSystemFilterCollection();
            foreach (string filter in filters)
            {
                (bool IsDirectory, bool StartsWithAnything, bool EndsWithAnything, bool IsComment) = GetFilterInfo(filter);
                if (IsComment) continue;
                results.Items.Add(new FileSystemFilterItem(filter, IsDirectory ? FileSystemType.Directory : FileSystemType.File, StartsWithAnything, EndsWithAnything));
            }
            filterCollection = results;
        }

        /// <summary>
        /// Removes special characters from the filter string.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static string GetCleanedFilterText(string filter)
        {
            return filter.Replace("\\", null).Replace("*", null);
        }


        /// <summary>
        /// Gets the ignore filter info.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>
        /// </returns>
        private static (bool IsDirectory, bool StartsWithAnything, bool EndsWithAnything, bool IsComment) GetFilterInfo(string filter)
        {
            bool IsComment = filter.Trim().StartsWith("#");
            bool IsDirectory = false;
            bool StartsWithAnything = false;
            bool EndsWithAnything = false;

            if (filter.StartsWith("\\"))
            {
                IsDirectory = true;
            }

            if (filter.StartsWith("\\*") || filter.StartsWith("*"))
            {
                StartsWithAnything = true;
            }

            if (filter.EndsWith("*"))
            {
                EndsWithAnything = true;
            }
            return (IsDirectory, StartsWithAnything, EndsWithAnything, IsComment);
        }
    }
}
