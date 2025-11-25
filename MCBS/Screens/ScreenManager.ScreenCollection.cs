using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.ObjectModel;
using QuanLib.Core.Events;

namespace MCBS.Screens
{
    public partial class ScreenManager
    {
        public class ScreenCollection : IEnumerable<ScreenContext>
        {
            public ScreenCollection(ScreenManager owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                _items = [];

                _items.Added += Items_Added;
                _items.Removed += Items_Removed;
            }

            private readonly ScreenManager _owner;

            private readonly GuidDictionary<ScreenContext> _items;

            public int Count => _items.Count;

            public ScreenContext GetScreen(Guid guid)
            {
                return _items[guid];
            }

            public ScreenContext GetScreen(string shortId)
            {
                return _items[shortId];
            }

            public ScreenContext[] GetScreens()
            {
                return _items.Values.ToArray();
            }

            internal void AddScreen(ScreenContext screenContext)
            {
                _items.Add(screenContext);
            }

            internal bool RemoveScreen(Guid guid)
            {
                return _items.Remove(guid);
            }

            internal bool RemoveScreen(string shortId)
            {
                return _items.Remove(shortId);
            }

            internal bool RemoveScreen(ScreenContext screenContext)
            {
                return _items.Remove(screenContext);
            }

            internal void ClearAllScreen()
            {
                _items.Clear();
            }

            public bool ContainsScreen(Guid guid)
            {
                return _items.ContainsKey(guid);
            }

            public bool ContainsScreen(string shortId)
            {
                return _items.ContainsKey(shortId);
            }

            public bool ContainsScreen(ScreenContext screenContext)
            {
                ArgumentNullException.ThrowIfNull(screenContext, nameof(screenContext));

                return _items.ContainsKey(screenContext.Guid);
            }

            public bool TryGetScreen(Guid guid, [MaybeNullWhen(false)] out ScreenContext result)
            {
                return _items.TryGetValue(guid, out result);
            }

            public bool TryGetScreen(string shortId, [MaybeNullWhen(false)] out ScreenContext result)
            {
                return _items.TryGetValue(shortId, out result);
            }

            public Guid PreGenerateGuid()
            {
                return _items.PreGenerateGuid();
            }

            private void Items_Added(GuidDictionary<ScreenContext> sender, EventArgs<ScreenContext> e)
            {
                _owner.AddedScreen.Invoke(_owner, e);
            }

            private void Items_Removed(GuidDictionary<ScreenContext> sender, EventArgs<ScreenContext> e)
            {
                _owner.RemovedScreen.Invoke(_owner, e);
            }

            public IEnumerator<ScreenContext> GetEnumerator()
            {
                return _items.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
