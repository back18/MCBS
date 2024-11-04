﻿using MCBS.Events;
using MCBS.UI;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.DialogBox
{
    public class MessageBoxForm : DialogBoxForm<MessageBoxButtons>
    {
        public MessageBoxForm(IForm initiator, string title, string message, MessageBoxButtons buttons) : base(initiator, title)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            _message = message;

            Text_MultilineLabel = new();
            Yes_Button = new();
            NO_Button = new();
            OK_Button = new();
            Cancel_Button = new();
            Retry_Button = new();

            _ButtonsToShow = buttons;
            DefaultResult = MessageBoxButtons.Cancel;
            DialogResult = DefaultResult;

            _pos1 = new(2, 50);
            _pos2 = new(38, 50);
            _pos3 = new(74, 50);
        }

        protected readonly string _message;

        private readonly Point _pos1, _pos2, _pos3;

        private readonly MultilineLabel Text_MultilineLabel;

        private readonly Button Yes_Button;

        private readonly Button NO_Button;

        private readonly Button OK_Button;

        private readonly Button Cancel_Button;

        private readonly Button Retry_Button;

        public MessageBoxButtons ButtonsToShow
        {
            get => _ButtonsToShow;
            set
            {
                _ButtonsToShow = value;
                ActiveLayoutAll();
            }
        }
        private MessageBoxButtons _ButtonsToShow;

        public override MessageBoxButtons DefaultResult { get; }

        public override MessageBoxButtons DialogResult { get; protected set; }

        public override void Initialize()
        {
            base.Initialize();

            ClientSize = new(110, 70 + TitleBar_Control.Height);
            CenterOnInitiatorForm();

            Home_PagePanel.ChildControls.Add(Text_MultilineLabel);
            Text_MultilineLabel.KeepWhenClear = true;
            Text_MultilineLabel.BorderWidth = 0;
            Text_MultilineLabel.ClientSize = new(110, 48);
            Text_MultilineLabel.Skin.SetAllBackgroundColor(BlockManager.Concrete.Lime);

            Yes_Button.Text = "是";
            Yes_Button.ClientSize = new(32, 16);
            Yes_Button.RightClick += Yes_Button_RightClick;

            NO_Button.Text = "否";
            NO_Button.ClientSize = new(32, 16);
            NO_Button.RightClick += NO_Button_RightClick;

            OK_Button.Text = "确认";
            OK_Button.ClientSize = new(32, 16);
            OK_Button.RightClick += OK_Button_RightClick;

            Cancel_Button.Text = "取消";
            Cancel_Button.ClientSize = new(32, 16);
            Cancel_Button.RightClick += Cancel_Button_RightClick;

            Retry_Button.Text = "重试";
            Retry_Button.ClientSize = new(32, 16);
            Retry_Button.RightClick += Retry_Button_RightClick;
        }

        public override void AfterInitialize()
        {
            base.AfterInitialize();

            Text_MultilineLabel.Text = _message;
            ActiveLayoutAll();
        }

        public override void ActiveLayoutAll()
        {
            Home_PagePanel.ChildControls.Clear();

            List<Button> buttons = new();
            if (ButtonsToShow.HasFlag(MessageBoxButtons.Yes))
                buttons.Add(Yes_Button);
            if (ButtonsToShow.HasFlag(MessageBoxButtons.No))
                buttons.Add(NO_Button);
            if (ButtonsToShow.HasFlag(MessageBoxButtons.OK))
                buttons.Add(OK_Button);
            if (ButtonsToShow.HasFlag(MessageBoxButtons.Cancel))
                buttons.Add(Cancel_Button);
            if (ButtonsToShow.HasFlag(MessageBoxButtons.Retry))
                buttons.Add(Retry_Button);

            switch (buttons.Count)
            {
                case 0:
                    return;
                case 1:
                    buttons[0].ClientLocation = _pos2;
                    break;
                case 2:
                    buttons[0].ClientLocation = _pos1;
                    buttons[1].ClientLocation = _pos3;
                    break;
                case >= 3:
                    buttons[0].ClientLocation = _pos1;
                    buttons[1].ClientLocation = _pos2;
                    buttons[2].ClientLocation = _pos3;
                    break;
            }

            for (int i = 0; i < Math.Min(3, buttons.Count); i++)
            {
                Home_PagePanel.ChildControls.Add(buttons[i]);
            }
        }

        private void Yes_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DialogResult = MessageBoxButtons.Yes;
            CloseForm();
        }

        private void NO_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DialogResult = MessageBoxButtons.No;
            CloseForm();
        }

        private void OK_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DialogResult = MessageBoxButtons.OK;
            CloseForm();
        }

        private void Cancel_Button_RightClick(Control sender, CursorEventArgs e)
        {
            CloseForm();
        }

        private void Retry_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DialogResult = MessageBoxButtons.Retry;
            CloseForm();
        }
    }
}
