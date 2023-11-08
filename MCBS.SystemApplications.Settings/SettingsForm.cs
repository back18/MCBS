using MCBS;
using MCBS.BlockForms;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Minecraft.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Settings
{
    public class SettingsForm : WindowForm
    {
        public SettingsForm()
        {
            GetInteraction_Button = new();
            ClearInteraction_Button = new();
        }

        private readonly Button GetInteraction_Button;

        private readonly Button ClearInteraction_Button;

        public override void Initialize()
        {
            base.Initialize();

            ClientPanel_Control.ChildControls.Add(GetInteraction_Button);
            GetInteraction_Button.Text = "获取交互";
            GetInteraction_Button.ClientSize = new(64, 16);
            GetInteraction_Button.ClientLocation = new(2, 2);
            GetInteraction_Button.RightClick += GetInteraction_Button_RightClick;

            ClientPanel_Control.ChildControls.Add(ClearInteraction_Button);
            ClearInteraction_Button.Text = "清除交互";
            ClearInteraction_Button.ClientSize = new(64, 16);
            ClearInteraction_Button.LayoutDown(ClientPanel_Control, GetInteraction_Button, 2);
            ClearInteraction_Button.RightClick += ClearInteraction_Button_RightClick;
        }

        private void GetInteraction_Button_RightClick(Control sender, CursorEventArgs e)
        {
            if (MCOS.Instance.InteractionManager.Items.ContainsKey(e.CursorContext.PlayerName))
                return;

            MCOS.Instance.InteractionManager.Items.Add(e.CursorContext.PlayerName).CreateInteraction();
        }

        private void ClearInteraction_Button_RightClick(Control sender, CursorEventArgs e)
        {
            if (MCOS.Instance.InteractionManager.Items.TryGetValue(e.CursorContext.PlayerName, out var interactionContext))
                interactionContext.CloseInteraction();
        }
    }
}
