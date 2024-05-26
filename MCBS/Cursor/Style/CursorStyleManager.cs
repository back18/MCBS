using Newtonsoft.Json;
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
using QuanLib.Minecraft;
using QuanLib.Core.Extensions;
using MCBS.Rendering;
using QuanLib.Core;

namespace MCBS.Cursor.Style
{
    public class CursorStyleManager : IReadOnlyDictionary<string, CursorStyle>, ISingleton<CursorStyleManager, InstantiateArgs>
    {
        private CursorStyleManager(Dictionary<string, CursorStyle> items)
        {
            ArgumentNullException.ThrowIfNull(items, nameof(items));

            _items = items;
        }

        private static readonly object _slock = new();

        public static bool IsInstanceLoaded => _Instance is not null;

        public static CursorStyleManager Instance => _Instance ?? throw new InvalidOperationException("实例未加载");
        private static CursorStyleManager? _Instance;

        private readonly Dictionary<string, CursorStyle> _items;

        public CursorStyle this[string key] => _items[key];

        public IEnumerable<string> Keys => _items.Keys;

        public IEnumerable<CursorStyle> Values => _items.Values;

        public int Count => _items.Count;

        public static CursorStyleManager LoadInstance()
        {
            return LoadInstance(InstantiateArgs.Empty);
        }

        public static CursorStyleManager LoadInstance(InstantiateArgs instantiateArgs)
        {
            ArgumentNullException.ThrowIfNull(instantiateArgs, nameof(instantiateArgs));

            lock (_slock)
            {
                if (_Instance is not null)
                    throw new InvalidOperationException("试图重复加载单例实例");

                Dictionary<string, CursorStyle> items = Load();
                _Instance = new(items);
                return _Instance;
            }
        }

        private static Dictionary<string, CursorStyle> Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using Stream indexsStream = assembly.GetManifestResourceStream("MCBS.SystemResource.CursorIndex.json") ?? throw new InvalidOperationException();
            string indexsJson = indexsStream.ReadAllText();
            CursorModel[] indexs = JsonConvert.DeserializeObject<CursorModel[]>(indexsJson) ?? throw new InvalidOperationException();

            Dictionary<string, CursorStyle> result = new();
            foreach (var index in indexs)
            {
                using Stream stream = assembly.GetManifestResourceStream($"MCBS.SystemResource.Cursors.{index.Image}") ?? throw new InvalidOperationException();
                var image = Image.Load<Rgba32>(stream);
                ColorBlockFrame<Rgba32> colorBlockFrame = new(image);
                CursorStyle cursor = new(index.Type, new(index.XOffset, index.YOffset), colorBlockFrame);
                result.Add(cursor.CursorType, cursor);
            }

            return result;
        }

        public bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out CursorStyle value)
        {
            return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, CursorStyle>> GetEnumerator()
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
