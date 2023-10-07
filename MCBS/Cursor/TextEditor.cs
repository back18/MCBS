using MCBS.Config;
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
        public TextEditor(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
                throw new ArgumentException($"“{nameof(playerName)}”不能为 null 或空。", nameof(playerName));

            PlayerName = playerName;
            InitialText = string.Empty;
            CurrentText = string.Empty;
            IsSynchronized = false;
        }

        public string PlayerName { get; }

        public string InitialText { get; private set; }

        public string CurrentText { get; private set; }

        public bool IsSynchronized { get; private set; }

        public bool ReadText(Item item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;

            if (!IsSynchronized)
            {
                if (string.IsNullOrEmpty(InitialText))
                    sender.SetPlayerHotbarItem(PlayerName, item.Slot, $"{ConfigManager.ScreenConfig.TextEditorItemID}{{pages:[]}}");
                else
                    sender.SetPlayerHotbarItem(PlayerName, item.Slot, $"{ConfigManager.ScreenConfig.TextEditorItemID}{{pages:[\"{InitialText}\"]}}");
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
