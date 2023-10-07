using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Snbt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor
{
    public class TextEditor
    {
        public TextEditor()
        {
            InitialText = string.Empty;
            CurrentText = string.Empty;
            IsSynchronized = false;
        }

        public string InitialText { get; private set; }

        public string CurrentText { get; private set; }

        public bool IsSynchronized { get; private set; }

        public bool ReadText(CommandSender sender, string player, Item item)
        {
            if (sender is null)
                throw new ArgumentNullException(nameof(sender));
            if (string.IsNullOrEmpty(player))
                throw new ArgumentException($"“{nameof(player)}”不能为 null 或空。", nameof(player));
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (!IsSynchronized)
            {
                if (string.IsNullOrEmpty(InitialText))
                    sender.SetPlayerHotbarItem(player, item.Slot, $"minecraft:writable_book{{pages:[]}}");
                else
                    sender.SetPlayerHotbarItem(player, item.Slot, $"minecraft:writable_book{{pages:[\"{InitialText}\"]}}");
                CurrentText = InitialText;
                IsSynchronized = true;
                return false;
            }
            else if (
                item.Tag is not null &&
                item.Tag.TryGetValue("pages", out var pagesTag) &&
                pagesTag is string[] pages && pages.Length > 0)
            {
                if (pages[0] != CurrentText)
                    CurrentText = pages[0];
                return true;
            }
            else if (!string.IsNullOrEmpty(CurrentText))
            {
                CurrentText = string.Empty;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetInitialText(string initialText)
        {
            InitialText = initialText;
            CurrentText = string.Empty;
            IsSynchronized = false;
        }
    }
}
