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
            if (rendering.NeedRendering())
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
                if (rendering.NeedRendering())
                    rendering.HandleRenderingCompleted(new(frame));
                return frame;
            }

            await Task.WhenAll(childTasks.Select(i => i.task).ToArray());
            foreach (var (child, task) in childTasks)
            {
                if (task.Result is null)
                    continue;

                frame.Overwrite(task.Result, child.GetRenderingLocation(), rendering.OffsetPosition);
                DrawBorder(frame, child, rendering.OffsetPosition);
            }

            return frame;
        }

        private static void DrawBorder(ArrayFrame frame, IControlRendering rendering, Point offset)
        {
            if (rendering.BorderWidth > 0)
            {
                int width = rendering.ClientSize.Width + rendering.BorderWidth * 2;
                int heigth = rendering.ClientSize.Height + rendering.BorderWidth * 2;

                Point location = rendering.GetRenderingLocation();
                location = new(location.X - offset.X, location.Y - offset.Y);
                int startTop = location.Y - 1;
                int startBottom = location.Y + rendering.ClientSize.Height;
                int startLeft = location.X - 1;
                int startRigth = location.X + rendering.ClientSize.Width;
                int endTop = location.Y - rendering.BorderWidth;
                int endBottom = location.Y + rendering.ClientSize.Height + rendering.BorderWidth - 1;
                int endLeft = location.X - rendering.BorderWidth;
                int endRight = location.X + rendering.ClientSize.Width + rendering.BorderWidth - 1;

                string blockID = rendering.Skin.GetBorderBlockID();

                for (int y = startTop; y >= endTop; y--)
                    frame.DrawRow(y, endLeft, width, blockID);
                for (int y = startBottom; y <= endBottom; y++)
                    frame.DrawRow(y, endLeft, width, blockID);
                for (int x = startLeft; x >= endLeft; x--)
                    frame.DrawColumn(x, endTop, heigth, blockID);
                for (int x = startRigth; x <= endRight; x++)
                    frame.DrawColumn(x, endTop, heigth, blockID);
            }
        }
    }
}
