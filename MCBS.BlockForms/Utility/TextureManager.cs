using Newtonsoft.Json;
using QuanLib.Core.Extension;
using QuanLib.Minecraft.Instance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public class TextureManager : IReadOnlyDictionary<string, Image<Rgba32>>
    {
        private const string TEXTURE_INDEX_NAME = "MCBS.BlockForms.SystemResource.TextureIndex.json";
        private const string TEXTURES_NAMESPACE = "MCBS.BlockForms.SystemResource.Textures";

        static TextureManager()
        {
            _slock = new();
            IsLoaded = false;
        }

        private TextureManager(Dictionary<string, Image<Rgba32>> items)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        private static readonly object _slock;

        public static bool IsLoaded { get; private set; }

        public static TextureManager Instance
        {
            get
            {
                if (_Instance is null)
                    throw new InvalidOperationException("实例未加载");
                return _Instance;
            }
        }
        private static TextureManager? _Instance;

        private readonly Dictionary<string, Image<Rgba32>> _items = new();

        public IEnumerable<string> Keys => _items.Keys;

        public IEnumerable<Image<Rgba32>> Values => _items.Values;

        public int Count => _items.Count;

        public Image<Rgba32> this[string key] => _items[key];

        public static TextureManager LoadInstance()
        {
            lock (_slock)
            {
                if (_Instance is not null)
                    throw new InvalidOperationException("试图重复加载单例实例");

                Dictionary<string, Image<Rgba32>> items = Load();
                _Instance ??= new(items);
                IsLoaded = true;
                return _Instance;
            }
        }

        private static Dictionary<string, Image<Rgba32>> Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream indexsStream = assembly.GetManifestResourceStream(TEXTURE_INDEX_NAME) ?? throw new IndexOutOfRangeException();
            string indexsJson = indexsStream.ToUtf8Text();
            Dictionary<string, string> indexs = JsonConvert.DeserializeObject<Dictionary<string, string>>(indexsJson) ?? throw new IndexOutOfRangeException();

            Dictionary<string, Image<Rgba32>> result = new();
            foreach (var index in indexs)
            {
                using Stream stream = assembly.GetManifestResourceStream($"{TEXTURES_NAMESPACE}.{index.Key}") ?? throw new IndexOutOfRangeException();
                result.Add(Path.GetFileNameWithoutExtension(index.Key), Image.Load<Rgba32>(stream));
            }

            return result;
        }

        public bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out Image<Rgba32> value)
        {
            return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, Image<Rgba32>>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
