using MCBS.Application;
using MCBS.SystemApplications.Desktop.DesktopIcons;
using Newtonsoft.Json;
using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop
{
    public static class IconTableReader
    {
        public static Dictionary<Point, IconIdentifier> ReadIconTable(string screenDataDirectory, Size tableSize)
        {
            ArgumentException.ThrowIfNullOrEmpty(screenDataDirectory, nameof(screenDataDirectory));

            string desktopDirectory = Path.Combine(screenDataDirectory, "Desktop");
            string iconDataJsonFile = Path.Combine(screenDataDirectory, "IconData.json");
            Dictionary<Point, IconIdentifier> iconData = ReadJson(iconDataJsonFile);
            Dictionary<Point, IconIdentifier> result = [];
            HashSet<string> appIds = [];
            HashSet<string> paths = [];

            foreach (var item in iconData)
            {
                Point position = item.Key;
                IconIdentifier iconIdentifier = item.Value;
                switch (iconIdentifier.Type)
                {
                    case DesktopAppIcon.ICON_TYPE:
                        string appId = iconIdentifier.Value;
                        if (MinecraftBlockScreen.Instance.AppComponents.ContainsKey(appId) && !appIds.Contains(appId))
                        {
                            result.Add(position, iconIdentifier);
                            appIds.Add(appId);
                        }
                        break;
                    case DesktopFileIcon.ICON_TYPE:
                    case DesktopDirectoryIcon.ICON_TYPE:
                        string fileName = FormatPath(desktopDirectory, iconIdentifier.Value);
                        if (paths.Contains(fileName))
                            break;
                        if (iconIdentifier.Type == DesktopFileIcon.ICON_TYPE && !File.Exists(fileName))
                            break;
                        if (iconIdentifier.Type == DesktopDirectoryIcon.ICON_TYPE && !Directory.Exists(fileName))
                            break;
                        result.Add(position, iconIdentifier);
                        paths.Add(fileName);
                        break;
                }
            }

            if (result.Count > 0)
            {
                tableSize.Width = Math.Max(tableSize.Width, result.Keys.Max(m => m.X) + 1);
                tableSize.Height = Math.Max(tableSize.Height, result.Keys.Max(m => m.Y) + 1);
            }

            foreach (ApplicationManifest applicationManifest in MinecraftBlockScreen.Instance.AppComponents.Values)
            {
                if (applicationManifest.IsBackground)
                    continue;

                if (!appIds.Contains(applicationManifest.ID))
                    result.Add(GetNextPosition(result, tableSize), new(DesktopAppIcon.ICON_TYPE, applicationManifest.ID));
            }

            if (!Directory.Exists(desktopDirectory))
                return result;

            string[] files = Directory.GetFiles(desktopDirectory);
            foreach (string file in files)
            {
                if (!paths.Contains(file))
                    result.Add(GetNextPosition(result, tableSize), new(DesktopFileIcon.ICON_TYPE, file));
            }

            string[] directories = Directory.GetDirectories(desktopDirectory);
            foreach (string directory in directories)
            {
                if (!paths.Contains(directory))
                    result.Add(GetNextPosition(result, tableSize), new(DesktopDirectoryIcon.ICON_TYPE, directory));
            }

            return result;
        }

        private static Point GetNextPosition(Dictionary<Point, IconIdentifier> icons, Size tableSize)
        {
            ArgumentNullException.ThrowIfNull(icons, nameof(icons));

            for (int x = 0; ; x++)
                for (int y = 0; y < tableSize.Height; y++)
                {
                    Point position = new(x, y);
                    if (!icons.ContainsKey(position))
                        return position;
                }
        }

        private static Dictionary<Point, IconIdentifier> ReadJson(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            if (!File.Exists(path))
                return [];

            string json = File.ReadAllText(path);
            Dictionary<string, string> identifiers = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? [];

            return identifiers.ToDictionary(
                item => ParsePosition(item.Key),
                item => IconIdentifier.Parse(item.Value));
        }

        private static Point ParsePosition(string value)
        {
            ThrowHelper.StringLengthOutOfRange(4, value, nameof(value));

            int x = Convert.ToInt32(value[..2], 16);
            int y = Convert.ToInt32(value[2..], 16);
            return new Point(x, y);
        }

        private static string FormatPath(string desktopDirectory, string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(desktopDirectory, nameof(desktopDirectory));
            ArgumentException.ThrowIfNullOrEmpty(fileName, nameof(fileName));

            return Path.Combine(desktopDirectory, fileName.TrimEnd(Path.PathSeparator));
        }
    }
}
