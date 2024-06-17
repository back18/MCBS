using QuanLib.Core;
using QuanLib.Core.Events;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    /// <summary>
    /// 控件布局同步器
    /// </summary>
    public class LayoutSyncer
    {
        public LayoutSyncer(Control target, EventHandler<Control, ValueChangedEventArgs<Point>> move, EventHandler<Control, ValueChangedEventArgs<Size>> resize)
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));

            Target = target;
            Move = move;
            Resize = resize;
        }

        public Control Target { get; }

        public event EventHandler<Control, ValueChangedEventArgs<Point>> Move;

        public event EventHandler<Control, ValueChangedEventArgs<Size>> Resize;

        /// <summary>
        /// 绑定
        /// </summary>
        public void Binding()
        {
            Target.Move += Move;
            Target.Resize += Resize;
        }

        /// <summary>
        /// 解绑
        /// </summary>
        public void Unbinding()
        {
            Target.Move -= Move;
            Target.Resize -= Resize;
        }

        /// <summary>
        /// 主动调用同步委托
        /// </summary>
        public void Sync()
        {
            Move.Invoke(Target, new(Target.ClientLocation, Target.ClientLocation));
            Resize.Invoke(Target, new(Target.ClientSize, Target.ClientSize));
        }
    }
}
