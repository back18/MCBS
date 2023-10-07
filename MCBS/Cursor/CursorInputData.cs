using QuanLib.Minecraft.Snbt.Models;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor
{
    public class CursorInputData
    {
        public CursorInputData()
        {
            CursorMode = CursorMode.Click;
            CursorPosition = Point.Empty;
            TextEditor = string.Empty;
            InventorySlot = 0;
            MainItem = null;
        }

        public CursorInputData(
            CursorMode cursorMode,
            Point cursorPosition,
            DateTime leftClickTime,
            DateTime rightClickTime,
            string textEditor,
            int inventorySlot,
            Item? mainItem,
            Item? deputyItem)
        {
            CursorMode = cursorMode;
            CursorPosition = cursorPosition;
            LeftClickTime = leftClickTime;
            RightClickTime = rightClickTime;
            TextEditor = textEditor;
            InventorySlot = inventorySlot;
            MainItem = mainItem;
            DeputyItem = deputyItem;
        }

        public CursorInputData(CursorInputData cursorInputData)
        {
            if (cursorInputData is null)
                throw new ArgumentNullException(nameof(cursorInputData));

            CursorMode = cursorInputData.CursorMode;
            CursorPosition = cursorInputData.CursorPosition;
            LeftClickTime = cursorInputData.LeftClickTime;
            RightClickTime = cursorInputData.RightClickTime;
            TextEditor = cursorInputData.TextEditor;
            InventorySlot = cursorInputData.InventorySlot;
            MainItem = cursorInputData.MainItem;
            DeputyItem = cursorInputData.DeputyItem;
        }

        public CursorMode CursorMode { get; internal set; }

        public Point CursorPosition { get; internal set; }

        public DateTime LeftClickTime { get; internal set; }

        public DateTime RightClickTime { get; internal set; }

        public string TextEditor { get; internal set; }

        public int InventorySlot { get; internal set; }

        public Item? MainItem { get; internal set; }

        public Item? DeputyItem { get; internal set; }

        public CursorInputData Clone()
        {
            return new(this);
        }
    }
}
