﻿using MCBS.Event;
using MCBS.UI;
using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class AbstractContainer<TControl> : Control, IContainerControl where TControl : class, IControl
    {
        protected AbstractContainer()
        {
            AddedChildControl += OnAddedChildControl;
            RemovedChildControl += OnRemovedChildControl;
        }

        public Type ChildControlType => typeof(TControl);

        public bool IsChildControlType<T>() => typeof(T) == ChildControlType;

        public abstract event EventHandler<AbstractContainer<TControl>, ControlEventArgs<TControl>> AddedChildControl;

        public abstract event EventHandler<AbstractContainer<TControl>, ControlEventArgs<TControl>> RemovedChildControl;

        protected virtual void OnAddedChildControl(AbstractContainer<TControl> sender, ControlEventArgs<TControl> e)
        {
            IControlInitializeHandling handling = e.Control;
            if (IsInitializeCompleted && !handling.IsInitializeCompleted)
                handling.HandleAllInitialize();
        }

        protected virtual void OnRemovedChildControl(AbstractContainer<TControl> sender, ControlEventArgs<TControl> e)
        {

        }

        public abstract IReadOnlyControlCollection<TControl> GetChildControls();

        IReadOnlyControlCollection<IControl> IContainerControl.GetChildControls()
        {
            return GetChildControls();
        }

        public override void HandleInitialize()
        {
            base.HandleInitialize();

            foreach (var control in GetChildControls())
            {
                control.HandleInitialize();
            }
        }

        public override void HandleInitCompleted1()
        {
            base.HandleInitCompleted1();

            foreach (var control in GetChildControls())
            {
                control.HandleInitCompleted1();
            }
        }

        public override void HandleInitCompleted2()
        {
            base.HandleInitCompleted2();

            foreach (var control in GetChildControls())
            {
                control.HandleInitCompleted2();
            }
        }

        public override void HandleInitCompleted3()
        {
            base.HandleInitCompleted3();

            foreach (var control in GetChildControls())
            {
                control.HandleInitCompleted3();
            }
        }

        public override void HandleCursorMove(CursorEventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
            {
                control.HandleCursorMove(new(control.ParentPos2ChildPos(e.Position)));
            }

            base.HandleCursorMove(e);
        }

        public override bool HandleRightClick(CursorEventArgs e)
        {
            TControl? control = GetChildControls().FirstHover;
            control?.HandleRightClick(new(control.ParentPos2ChildPos(e.Position)));

            return TryHandleRightClick(e);
        }

        public override bool HandleLeftClick(CursorEventArgs e)
        {
            TControl? control = GetChildControls().FirstHover;
            control?.HandleLeftClick(new(control.ParentPos2ChildPos(e.Position)));

            return TryHandleLeftClick(e);
        }

        public override bool HandleCursorSlotChanged(CursorSlotEventArgs e)
        {
            TControl? control = GetChildControls().FirstHover;
            control?.HandleCursorSlotChanged(new(control.ParentPos2ChildPos(e.Position), e.OldSlot, e.NewSlot));

            return TryHandleCursorSlotChanged(e);
        }

        public override bool HandleCursorItemChanged(CursorItemEventArgs e)
        {
            TControl? control = GetChildControls().FirstHover;
            control?.HandleCursorItemChanged(new(control.ParentPos2ChildPos(e.Position), e.Item));

            return TryHandleCursorItemChanged(e);
        }

        public override bool HandleTextEditorUpdate(CursorTextEventArgs e)
        {
            TControl? control = GetChildControls().FirstHover;
            control?.HandleTextEditorUpdate(new(control.ParentPos2ChildPos(e.Position), e.Text));

            return TryHandleTextEditorUpdate(e);
        }

        public override void HandleBeforeFrame(EventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
            {
                control.HandleBeforeFrame(e);
            }

            base.HandleBeforeFrame(e);
        }

        public override void HandleAfterFrame(EventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
            {
                control.HandleAfterFrame(e);
            }

            base.HandleAfterFrame(e);
        }
    }
}
