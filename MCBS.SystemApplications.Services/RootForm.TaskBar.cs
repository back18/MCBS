using MCBS.BlockForms;
using MCBS.BlockForms.Utility;
using MCBS.Cursor;
using MCBS.Cursor.Style;
using MCBS.Events;
using MCBS.Forms;
using MCBS.Screens;
using MCBS.UI;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Services
{
    public partial class RootForm
    {
        public class TaskBar : ContainerControl<Control>
        {
            public TaskBar(RootForm owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;

                InvokeExternalCursorMove = true;
                BorderWidth = 0;
                Height = 18;
                Skin.SetAllBackgroundColor(BlockManager.Concrete.White);

                _draggingCursors = new();
                StartMenu_Switch = new();
                TaskBarIconMenu_Control = new();
                FullScreen_Button = new();
            }

            private readonly Dictionary<string, DragContext> _draggingCursors;

            private readonly RootForm _owner;

            private readonly Switch StartMenu_Switch;

            private readonly Button FullScreen_Button;

            private readonly TaskBarIconMenu TaskBarIconMenu_Control;

            public override void Initialize()
            {
                base.Initialize();

                if (_owner != ParentContainer)
                    throw new InvalidOperationException();

                ChildControls.Add(StartMenu_Switch);
                StartMenu_Switch.BorderWidth = 0;
                StartMenu_Switch.ClientLocation = new(0, 1);
                StartMenu_Switch.ClientSize = new(16, 16);
                StartMenu_Switch.Anchor = Direction.Bottom | Direction.Left;
                StartMenu_Switch.FirstHandleRightClick = true;
                StartMenu_Switch.IsRenderingTransparencyTexture = false;
                StartMenu_Switch.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Hover);
                StartMenu_Switch.Skin.SetBackgroundColor(BlockManager.Concrete.Orange, ControlState.Selected, ControlState.Hover | ControlState.Selected);
                StartMenu_Switch.Skin.SetAllBackgroundTexture(TextureManager.Instance["Logo"]);
                StartMenu_Switch.ControlSelected += StartMenu_Switch_ControlSelected;
                StartMenu_Switch.ControlDeselected += StartMenu_Switch_ControlDeselected; ;

                ChildControls.Add(FullScreen_Button);
                FullScreen_Button.BorderWidth = 0;
                FullScreen_Button.ClientSize = new(16, 16);
                FullScreen_Button.LayoutLeft(this, 1, 0);
                FullScreen_Button.Anchor = Direction.Bottom | Direction.Right;
                FullScreen_Button.FirstHandleRightClick = true;
                FullScreen_Button.IsRenderingTransparencyTexture = false;
                FullScreen_Button.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                FullScreen_Button.Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                FullScreen_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Expand"]);
                FullScreen_Button.RightClick += HideTitleBar_Button_RightClick;

                ChildControls.Add(TaskBarIconMenu_Control);
                TaskBarIconMenu_Control.Spacing = 0;
                TaskBarIconMenu_Control.MinWidth = 18;
                TaskBarIconMenu_Control.BorderWidth = 0;
                TaskBarIconMenu_Control.ClientSize = new(ClientSize.Width - StartMenu_Switch.Width - FullScreen_Button.Width, ClientSize.Height);
                TaskBarIconMenu_Control.ClientLocation = new(StartMenu_Switch.RightLocation + 1, 0);
                TaskBarIconMenu_Control.Stretch = Direction.Right;

                _owner.FormContainer_Control.AddedChildControl += FormContainer_AddedChildControl;
                _owner.FormContainer_Control.RemovedChildControl += FormContainer_RemovedChildControl;
            }

            protected override void OnCursorMove(Control sender, CursorEventArgs e)
            {
                base.OnCursorMove(sender, e);

                if (!_draggingCursors.TryGetValue(e.CursorContext.PlayerName, out var dragContext))
                    return;

                if (e.Position.Y < -32 ||
                    e.Position.X < -32 ||
                    e.Position.Y > ClientSize.Height + 32 ||
                    e.Position.X > ClientSize.Width + 32)
                {
                    _draggingCursors.Remove(e.CursorContext.PlayerName);
                    if (_draggingCursors.Count == 0)
                        TaskBarIconMenu_Control.FirstHandleCursorSlotChanged = true;
                    return;
                }

                Point position = _owner.ClientLocation;
                Point offset = new(e.Position.X - dragContext.AnchorPosition.X, e.Position.Y - dragContext.AnchorPosition.Y);
                position.Offset(offset);
                _owner.ClientLocation = position;
                e.CursorContext.StyleType = CursorStyleType.Default;
            }

            protected override void OnRightClick(Control sender, CursorEventArgs e)
            {
                base.OnRightClick(sender, e);

                if (!_owner.AllowDrag)
                    return;

                if (TaskBarIconMenu_Control.ChildControls.FirstHover is not null)
                    return;

                string player = e.CursorContext.PlayerName;
                if (_draggingCursors.ContainsKey(player))
                    _draggingCursors.Remove(player);
                else
                    _draggingCursors.Add(player, new(e.CursorContext, e.Position));

                if (_draggingCursors.Count > 0)
                    TaskBarIconMenu_Control.FirstHandleCursorSlotChanged = false;
                else
                    TaskBarIconMenu_Control.FirstHandleCursorSlotChanged = true;
            }

            protected override void OnCursorSlotChanged(Control sender, CursorEventArgs e)
            {
                base.OnCursorSlotChanged(sender, e);

                if (!_draggingCursors.ContainsKey(e.CursorContext.PlayerName))
                    return;

                ScreenContext? screenContext = MinecraftBlockScreen.Instance.ScreenContextOf(_owner);
                if (screenContext is null)
                    return;

                screenContext.ScreenOutputHandler.FillAirBlock();
                screenContext.Screen.OffsetPlaneCoordinate(e.InventorySlotDelta);
                screenContext.ScreenOutputHandler.ResetBuffer();
            }

            public void SwitchSelectedForm(IForm form)
            {
                TaskBarIconMenu_Control.SwitchSelectedForm(form);
            }

            private void StartMenu_Switch_ControlSelected(Control sender, EventArgs e)
            {
                _owner.ChildControls.TryAdd(_owner.StartMenu_ListMenuBox);

                _owner.StartMenu_ListMenuBox.ClientLocation = new(0, Math.Max(_owner.ClientSize.Height - _owner.TaskBar_Control.Height - _owner.StartMenu_ListMenuBox.Height, 0));
                if (_owner.StartMenu_ListMenuBox.BottomToBorder < _owner.TaskBar_Control.Height)
                    _owner.StartMenu_ListMenuBox.BottomToBorder = _owner.TaskBar_Control.Height;

                //if (MCOS.Instance.ScreenContextOf(_owner)?.Screen.TestLight() ?? false)
                //    _owner.Light_Switch.IsSelected = false;
                //else
                //    _owner.Light_Switch.IsSelected = true;
            }

            private void StartMenu_Switch_ControlDeselected(Control sender, EventArgs e)
            {
                _owner.ChildControls.Remove(_owner.StartMenu_ListMenuBox);
            }

            private void FormContainer_AddedChildControl(AbstractControlContainer<IControl> sender, ControlEventArgs<IControl> e)
            {
                if (e.Control is not IForm form)
                    return;

                var applicationManifest = MinecraftBlockScreen.Instance.ProcessContextOf(form)?.Application;
                if (applicationManifest is null || applicationManifest.IsBackground)
                    return;

                var context = MinecraftBlockScreen.Instance.FormContextOf(form);
                if (context is null)
                    return;

                switch (context.StateManager.CurrentState)
                {
                    case FormState.NotLoaded:
                    case FormState.Dragging:
                        TaskBarIconMenu_Control.AddedChildControlAndLayout(new TaskBarIcon(form));
                        break;
                    case FormState.Minimize:
                        var icon = TaskBarIconMenu_Control.TaskBarIconOf(form);
                        if (icon is not null)
                            icon.IsSelected = true;
                        break;
                }
            }

            private void FormContainer_RemovedChildControl(AbstractControlContainer<IControl> sender, ControlEventArgs<IControl> e)
            {
                if (e.Control is not IForm form)
                    return;

                var context = MinecraftBlockScreen.Instance.FormContextOf(form);
                var icon = TaskBarIconMenu_Control.TaskBarIconOf(form);
                if (context is null || icon is null)
                    return;

                switch (context.StateManager.NextState)
                {
                    case FormState.Minimize:
                        icon.IsSelected = false;
                        break;
                    case FormState.Dragging:
                    case FormState.Closed:
                        TaskBarIconMenu_Control.RemoveChildControlAndLayout(icon);
                        break;
                }
            }

            private void HideTitleBar_Button_RightClick(Control sender, CursorEventArgs e)
            {
                _owner.ShowTaskBar = false;
            }

            private class DragContext(CursorContext cursorContext, Point anchorPosition)
            {
                public CursorContext CursorContext { get; } = cursorContext;

                public Point AnchorPosition { get; } = anchorPosition;
            }
        }
    }
}
