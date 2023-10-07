using QuanLib.Minecraft.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class CursorHoverText
    {
        public CursorHoverText(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException($"“{nameof(text)}”不能为 null 或空。", nameof(text));

            Text = text;

            ForegroundBlockID = BlockManager.Concrete.Black;
            BackgroundBlockID = BlockManager.Concrete.White;
            BorderBlockID = BlockManager.Concrete.Gray;
        }

        public string Text { get; set; }

        public string ForegroundBlockID { get; set; }

        public string BackgroundBlockID { get; set; }

        public string BorderBlockID { get; set; }
    }
}
