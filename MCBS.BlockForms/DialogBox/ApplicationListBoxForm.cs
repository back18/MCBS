using MCBS.Application;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.DialogBox
{
    public class ApplicationListBoxForm : DialogBoxForm<ApplicationManifest?>
    {
        public ApplicationListBoxForm(IForm initiator, string title) : base(initiator, title)
        {
            ApplicationList_ListMenuBox = new();
            OK_Button = new();
            Cancel_Button = new();

            DefaultResult = null;
            DialogResult = DefaultResult;
        }

        private readonly ListMenuBox<ApplicationItem> ApplicationList_ListMenuBox;

        private readonly Button OK_Button;

        private readonly Button Cancel_Button;

        public override ApplicationManifest? DefaultResult { get; }

        public override ApplicationManifest? DialogResult { get; protected set; }

        public override void Initialize()
        {
            base.Initialize();

            ClientSize = new(113, 86 + TitleBar.Height);
            CenterOnInitiatorForm();

            ClientPanel.ChildControls.Add(ApplicationList_ListMenuBox);
            ApplicationList_ListMenuBox.ClientLocation = new(2, 2);
            ApplicationList_ListMenuBox.ClientSize = new(107, 60);
            ApplicationList_ListMenuBox.RightClick += ApplicationList_ListMenuBox_RightClick;

            foreach (var appInfo in MCOS.Instance.AppComponents.Values)
            {
                ApplicationItem item = new(appInfo);
                item.ClientSize = new(96, 16);
                ApplicationList_ListMenuBox.AddedChildControlAndLayout(item);
            }

            ClientPanel.ChildControls.Add(Cancel_Button);
            Cancel_Button.Text = "取消";
            Cancel_Button.ClientSize = new(32, 16);
            Cancel_Button.LayoutLeft(this, ApplicationList_ListMenuBox.BottomLocation + 3, 2);
            Cancel_Button.RightClick += Cancel_Button_RightClick;

            ClientPanel.ChildControls.Add(OK_Button);
            OK_Button.Text = "确认";
            OK_Button.ClientSize = new(32, 16);
            OK_Button.LayoutLeft(ClientPanel, Cancel_Button, 2);
            OK_Button.RightClick += OK_Button_RightClick;
        }

        private void ApplicationList_ListMenuBox_RightClick(Control sender, CursorEventArgs e)
        {
            var selecteds = ApplicationList_ListMenuBox.ChildControls.GetSelecteds();
            Control? selected = ApplicationList_ListMenuBox.ChildControls.FirstSelected;
            if (selecteds.Length > 1 && selected is not null)
            {
                selected.IsSelected = false;
            }
        }

        private void OK_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DialogResult = (ApplicationList_ListMenuBox.ChildControls.FirstSelected as ApplicationItem)?.ApplicationManifest;
            CloseForm();
        }

        private void Cancel_Button_RightClick(Control sender, CursorEventArgs e)
        {
            CloseForm();
        }
    }
}
