using MCBS.BlockForms;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.Rendering;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Album
{
    public class AlbumForm : WindowForm
    {
        public AlbumForm(string? path = null)
        {
            _open = path;
            _extensions = new()
            {
                "jpg",
                "jpeg",
                "png",
                "bmp",
                "webp"
            };

            Setting_Switch = new();
            Path_TextBox = new();
            PreviousImage_Button = new();
            NextImage_Button = new();
            ScalablePictureBox = new();
            Setting_ListMenuBox = new();
            ResizeMode_ComboButton = new();
            AnchorPositionMode_ComboButton = new();
            Resampler_ComboButton = new();
            SetAsWallpaper_Button = new();

            OverlayShowTime = 20;
            OverlayHideTime = 0;
        }

        private readonly string? _open;

        private readonly List<string> _extensions;

        private FileList? _images;

        private readonly ScalablePictureBox<Rgba32> ScalablePictureBox;

        private readonly Switch Setting_Switch;

        private readonly TextBox Path_TextBox;

        private readonly Button PreviousImage_Button;

        private readonly Button NextImage_Button;

        private readonly ListMenuBox<Control> Setting_ListMenuBox;

        private readonly ComboButton<ResizeMode> ResizeMode_ComboButton;

        private readonly ComboButton<AnchorPositionMode> AnchorPositionMode_ComboButton;

        private readonly ComboButton<IResampler> Resampler_ComboButton;

        private readonly Button SetAsWallpaper_Button;

        public int OverlayShowTime { get; set; }

        public int OverlayHideTime { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            Home_PagePanel.Resize += ClientPanel_Resize;

            Home_PagePanel.ChildControls.Add(ScalablePictureBox);
            ScalablePictureBox.EnableZoom = true;
            ScalablePictureBox.EnableDrag = true;
            ScalablePictureBox.BorderWidth = 0;
            ScalablePictureBox.ClientSize = Home_PagePanel.ClientSize;
            ScalablePictureBox.Resize += ScalablePictureBox_Resize;
            ScalablePictureBox.TextureChanged += ScalablePictureBox_TextureChanged;

            Home_PagePanel.ChildControls.Add(Setting_Switch);
            Setting_Switch.OffText = "设置";
            Setting_Switch.OnText = "应用";
            Setting_Switch.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Setting_Switch.Skin.SetBackgroundColor(BlockManager.Concrete.Yellow, ControlState.Hover);
            Setting_Switch.Skin.SetBackgroundColor(BlockManager.Concrete.Orange, ControlState.Selected, ControlState.Hover | ControlState.Selected);
            Setting_Switch.ClientLocation = new(2, 2);
            Setting_Switch.ControlSelected += Setting_Switch_ControlSelected;
            Setting_Switch.ControlDeselected += Setting_Switch_ControlDeselected;

            Home_PagePanel.ChildControls.Add(Path_TextBox);
            Path_TextBox.LayoutRight(Home_PagePanel, Setting_Switch, 2);
            Path_TextBox.Width = Home_PagePanel.ClientSize.Width - Setting_Switch.Width - 6;
            Path_TextBox.Stretch = Direction.Right;
            Path_TextBox.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Path_TextBox.TextChanged += Path_TextBox_TextChanged;

            Home_PagePanel.ChildControls.Add(PreviousImage_Button);
            PreviousImage_Button.Text = "<";
            PreviousImage_Button.ClientSize = new(16, 16);
            PreviousImage_Button.LayoutSyncer = new(Home_PagePanel, (sender, e) => { }, (sender, e) =>
            PreviousImage_Button.LayoutVerticalCentered(Home_PagePanel, 2));
            PreviousImage_Button.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            PreviousImage_Button.RightClick += PreviousImage_Button_RightClick;

            Home_PagePanel.ChildControls.Add(NextImage_Button);
            NextImage_Button.Text = ">";
            NextImage_Button.ClientSize = new(16, 16);
            NextImage_Button.LayoutSyncer = new(Home_PagePanel, (sender, e) => { }, (sender, e) =>
            NextImage_Button.LayoutVerticalCentered(Home_PagePanel, Home_PagePanel.ClientSize.Width - NextImage_Button.Width - 3));
            NextImage_Button.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            NextImage_Button.RightClick += NextImage_Button_RightClick;

            Setting_ListMenuBox.ClientSize = new(128, 20 * 3 + 2);
            Setting_ListMenuBox.Spacing = 2;
            Setting_ListMenuBox.LayoutDown(Home_PagePanel, Setting_Switch, 2);
            Setting_ListMenuBox.Skin.SetAllBackgroundColor(string.Empty);

            int width = Setting_ListMenuBox.ClientSize.Width - 4;

            ResizeMode_ComboButton.Width = width;
            ResizeMode_ComboButton.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            ResizeMode_ComboButton.Title = "模式";
            ResizeMode_ComboButton.Items.AddRenge(EnumUtil.ToArray<ResizeMode>());
            ResizeMode_ComboButton.Items.SelectedItem = ScalablePictureBox.DefaultResizeOptions.Mode;
            ResizeMode_ComboButton.Items.ItemToStringFunc = (item) =>
            {
                return item switch
                {
                    ResizeMode.Crop => "裁剪",
                    ResizeMode.Pad => "填充",
                    ResizeMode.BoxPad => "盒式填充",
                    ResizeMode.Max => "最大",
                    ResizeMode.Min => "最小",
                    ResizeMode.Stretch => "拉伸",
                    ResizeMode.Manual => "手动",
                    _ => throw new InvalidOperationException(),
                };
            };
            Setting_ListMenuBox.AddedChildControlAndLayout(ResizeMode_ComboButton);

            AnchorPositionMode_ComboButton.Width = width;
            AnchorPositionMode_ComboButton.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            AnchorPositionMode_ComboButton.Title = "锚点";
            AnchorPositionMode_ComboButton.Items.AddRenge(EnumUtil.ToArray<AnchorPositionMode>());
            AnchorPositionMode_ComboButton.Items.SelectedItem = ScalablePictureBox.DefaultResizeOptions.Position;
            AnchorPositionMode_ComboButton.Items.ItemToStringFunc = (item) =>
            {
                return item switch
                {
                    AnchorPositionMode.Center => "中心",
                    AnchorPositionMode.Top => "顶部",
                    AnchorPositionMode.Bottom => "底部",
                    AnchorPositionMode.Left => "左侧",
                    AnchorPositionMode.Right => "右侧",
                    AnchorPositionMode.TopLeft => "左上角",
                    AnchorPositionMode.TopRight => "右上角",
                    AnchorPositionMode.BottomRight => "右下角",
                    AnchorPositionMode.BottomLeft => "左下角",
                    _ => throw new InvalidOperationException(),
                };
            };
            Setting_ListMenuBox.AddedChildControlAndLayout(AnchorPositionMode_ComboButton);

            Resampler_ComboButton.Width = width;
            Resampler_ComboButton.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Resampler_ComboButton.Title = "算法";
            Resampler_ComboButton.Items.Add(KnownResamplers.Bicubic, nameof(KnownResamplers.Bicubic));
            Resampler_ComboButton.Items.Add(KnownResamplers.Box, nameof(KnownResamplers.Box));
            Resampler_ComboButton.Items.Add(KnownResamplers.CatmullRom, nameof(KnownResamplers.CatmullRom));
            Resampler_ComboButton.Items.Add(KnownResamplers.Hermite, nameof(KnownResamplers.Hermite));
            Resampler_ComboButton.Items.Add(KnownResamplers.Lanczos2, nameof(KnownResamplers.Lanczos2));
            Resampler_ComboButton.Items.Add(KnownResamplers.Lanczos3, nameof(KnownResamplers.Lanczos3));
            Resampler_ComboButton.Items.Add(KnownResamplers.Lanczos5, nameof(KnownResamplers.Lanczos5));
            Resampler_ComboButton.Items.Add(KnownResamplers.Lanczos8, nameof(KnownResamplers.Lanczos8));
            Resampler_ComboButton.Items.Add(KnownResamplers.MitchellNetravali, nameof(KnownResamplers.MitchellNetravali));
            Resampler_ComboButton.Items.Add(KnownResamplers.NearestNeighbor, nameof(KnownResamplers.NearestNeighbor));
            Resampler_ComboButton.Items.Add(KnownResamplers.Robidoux, nameof(KnownResamplers.Robidoux));
            Resampler_ComboButton.Items.Add(KnownResamplers.RobidouxSharp, nameof(KnownResamplers.RobidouxSharp));
            Resampler_ComboButton.Items.Add(KnownResamplers.Spline, nameof(KnownResamplers.Spline));
            Resampler_ComboButton.Items.SelectedItem = ScalablePictureBox.DefaultResizeOptions.Sampler;
            Setting_ListMenuBox.AddedChildControlAndLayout(Resampler_ComboButton);

            SetAsWallpaper_Button.Width = width;
            SetAsWallpaper_Button.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            SetAsWallpaper_Button.Text = "设为壁纸";
            SetAsWallpaper_Button.RightClick += SetAsWallpaper_Button_RightClick;
            Setting_ListMenuBox.AddedChildControlAndLayout(SetAsWallpaper_Button);
        }

        public override void AfterInitialize()
        {
            base.AfterInitialize();

            if (_open is not null)
                Path_TextBox.Text = _open;
            else
                ScalablePictureBox.SetImage(new Image<Rgba32>(Home_PagePanel.Width, Home_PagePanel.Height, ScalablePictureBox.GetBlockColor<Rgba32>(BlockManager.Concrete.White)));
        }

        protected override void OnCursorMove(Control sender, CursorEventArgs e)
        {
            base.OnCursorMove(sender, e);

            ShowOverlay();
            OverlayHideTime = OverlayShowTime;
        }

        protected override void OnRightClick(Control sender, CursorEventArgs e)
        {
            if (Home_PagePanel.ChildControls.FirstHover is null or ScalablePictureBox<Rgba32>)
            {
                if (Setting_Switch.Visible)
                {
                    HideOverlay();
                    OverlayHideTime = 0;
                }
                else
                {
                    ShowOverlay();
                    OverlayHideTime = OverlayShowTime;
                }
            }
        }

        protected override void OnBeforeFrame(Control sender, EventArgs e)
        {
            base.OnBeforeFrame(sender, e);

            if (Home_PagePanel.ChildControls.FirstHover is null or BlockForms.ScalablePictureBox<Rgba32>)
            {
                if (OverlayHideTime <= 0)
                    HideOverlay();
                OverlayHideTime--;
            }
        }

        private void ShowOverlay()
        {
            Setting_Switch.Visible = true;
            Path_TextBox.Visible = true;
            PreviousImage_Button.Visible = true;
            NextImage_Button.Visible = true;
        }

        private void HideOverlay()
        {
            Setting_Switch.Visible = false;
            Path_TextBox.Visible = false;
            PreviousImage_Button.Visible = false;
            NextImage_Button.Visible = false;
        }

        private void ClientPanel_Resize(Control sender, SizeChangedEventArgs e)
        {
            ScalablePictureBox.ClientSize = Home_PagePanel.ClientSize;
        }

        private void ScalablePictureBox_Resize(Control sender, SizeChangedEventArgs e)
        {
            ScalablePictureBox.LayoutCentered(Home_PagePanel);
        }

        private void ScalablePictureBox_TextureChanged(PictureBox<Rgba32> sender, TextureChangedEventArgs<Rgba32> e)
        {
            ScalablePictureBox.ClientSize = Home_PagePanel.ClientSize;
        }

        private void Setting_Switch_ControlSelected(Control sender, EventArgs e)
        {
            Home_PagePanel.ChildControls.TryAdd(Setting_ListMenuBox);
        }

        private void Setting_Switch_ControlDeselected(Control sender, EventArgs e)
        {
            ApplySetting(ScalablePictureBox.DefaultResizeOptions);
            ApplySetting(ScalablePictureBox.Texture.ResizeOptions);
            ScalablePictureBox.AutoSetSize();
            Home_PagePanel.ChildControls.Remove(Setting_ListMenuBox);
        }

        private void Path_TextBox_TextChanged(Control sender, TextChangedEventArgs e)
        {
            if (SR.DefaultFont.GetTotalSize(e.NewText).Width > Path_TextBox.ClientSize.Width)
                Path_TextBox.ContentAnchor = AnchorPosition.UpperRight;
            else
                Path_TextBox.ContentAnchor = AnchorPosition.UpperLeft;

            if (ScalablePictureBox.TryReadImageFile(e.NewText))
            {
                if (_images is null || !_images.Contains(e.NewText))
                    _images = FileList.LoadFile(e.NewText, _extensions);
            }
            else
            {
                _ = DialogBoxHelper.OpenMessageBoxAsync(this, "警告", $"无法打开图片文件：“{e.NewText}”", MessageBoxButtons.OK);
            }
        }

        private void PreviousImage_Button_RightClick(Control sender, CursorEventArgs e)
        {
            string? file = _images?.GetPrevious();
            if (file is not null)
                Path_TextBox.Text = file;
        }

        private void NextImage_Button_RightClick(Control sender, CursorEventArgs e)
        {
            string? file = _images?.GetNext();
            if (file is not null)
                Path_TextBox.Text = file;
        }

        private void SetAsWallpaper_Button_RightClick(Control sender, CursorEventArgs e)
        {
            foreach (var context in MinecraftBlockScreen.Instance.FormManager.Items.Values)
            {
                //if (context.Form is DesktopForm desktop)
                //    desktop.SetAsWallpaper(ScalablePictureBox.ImageFrame.Image);
                //TODO: 需要进一步完善应用程序依赖机制
            }
        }

        private void ApplySetting(ResizeOptions options)
        {
            options.Mode = ResizeMode_ComboButton.Items.SelectedItem;
            options.Position = AnchorPositionMode_ComboButton.Items.SelectedItem;
            options.Sampler = Resampler_ComboButton.Items.SelectedItem ?? OptionsUtil.CreateDefaultResizeOption().Sampler;
        }
    }
}
