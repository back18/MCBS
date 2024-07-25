using MCBS.SystemApplications.Desktop.DesktopIcons;
using Newtonsoft.Json;
using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop
{
    public class DesktopIconTable
    {
        public int MAX_SIZE = 256;

        public DesktopIconTable()
        {
            _icons = [];
        }

        public DesktopIconTable(IDictionary<Point, DesktopIcon> icons)
        {
            ArgumentNullException.ThrowIfNull(icons, nameof(icons));

            _icons = icons.ToDictionary();
        }

        private readonly Dictionary<Point, DesktopIcon> _icons;

        public int Width => _icons.Keys.Max(m => m.X) + 1;

        public int Height => _icons.Keys.Max(m => m.Y) + 1;

        public int IconCount => _icons.Count;

        public DesktopIcon GetIcon(Point position)
        {
            return _icons[position];
        }

        public DesktopIcon GetIcon(int x, int y)
        {
            return GetIcon(new(x, y));
        }

        public bool TryGetIcon(Point position, [MaybeNullWhen(false)] out DesktopIcon result)
        {
            return _icons.TryGetValue(position, out result);
        }

        public bool TryGetIcon(int x, int y, [MaybeNullWhen(false)] out DesktopIcon result)
        {
            return TryGetIcon(new(x, y), out result);
        }

        public bool ContainsIcon(Point position)
        {
            return _icons.ContainsKey(position);
        }

        public bool ContainsIcon(int x, int y)
        {
            return ContainsIcon(new(x, y));
        }

        public void CreateIcon(Point position, DesktopIcon desktopIcon)
        {
            ValidatePosition(position, nameof(position));
            
            if (_icons.ContainsKey(position))
                throw new InvalidOperationException($"无法创建图标，因为目标位置[{position.X},{position.Y}]已被其他图标占用");

            desktopIcon.ClientLocation = position * 24;
            _icons[position] = desktopIcon;
        }

        public void CreateIcon(int x, int y, DesktopIcon desktopIcon)
        {
            CreateIcon(new(x, y), desktopIcon);
        }

        public void CreateIcon(Point position, string desktopDirectory, IconIdentifier iconIdentifier)
        {
            CreateIcon(position, DesktopIconFactory.CreateDesktopIcon(desktopDirectory, iconIdentifier));
        }

        public void CreateIcon(int x, int y, string desktopDirectory, IconIdentifier iconIdentifier)
        {
            CreateIcon(new(x, y), desktopDirectory, iconIdentifier);
        }

        public bool RemoveIcon(Point position)
        {
            return _icons.Remove(position);
        }

        public bool RemoveIcon(int x, int y)
        {
            return RemoveIcon(new Point(x, y));
        }

        public void ClearIcons()
        {
            _icons.Clear();
        }

        public void MoveIcon(Point source, Point destination)
        {
            ValidatePosition(destination, nameof(destination));

            if (!_icons.TryGetValue(source, out var sourceIcon))
                throw new InvalidOperationException($"无法移动图标，因为原位置[{source.X},{source.Y}]的图标不存在");
            if (_icons.ContainsKey(destination))
                throw new InvalidOperationException($"无法移动图标，因为目标位置[{destination.X},{destination.Y}]已被其他图标占用");

            sourceIcon.ClientLocation = destination * 24;
            _icons[destination] = sourceIcon;
            _icons.Remove(source);
        }

        private void ValidatePosition(Point position, string paramName)
        {
            if (position.X < 0 ||
                position.X >= MAX_SIZE ||
                position.Y < 0 ||
                position.Y >= MAX_SIZE)
                throw new ArgumentException($"图标位置的坐标范围应该为0到{MAX_SIZE - 1}之间", paramName);
        }

        public string ToJson()
        {
            Dictionary<string, string> identifiers = _icons.ToDictionary(
                item => ToString(item.Key),
                item => item.Value.GetIconIdentifier().ToString());

            return JsonConvert.SerializeObject(identifiers);
        }

        private static string ToString(Point position)
        {
            return Convert.ToString(position.X, 16).PadLeft(2, '0') + Convert.ToString(position.Y, 16).PadLeft(2, '0');
        }

        private static Point ParsePosition(string value)
        {
            ThrowHelper.StringLengthOutOfRange(4, value, nameof(value));

            int x = Convert.ToInt32(value[..2], 16);
            int y = Convert.ToInt32(value[2..], 16);
            return new Point(x, y);
        }
    }
}
