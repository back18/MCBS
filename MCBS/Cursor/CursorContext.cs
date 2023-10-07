using MCBS.Cursor.Style;
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
            Visible = true;
            StyleType = CursorStyleType.Default;
            TextEditor = new();
            InputData = new();
            HoverControl = null;
        }

        public string PlayerName { get; }

        public bool Active { get; private set; }

        public bool Visible { get; set; }

        public string StyleType { get; set; }

        public TextEditor TextEditor { get; }

        public CursorInputData InputData { get; private set; }

        public CursorHoverControl? HoverControl { get; set; }

        public void SetNewInputData(CursorInputData inputData)
        {
            InputData = inputData ?? throw new ArgumentNullException(nameof(inputData));
            Active = true;
        }

        public void Reset()
        {
            Active = false;
        }
    }
}
