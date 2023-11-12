using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using System.Diagnostics;
using SixLabors.ImageSharp.PixelFormats;
using MCBS.BlockForms;
using MCBS.BlockForms.Utility;
using System.Reflection;
using MCBS.Events;
using MCBS.Rendering;

namespace MCBS.SystemApplications.Desktop
{
    public class DesktopForm : Form
    {
        static DesktopForm()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".SystemResource.DefaultWallpaper.jpg") ?? throw new InvalidOperationException();
            _defaultWallpaper = Image.Load<Rgba32>(stream);
        }

        public DesktopForm()
        {
            AllowDrag = false;
            AllowStretch = false;
            DisplayPriority = int.MinValue;
            MaxDisplayPriority = int.MinValue + 1;
            BorderWidth = 0;

            Icons_ScrollablePanel = new();
        }

        private static readonly Image<Rgba32> _defaultWallpaper;

        public readonly ScrollablePanel Icons_ScrollablePanel;

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(Icons_ScrollablePanel);
            Icons_ScrollablePanel.BorderWidth = 0;
            Icons_ScrollablePanel.ClientSize = ClientSize;
            Icons_ScrollablePanel.Stretch = Direction.Bottom | Direction.Right;
            Icons_ScrollablePanel.Skin.SetAllBackgroundTexture(_defaultWallpaper.Clone());
            Icons_ScrollablePanel.LayoutAll += ClientPanel_LayoutAll;

            ActiveLayoutAll();
        }

        public override void ActiveLayoutAll()
        {
            Icons_ScrollablePanel.ChildControls.Clear();
            foreach (var applicationManifest in MCOS.Instance.AppComponents.Values)
            {
                if (!applicationManifest.IsBackground)
                    Icons_ScrollablePanel.ChildControls.Add(new DesktopIcon(applicationManifest));
            }

            if (Icons_ScrollablePanel.ChildControls.Count == 0)
                return;

            LayoutHelper.FillLayoutDownRight(this, Icons_ScrollablePanel.ChildControls, 0);
            Icons_ScrollablePanel.PageSize = new((Icons_ScrollablePanel.ChildControls.RecentlyAddedControl ?? Icons_ScrollablePanel.ChildControls[^1]).RightLocation + 1, Icons_ScrollablePanel.ClientSize.Height);
            Icons_ScrollablePanel.OffsetPosition = new(0, 0);
            Icons_ScrollablePanel.RefreshHorizontalScrollBar();
        }

        [Obsolete("暂时无法设置壁纸", true)]
        public void SetAsWallpaper(Image<Rgba32> image)
        {
            throw new NotSupportedException();
        }

        private void ClientPanel_LayoutAll(AbstractControlContainer<Control> sender, SizeChangedEventArgs e)
        {
            ActiveLayoutAll();
        }
    }
}
