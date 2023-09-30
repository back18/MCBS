using MCBS.Events;
using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    public interface ITextEditor
    {
        /// <summary>
        /// 光标当前的位置
        /// </summary>
        public Point CurrentPosition { get; }

        /// <summary>
        /// 文本编辑器当前的文本
        /// </summary>
        public string CurrentText { get; }

        /// <summary>
        /// 文本编辑器的初始文本
        /// </summary>
        public string InitialText { get; set; }

        /// <summary>
        /// 当文本编辑器更新时
        /// </summary>
        public event EventHandler<ITextEditor, CursorTextEventArgs> TextEditorUpdate;
    }
}
