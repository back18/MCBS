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

            ClientPanel = new();
        }

        private static readonly Image<Rgba32> _defaultWallpaper;

        public readonly ClientPanel ClientPanel;

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(ClientPanel);
            ClientPanel.ClientSize = ClientSize;
            ClientPanel.LayoutSyncer = new(this, (sender, e) => { }, (sender, e) =>
            ClientPanel.ClientSize = ClientSize);
            ClientPanel.LayoutAll += ClientPanel_LayoutAll;

            ActiveLayoutAll();

            ClientPanel.Skin.SetAllBackgroundTexture(_defaultWallpaper);
        }

        protected override BlockFrame Rendering()
        {
            return base.Rendering();
        }

        public override void ActiveLayoutAll()
        {
            ClientPanel.ChildControls.Clear();
            foreach (var applicationManifest in MCOS.Instance.AppComponents.Values)
            {
                if (!applicationManifest.IsBackground)
                    ClientPanel.ChildControls.Add(new DesktopIcon(applicationManifest));
            }

            if (ClientPanel.ChildControls.Count == 0)
                return;

            LayoutHelper.FillLayoutDownRight(this, ClientPanel.ChildControls, 0);
            ClientPanel.PageSize = new((ClientPanel.ChildControls.RecentlyAddedControl ?? ClientPanel.ChildControls[^1]).RightLocation + 1, ClientPanel.ClientSize.Height);
            ClientPanel.OffsetPosition = new(0, 0);
            ClientPanel.RefreshHorizontalScrollBar();
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
