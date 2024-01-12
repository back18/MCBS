using QuanLib.Minecraft.NBT.Models;
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
            LeftClickPosition = Point.Empty;
            RightClickPosition = Point.Empty;
            LeftClickTime = DateTime.MinValue;
            RightClickTime = DateTime.MinValue;
            TextEditor = string.Empty;
            InventorySlot = 0;
            MainItem = null;
            DeputyItem = null;
        }

        public CursorInputData(
            CursorMode cursorMode,
            Point cursorPosition,
            Point leftClickPosition,
            Point rightClickPosition,
            DateTime leftClickTime,
            DateTime rightClickTime,
            string textEditor,
            int inventorySlot,
            Item? mainItem,
            Item? deputyItem)
        {
            CursorMode = cursorMode;
            CursorPosition = cursorPosition;
            LeftClickPosition = leftClickPosition;
            RightClickPosition = rightClickPosition;
            LeftClickTime = leftClickTime;
            RightClickTime = rightClickTime;
            TextEditor = textEditor;
            InventorySlot = inventorySlot;
            MainItem = mainItem;
            DeputyItem = deputyItem;
        }

        public CursorInputData(CursorInputData cursorInputData)
        {
            ArgumentNullException.ThrowIfNull(cursorInputData, nameof(cursorInputData));

            CursorMode = cursorInputData.CursorMode;
            CursorPosition = cursorInputData.CursorPosition;
            LeftClickPosition = cursorInputData.LeftClickPosition;
            RightClickPosition = cursorInputData.RightClickPosition;
            LeftClickTime = cursorInputData.LeftClickTime;
            RightClickTime = cursorInputData.RightClickTime;
            TextEditor = cursorInputData.TextEditor;
            InventorySlot = cursorInputData.InventorySlot;
            MainItem = cursorInputData.MainItem;
            DeputyItem = cursorInputData.DeputyItem;
        }

        public CursorMode CursorMode { get; internal set; }

        public Point CursorPosition { get; internal set; }

        public Point LeftClickPosition { get; internal set; }

        public Point RightClickPosition { get; internal set; }

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
