using MCBS.SystemApplications.Desktop.Extensions;
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
    public class DesktopIconManager
    {
        public int MAX_SIZE = 256;

        public DesktopIconManager()
        {
            Icons = [];
        }

        public DesktopIconManager(IDictionary<Point, IconIdentifier> icons)
        {
            ArgumentNullException.ThrowIfNull(icons, nameof(icons));

            Icons = icons.ToDictionary();
            MaxSize = new(MAX_SIZE, MAX_SIZE);
        }

        public Dictionary<Point, IconIdentifier> Icons { get; private set; }

        public int Width => Icons.Count == 0 ? 0 : Icons.Keys.Max(m => m.X) + 1;

        public int Height => Icons.Count == 0 ? 0 : Icons.Keys.Max(m => m.Y) + 1;

        public Size MaxSize
        {
            get => _MaxSize;
            set
            {
                ThrowHelper.ArgumentOutOfRange(0, MAX_SIZE, value.Width, "value.Width");
                ThrowHelper.ArgumentOutOfRange(0, MAX_SIZE, value.Height, "value.Height");
                _MaxSize = value;
            }
        }
        private Size _MaxSize;

        public int MaxCount => MaxSize.Width * MaxSize.Height;

        public int AvailablePositionCount => GetAllAvailablePosition().Length;

        public bool HasAvailablePosition
        {
            get
            {
                for (int x = 0; x < MaxSize.Width; x++)
                    for (int y = 0; y < MaxSize.Height; y++)
                    {
                        Point position = new(x, y);
                        if (!Icons.ContainsKey(position))
                            return true;
                    }

                return false;
            }
        }

        public void AppendIcon(IconIdentifier iconIdentifier)
        {
            Icons.Add(GetAvailablePosition(), iconIdentifier);
        }

        public void MoveIcon(Point source, Point destination)
        {
            if (source == destination)
                return;

            if (!Icons.ContainsKey(source))
                throw new InvalidOperationException($"源位置{source.ToPositionString()}的图标不存在");
            if (Icons.ContainsKey(destination))
                throw new InvalidOperationException($"目标位置{source.ToPositionString()}已被其他图标占用");

            if (Icons.Remove(source, out var iconIdentifier))
                Icons.TryAdd(destination, iconIdentifier);
        }

        public void IconsPositionFix()
        {
            List<Point> positions = [];

            foreach (var item in Icons)
            {
                Point position = item.Key;
                if (position.X < 0 ||
                    position.X >= MAX_SIZE ||
                    position.Y < 0 ||
                    position.Y >= MAX_SIZE)
                    positions.Add(position);
            }

            foreach (Point position in positions)
            {
                if (Icons.Remove(position, out var iconIdentifier))
                    Icons.Add(GetAvailablePosition(), iconIdentifier);
            }
        }

        public Point GetAvailablePosition()
        {
            for (int x = 0; x < MaxSize.Width; x++)
                for (int y = 0; y < MaxSize.Height; y++)
                {
                    Point position = new(x, y);
                    if (!Icons.ContainsKey(position))
                        return position;
                }

            throw new InvalidOperationException("没有可用的位置");
        }

        public Point[] GetAllAvailablePosition()
        {
            List<Point> result = [];
            for (int x = 0; x < MaxSize.Width; x++)
                for (int y = 0; y < MaxSize.Height; y++)
                {
                    Point position = new(x, y);
                    if (!Icons.ContainsKey(position))
                        result.Add(position);
                }

            return result.ToArray();
        }

        public void UpdateIcons(IDictionary<Point, IconIdentifier> icons)
        {
            ArgumentNullException.ThrowIfNull(icons, nameof(icons));

            Icons = icons.ToDictionary();
        }

        public void FromDataModelUpdate(Dictionary<string, string> dataModel)
        {
            UpdateIcons(FromDataModelRead(dataModel));
        }

        public void FromJsonUpdate(string json)
        {
            UpdateIcons(FromJsonRead(json));
        }

        public Dictionary<string, string> ToDataModel()
        {
            return Icons.ToDictionary(i => i.Key.ToHex8(), i => i.Value.ToString());
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(ToDataModel());
        }

        public static Dictionary<Point, IconIdentifier> FromDataModelRead(Dictionary<string, string> dataModel)
        {
            ArgumentNullException.ThrowIfNull(dataModel, nameof(dataModel));

            Dictionary<Point, IconIdentifier> icons = dataModel.ToDictionary(i => PointExtensions.ParseHex8(i.Key), i => IconIdentifier.Parse(i.Value));
            return new(icons);
        }

        public static Dictionary<Point, IconIdentifier> FromJsonRead(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

            Dictionary<string, string> dataModel = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? [];
            return FromDataModelRead(dataModel);
        }

        public static bool Equals(IDictionary<Point, IconIdentifier>? iconsA, IDictionary<Point, IconIdentifier>? iconsB)
        {
            if (iconsA == iconsB)
                return true;
            if (iconsA is null || iconsB is null)
                return false;

            if (iconsA.Count != iconsB.Count)
                return false;

            foreach (var item in iconsA)
            {
                if (!iconsB.TryGetValue(item.Key, out IconIdentifier iconIdentifier) || iconIdentifier != item.Value)
                    return false;
            }

            return true;
        }
    }
}
