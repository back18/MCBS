using MCBS.Cursor.Style;
using MCBS.Screens;
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
            ArgumentException.ThrowIfNullOrEmpty(playerName, nameof(playerName));

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

        public bool Active => LastActiveTick == MinecraftBlockScreen.Instance.SystemTick;

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
            ArgumentNullException.ThrowIfNull(screenContext, nameof(screenContext));
            ArgumentNullException.ThrowIfNull(inputData, nameof(inputData));

            if (LastActiveTick != MinecraftBlockScreen.Instance.SystemTick)
            {
                LastActiveTick = MinecraftBlockScreen.Instance.SystemTick;
                OldInputData = NewInputData;
            }

            ScreenContextOf = screenContext;
            NewInputData = inputData;
        }
    }
}
