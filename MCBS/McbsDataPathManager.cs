using QuanLib.IO.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class McbsDataPathManager
    {
        public McbsDataPathManager(string mcbsDataDirectory)
        {
            ArgumentException.ThrowIfNullOrEmpty(mcbsDataDirectory, nameof(mcbsDataDirectory));

            _mcbsDataPaths = new(mcbsDataDirectory);
        }

        protected readonly McbsDataPaths _mcbsDataPaths;

        public DirectoryInfo McbsData => _mcbsDataPaths.McbsData.CreateDirectoryInfo();

        public DirectoryInfo McbsData_Screens => _mcbsDataPaths.McbsData_Screens.CreateDirectoryInfo();

        public DirectoryInfo McbsData_Interactions => _mcbsDataPaths.McbsData_Interactions.CreateDirectoryInfo();

        public static McbsDataPathManager FromWorldDirectoryCreate(string worldDirectory)
        {
            ArgumentException.ThrowIfNullOrEmpty(worldDirectory, nameof(worldDirectory));

            return new(Path.Combine(worldDirectory, "mcbsdata"));
        }

        protected class McbsDataPaths
        {
            public McbsDataPaths(string mcbsDataDirectory)
            {
                ArgumentException.ThrowIfNullOrEmpty(mcbsDataDirectory, nameof(mcbsDataDirectory));

                McbsData = mcbsDataDirectory;
                McbsData_Screens = McbsData.PathCombine("screens");
                McbsData_Interactions = McbsData.PathCombine("interactions");
            }

            public readonly string McbsData;

            public readonly string McbsData_Screens;

            public readonly string McbsData_Interactions;
        }
    }
}
