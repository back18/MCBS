using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Core.Events;
using QuanLib.Game;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public partial class FileBrowserBox
    {
        public class DriveListPage : PagePanel
        {
            public DriveListPage(FileBrowserBox owner, string pageKey = "DriveList") : base(pageKey)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
            }

            private readonly FileBrowserBox _owner;

            public override void Initialize()
            {
                base.Initialize();

                UpdateList();
            }

            protected override void OnRightClick(Control sender, CursorEventArgs e)
            {
                base.OnRightClick(sender, e);

                DriveBox? driveBox = GetHoverDriveBox(e);
                if (driveBox is null)
                    return;

                driveBox.IsSelected = !driveBox.IsSelected;
            }

            protected override void OnDoubleRightClick(Control sender, CursorEventArgs e)
            {
                base.OnDoubleRightClick(sender, e);

                DriveBox? driveBox = GetHoverDriveBox(e);
                if (driveBox is null)
                    return;

                if (driveBox.IsSelected)
                    return;
                else
                    _owner.Text = driveBox.DriveInfo.RootDirectory.FullName;
            }

            public void UpdateList()
            {
                ChildControls.Clear();

                List<DriveBox> driveBoxes = [];
                foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
                {
                    DriveBox driveBox = new(driveInfo)
                    {
                        Width = ClientSize.Width - 2,
                        Stretch = Direction.Right
                    };
                    driveBoxes.Add(driveBox);
                    ChildControls.Add(driveBox);
                }

                if (driveBoxes.Count == 0)
                    return;

                LayoutHelper.FillLayoutDown(this, driveBoxes, 1);
                PageSize = new(ClientSize.Width, driveBoxes[^1].BottomLocation + 2);
                OffsetPosition = Point.Empty;
            }

            private DriveBox? GetHoverDriveBox(CursorEventArgs e)
            {
                ArgumentNullException.ThrowIfNull(e, nameof(e));

                return ChildControls
                    .GetHovers()
                    .Where(w => GetHoverCursors().Contains(e.CursorContext))
                    .OfType<DriveBox>()
                    .FirstOrDefault();
            }
        }
    }
}
