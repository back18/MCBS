using MCBS;
using MCBS.Application;
using MCBS.BlockForms;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Core.Events;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MCBS.SystemApplications.Drawing
{
    public class DrawingForm : WindowForm
    {
        public DrawingForm(string? open = null)
        {
            _open = open;
            _save = open;

            Draw_Switch = new();
            Zoom_Switch = new();
            Drag_Switch = new();
            PenWidth_NumberBox = new();
            MoreMenu_Switch = new();
            More_ListMenuBox = new();
            Undo_Button = new();
            Redo_Button = new();
            FillButton = new();
            Create_Button = new();
            Open_Button = new();
            Save_Button = new();
            DrawingBox = new();

            MinSize = 4;
            MaxSize = 4096;
        }

        private string? _save;

        private readonly string? _open;

        private readonly Switch Draw_Switch;

        private readonly Switch Zoom_Switch;

        private readonly Switch Drag_Switch;

        private readonly NumberBox PenWidth_NumberBox;

        private readonly Switch MoreMenu_Switch;

        private readonly ListMenuBox<Button> More_ListMenuBox;

        private readonly Button Undo_Button;

        private readonly Button Redo_Button;

        private readonly Button FillButton;

        private readonly Button Create_Button;

        private readonly Button Open_Button;

        private readonly Button Save_Button;

        private readonly DrawingBox<Rgba32> DrawingBox;

        public int MinSize { get; set; }

        public int MaxSize { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            Home_PagePanel.Resize += ClientPanel_Resize;

            Home_PagePanel.ChildControls.Add(Draw_Switch);
            Draw_Switch.Text = "绘制";
            Draw_Switch.LayoutLeft(Home_PagePanel, 1, 1);
            Draw_Switch.Anchor = Direction.Top | Direction.Right;
            Draw_Switch.ControlSelected += Draw_Switch_ControlSelected;
            Draw_Switch.ControlDeselected += Draw_Switch_ControlDeselected;
            Draw_Switch.IsSelected = true;

            Home_PagePanel.ChildControls.Add(Zoom_Switch);
            Zoom_Switch.Text = "缩放";
            Zoom_Switch.LayoutDown(Home_PagePanel, Draw_Switch, 1);
            Zoom_Switch.Anchor = Direction.Top | Direction.Right;
            Zoom_Switch.ControlSelected += Zoom_Switch_ControlSelected;
            Zoom_Switch.ControlDeselected += Zoom_Switch_ControlDeselected;

            Home_PagePanel.ChildControls.Add(Drag_Switch);
            Drag_Switch.Text = "拖拽";
            Drag_Switch.LayoutDown(Home_PagePanel, Zoom_Switch, 1);
            Drag_Switch.Anchor = Direction.Top | Direction.Right;
            Drag_Switch.ControlSelected += Drag_Switch_ControlSelected;
            Drag_Switch.ControlDeselected += Drag_Switch_ControlDeselected;

            Home_PagePanel.ChildControls.Add(PenWidth_NumberBox);
            PenWidth_NumberBox.Skin.SetAllBackgroundColor(BlockManager.Concrete.Pink);
            PenWidth_NumberBox.MinNumberValue = 1;
            PenWidth_NumberBox.LayoutDown(Home_PagePanel, Drag_Switch, 1);
            PenWidth_NumberBox.Anchor = Direction.Top | Direction.Right;
            PenWidth_NumberBox.NumberValueChanged += PenWidth_NumberBox_NumberValueChanged;
            PenWidth_NumberBox.NumberValue = 5;

            Home_PagePanel.ChildControls.Add(MoreMenu_Switch);
            MoreMenu_Switch.Skin.SetBackgroundColor(BlockManager.Concrete.Yellow, ControlState.None, ControlState.Hover);
            MoreMenu_Switch.Skin.SetBackgroundColor(BlockManager.Concrete.Orange, ControlState.Selected, ControlState.Hover | ControlState.Selected);
            MoreMenu_Switch.OffText = "更多";
            MoreMenu_Switch.OnText = "隐藏";
            MoreMenu_Switch.LayoutDown(Home_PagePanel, PenWidth_NumberBox, 1);
            MoreMenu_Switch.Anchor = Direction.Top | Direction.Right;
            MoreMenu_Switch.ControlSelected += MoreMenu_Switch_ControlSelected;
            MoreMenu_Switch.ControlDeselected += MoreMenu_Switch_ControlDeselected; ;

            More_ListMenuBox.Size = new(45, MoreMenu_Switch.BottomLocation);
            More_ListMenuBox.Skin.SetAllBackgroundColor(string.Empty);
            More_ListMenuBox.Anchor = Direction.Top | Direction.Right;

            Undo_Button.Text = "撤销";
            Undo_Button.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Undo_Button.RightClick += Undo_Button_RightClick;
            More_ListMenuBox.AddedChildControlAndLayout(Undo_Button);

            Redo_Button.Text = "重做";
            Redo_Button.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Redo_Button.RightClick += Redo_Button_RightClick;
            More_ListMenuBox.AddedChildControlAndLayout(Redo_Button);

            FillButton.Text = "填充";
            FillButton.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            FillButton.RightClick += Fill_Button_RightClick;
            More_ListMenuBox.AddedChildControlAndLayout(FillButton);

            Create_Button.Text = "新建";
            Create_Button.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Create_Button.RightClick += Create_Button_RightClick;
            More_ListMenuBox.AddedChildControlAndLayout(Create_Button);

            Open_Button.Text = "打开";
            Open_Button.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Open_Button.RightClick += Open_Button_RightClick;
            More_ListMenuBox.AddedChildControlAndLayout(Open_Button);

            Save_Button.Text = "保存";
            Save_Button.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Save_Button.RightClick += Save_Button_RightClick;
            More_ListMenuBox.AddedChildControlAndLayout(Save_Button);

            Home_PagePanel.ChildControls.Add(DrawingBox);
            DrawingBox.ClientLocation = new(1, 1);
            DrawingBox.Size = new(Home_PagePanel.ClientSize.Width - Draw_Switch.Width - 3, Home_PagePanel.ClientSize.Height - 2);
            //DrawingBox.Stretch = Direction.Bottom | Direction.Right;
        }

        public override void AfterInitialize()
        {
            base.AfterInitialize();

            if (_open is not null)
                OpenImage(_open);
            else
                DrawingBox.SetImage(new Image<Rgba32>(DrawingBox.ClientSize.Width, DrawingBox.ClientSize.Height, DrawingBox.GetBlockColor<Rgba32>(BlockManager.Concrete.White)));
        }

        private void ClientPanel_Resize(Control sender, SizeChangedEventArgs e)
        {
            DrawingBox.Size = new(Home_PagePanel.ClientSize.Width - Draw_Switch.Width - 3, Home_PagePanel.ClientSize.Height - 2);
        }

        private void Draw_Switch_ControlSelected(Control sender, EventArgs e)
        {
            DrawingBox.EnableDraw = true;
        }

        private void Draw_Switch_ControlDeselected(Control sender, EventArgs e)
        {
            DrawingBox.EnableDraw = false;
        }

        private void Zoom_Switch_ControlSelected(Control sender, EventArgs e)
        {
            DrawingBox.EnableZoom = true;
        }

        private void Zoom_Switch_ControlDeselected(Control sender, EventArgs e)
        {
            DrawingBox.EnableZoom = false;
        }

        private void Drag_Switch_ControlSelected(Control sender, EventArgs e)
        {
            DrawingBox.EnableDrag = true;
        }

        private void Drag_Switch_ControlDeselected(Control sender, EventArgs e)
        {
            DrawingBox.EnableDrag = false;
        }

        private void PenWidth_NumberBox_NumberValueChanged(NumberBox sender, ValueChangedEventArgs<int> e)
        {
            DrawingBox.PenWidth = e.NewValue;
        }

        private void MoreMenu_Switch_ControlSelected(Control sender, EventArgs e)
        {
            Home_PagePanel.ChildControls.TryAdd(More_ListMenuBox);
            More_ListMenuBox.ClientLocation = new(sender.LeftLocation - More_ListMenuBox.Width - 1, sender.BottomLocation - More_ListMenuBox.Height + 1);
        }

        private void MoreMenu_Switch_ControlDeselected(Control sender, EventArgs e)
        {
            Home_PagePanel.ChildControls.Remove(More_ListMenuBox);
        }

        private void Undo_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DrawingBox.Undo();
        }

        private void Redo_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DrawingBox.Redo();
        }

        private void Fill_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DrawingBox.Fill(e);
        }

        private void Create_Button_RightClick(Control sender, CursorEventArgs e)
        {
            SizeSettingsBoxForm dialogBox = new(this, "输入尺寸", DrawingBox.Texture.GetOutputSize());
            _ = DialogBoxHelper.OpenDialogBoxAsync(this, dialogBox, (size) =>
            {
                if (size == dialogBox.DefaultResult)
                    return;

                if (size.Width < MinSize || size.Height < MinSize || size.Width > MaxSize || size.Height > MaxSize)
                {
                    _ = DialogBoxHelper.OpenMessageBoxAsync(this, "温馨提醒", $"图片尺寸需要在{MinSize}至{MaxSize}之间", MessageBoxButtons.OK);
                    return;
                }

                DrawingBox.SetImage(new Image<Rgba32>(size.Width, size.Height, DrawingBox.GetBlockColor<Rgba32>(string.Empty)));
                _save = null;
            });
        }

        private void Open_Button_RightClick(Control sender, CursorEventArgs e)
        {
            string? dir = MCOS.Instance.ProcessContextOf(this)?.Program.GetApplicationDirectory();
            if (string.IsNullOrEmpty(dir))
                return;
            dir = Path.Combine(dir, "Saves");

            MCOS.Instance.ProcessManager.StartProcess("System.FileExplorer", [dir], this);
        }

        private void Save_Button_RightClick(Control sender, CursorEventArgs e)
        {
            string? dir = MCOS.Instance.ProcessContextOf(this)?.Program.GetApplicationDirectory();
            if (string.IsNullOrEmpty(dir))
                return;
            dir = Path.Combine(dir, "Saves");

            if (_save is not null && _save.StartsWith(dir))
            {
                DrawingBox.Texture.ImageSource.Save(_save);
                _ = DialogBoxHelper.OpenMessageBoxAsync(this, "温馨提醒", "已成功保存", MessageBoxButtons.OK);
                return;
            }

            _ = DialogBoxHelper.OpenTextInputBoxAsync(this, "输入名称", (name) =>
            {
                bool save = false;
                if (string.IsNullOrEmpty(name))
                {
                    save = false;
                }
                else if (File.Exists(Path.Combine(dir, name + ".png")))
                {
                    DialogBoxHelper.OpenMessageBox(this, "警告", "文件已存在，是否覆盖？", MessageBoxButtons.OK | MessageBoxButtons.Cancel, (result) => save = result == MessageBoxButtons.OK);
                }
                else
                {
                    save = true;
                }

                if (save)
                {
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    _save = Path.Combine(dir, name + ".png");
                    DrawingBox.Texture.ImageSource.Save(_save);
                    DialogBoxHelper.OpenMessageBox(this, "温馨提醒", "已成功保存", MessageBoxButtons.OK);
                }
                else
                {
                    DialogBoxHelper.OpenMessageBox(this, "温馨提醒", "已取消保存", MessageBoxButtons.OK);
                }
            });
        }

        private void OpenImage(string path)
        {
            if (!DrawingBox.TryReadImageFile(path))
            {
                _ = DialogBoxHelper.OpenMessageBoxAsync(this, "警告", $"无法打开图片文件：“{path}”", MessageBoxButtons.OK);
            }
        }
    }
}
