using MCBS.Cursor.Style;
using MCBS.Screens;
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
            LastActiveFrame = -1;
            Visible = true;
            StyleType = CursorStyleType.Default;
            TextEditor = new(playerName);
            ClickReader = new(playerName);
            InputData = new();
            ScreenContextOf = null;
            HoverControl = null;
        }

        public string PlayerName { get; }

        public int LastActiveFrame { get; private set; }

        public bool Active { get; private set; }

        public CursorState CursorState
        {
            get
            {
                int tick = MCOS.Instance.SystemTick;
                if (LastActiveFrame == tick)
                    return CursorState.Active;
                else if (LastActiveFrame == tick - 1)
                    return CursorState.Ready;
                else
                    return CursorState.Offline;
            }
        }

        public bool Visible { get; set; }

        public string StyleType { get; set; }

        public TextEditor TextEditor { get; }

        public ClickReader ClickReader { get; }

        public CursorInputData InputData { get; private set; }

        public ScreenContext? ScreenContextOf { get; private set; }

        public CursorHoverControl? HoverControl { get; set; }

        public void SetNewInputData(ScreenContext screenContext, CursorInputData inputData)
        {
            ScreenContextOf = screenContext ?? throw new ArgumentNullException(nameof(screenContext));
            InputData = inputData ?? throw new ArgumentNullException(nameof(inputData));
            LastActiveFrame = MCOS.Instance.SystemTick;
        }
    }
}
