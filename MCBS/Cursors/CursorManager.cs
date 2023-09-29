using Newtonsoft.Json;
using QuanLib.Core.Extension;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MCBS.Frame;
using QuanLib.Minecraft;

namespace MCBS.Cursors
{
    public class CursorManager : IReadOnlyDictionary<string, Cursor>
    {
        static CursorManager()
        {
            _slock = new();
            IsLoaded = false;
        }

        private CursorManager(Dictionary<string, Cursor> items)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        private static readonly object _slock;

        public static bool IsLoaded { get; private set; }

        public static CursorManager Instance
        {
            get
            {
                if (_Instance is null)
                    throw new InvalidOperationException("实例未加载");
                return _Instance;
            }
        }
        private static CursorManager? _Instance;

        private readonly Dictionary<string, Cursor> _items;

        public Cursor this[string key] => _items[key];

        public IEnumerable<string> Keys => _items.Keys;

        public IEnumerable<Cursor> Values => _items.Values;

        public int Count => _items.Count;

        public static CursorManager LoadInstance()
        {
            lock (_slock)
            {
                if (_Instance is not null)
                    throw new InvalidOperationException("试图重复加载单例实例");

                Dictionary<string, Cursor> items = Load();
                _Instance ??= new(items);
                IsLoaded = true;
                return _Instance;
            }
        }

        private static Dictionary<string, Cursor> Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using Stream indexsStream = assembly.GetManifestResourceStream(SR.SYSTEM_RESOURCE_NAMESPACE + ".CursorIndex.json") ?? throw new InvalidOperationException();
            string indexsJson = indexsStream.ToUtf8Text();
            CursorModel[] indexs = JsonConvert.DeserializeObject<CursorModel[]>(indexsJson) ?? throw new InvalidOperationException();

            Dictionary<string, Cursor> result = new();
            foreach (var index in indexs)
            {
                using Stream stream = assembly.GetManifestResourceStream(SR.SYSTEM_RESOURCE_NAMESPACE + ".Cursors." + index.Image) ?? throw new InvalidOperationException();
                var image = Image.Load<Rgba32>(stream);
                ArrayFrame frame = ArrayFrame.FromImage(Facing.Zm, image, string.Empty);
                Cursor cursor = new(index.Type, new(index.XOffset, index.YOffset), frame);
                result.Add(cursor.CursorType, cursor);
            }

            return result;
        }

        public bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out Cursor value)
        {
            return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, Cursor>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        private class CursorModel
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            public string Type { get; set; }

            public string Image { get; set; }

            public int XOffset { get; set; }

            public int YOffset { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
