using MCBS.BlockForms;
using MCBS.Screens;
using QuanLib.Game;
using QuanLib.IO.Extensions;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.UI.Extensions;

namespace MCBS.SystemApplications.Desktop
{
    public partial class DesktopForm : Form
    {
        public DesktopForm()
        {
            AllowDrag = false;
            AllowStretch = false;
            DisplayPriority = int.MinValue;
            MaxDisplayPriority = int.MinValue + 1;
            BorderWidth = 0;
            Skin.SetAllBackgroundColor(BlockManager.StainedGlass.Gray);

            DesktopIconTable_Panel = new(this);
        }

        private static readonly HashSet<string> _wallpaperExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".webp"];

        private Task<Image<Rgba32>>? _loadWallpaperTask;

        public DesktopPathManager DesktopPathManager
        {
            get
            {
                if (_DesktopPathManager is null)
                {
                    ScreenContext? screenContext = this.GetScreenContext() ?? throw new InvalidOperationException("无法获取屏幕上下文");
                    DirectoryInfo appsDir = McbsPathManager.MCBS_Applications;
                    string basePath = Path.Combine(appsDir.FullName, DesktopApp.Id, "ScreenData", screenContext.Guid.ToString());
                    _DesktopPathManager = new(basePath);
                }

                return _DesktopPathManager;
            }
        }
        private DesktopPathManager? _DesktopPathManager;

        private readonly DesktopIconTablePanel DesktopIconTable_Panel;

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(DesktopIconTable_Panel);
            DesktopIconTable_Panel.ClientSize = ClientSize;
            DesktopIconTable_Panel.Stretch = Direction.Bottom | Direction.Right;

            ReloadWallpaper();
        }

        protected override void OnAfterFrame(Control sender, EventArgs e)
        {
            base.OnAfterFrame(sender, e);

            if (_loadWallpaperTask is not null && _loadWallpaperTask.IsCompleted)
            {
                try
                {
                    Image<Rgba32> image = _loadWallpaperTask.Result;
                    _loadWallpaperTask = null;
                    Skin.SetAllBackgroundTexture(image);
                }
                catch
                {
                    Skin.SetAllBackgroundTexture(null);
                }
            }
        }

        private void ReloadWallpaper()
        {
            if (_loadWallpaperTask is not null)
                return;

            string[] wallpapers = GetWallpapers();
            if (wallpapers.Length > 0)
                _loadWallpaperTask = Image.LoadAsync<Rgba32>(wallpapers[0]);
            else
                Skin.SetAllBackgroundTexture(null);
        }

        private string[] GetWallpapers()
        {
            string wallpapersDir = DesktopPathManager.WallpapersDir;
            string[] files = Directory.Exists(wallpapersDir) ? Directory.GetFiles(wallpapersDir) : [];
            List<string> result = [];

            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);
                if (name == "Wallpaper" && _wallpaperExtensions.Contains(extension))
                    result.Add(file);
            }

            return result.ToArray();
        }
    }
}
