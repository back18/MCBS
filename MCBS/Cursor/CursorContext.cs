using MCBS.Cursor.Style;
using MCBS.Screens;
using MCBS.UI;
using QuanLib.Minecraft.Snbt.Models;
using QuanLib.Minecraft.Vector;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor
{
    public class CursorContext
    {
        public CursorContext(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
                throw new ArgumentException($"“{nameof(playerName)}”不能为 null 或空。", nameof(playerName));

            PlayerName = playerName;
            LastActiveTick = -1;
            Visible = true;
            StyleType = CursorStyleType.Default;
            TextEditor = new(playerName);
            ClickReader = new(playerName);
            OldInputData = new();
            NewInputData = new();
            ScreenContextOf = null;
            HoverControls = new();
        }

        public string PlayerName { get; }

        public int LastActiveTick { get; private set; }

        public bool Active => LastActiveTick == MCOS.Instance.SystemTick;

        public CursorState CursorState
        {
            get
            {
                int tick = MCOS.Instance.SystemTick;
                if (LastActiveTick == tick)
                    return CursorState.Active;
                else if (LastActiveTick == tick - 1)
                    return CursorState.Ready;
                else
                    return CursorState.Offline;
            }
        }

        public bool Visible { get; set; }

        public string StyleType { get; set; }

        public TextEditor TextEditor { get; }

        public ClickReader ClickReader { get; }

        public CursorInputData OldInputData { get; private set; }

        public CursorInputData NewInputData { get; private set; }

        public ScreenContext? ScreenContextOf { get; private set; }

        public HoverControlCollection HoverControls { get; }

        internal void SetNewInputData(ScreenContext screenContext, CursorInputData inputData)
        {
            if (screenContext is null)
                throw new ArgumentNullException(nameof(screenContext));
            if (inputData is null)
                throw new ArgumentNullException(nameof(inputData));

            if (LastActiveTick != MCOS.Instance.SystemTick)
            {
                LastActiveTick = MCOS.Instance.SystemTick;
                OldInputData = NewInputData;
            }

            ScreenContextOf = screenContext;
            NewInputData = inputData;
        }
    }
}
