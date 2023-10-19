using MCBS.Frame;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public static class UIRenderer
    {
        public static async Task<ArrayFrame?> RenderingAsync(IControl control)
        {
            IControlRendering rendering = control;
            Task<ArrayFrame> mainTask;
            if (rendering.NeedRendering)
            {
                if (!rendering.Visible)
                    return null;

                if (rendering.ClientSize.Width < 0 || rendering.ClientSize.Height < 0)
                    return null;

                mainTask = Task.Run(() =>
                {
                    IFrame frame = rendering.RenderingFrame();
                    if (rendering.OffsetPosition != new Point(0, 0))
                        frame = frame.ToArrayFrame();

                    frame.CorrectSize(rendering.ClientSize, rendering.OffsetPosition, rendering.ContentAnchor, rendering.Skin.GetBackgroundBlockID());
                    return frame.ToArrayFrame();
                });
                _ = mainTask.ContinueWith((t) => rendering.HandleRenderingCompleted(new(t.Result.ToArrayFrame())));
            }
            else
            {
                mainTask = Task.Run(() => rendering.GetFrameCache() ?? throw new InvalidOperationException("无法获取帧缓存"));
            }

            var childs = (control as IContainerControl)?.GetChildControls();
            List<(IControlRendering rendering, Task<ArrayFrame?> task)> childTasks = new();
            if (childs is not null)
            {
                foreach (var child in childs)
                    childTasks.Add((child, RenderingAsync(child)));
            }

            ArrayFrame frame = await mainTask;
            if (childTasks.Count == 0)
            {
                if (rendering.NeedRendering)
                    rendering.HandleRenderingCompleted(new(frame));
                return frame;
            }

            await Task.WhenAll(childTasks.Select(i => i.task).ToArray());
            foreach (var (child, task) in childTasks)
            {
                if (task.Result is null)
                    continue;

                frame.Overwrite(task.Result, child.GetRenderingLocation(), rendering.OffsetPosition);
                frame.DrawBorder(child, child.GetRenderingLocation(), rendering.OffsetPosition);
            }

            return frame;
        }
    }
}
