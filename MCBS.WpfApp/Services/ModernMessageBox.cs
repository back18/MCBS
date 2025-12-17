using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace MCBS.WpfApp.Services
{
    public class ModernMessageBox : IMessageBoxService, IMessageBoxAsyncService
    {
        public MessageBoxResult Show(string messageBoxText)
        {
            return MessageBox.Show(messageBoxText);
        }

        public MessageBoxResult Show(string messageBoxText, string caption)
        {
            return MessageBox.Show(messageBoxText, caption);
        }

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            return MessageBox.Show(messageBoxText, caption, button);
        }

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return MessageBox.Show(messageBoxText, caption, button, icon);
        }

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return MessageBox.Show(messageBoxText, caption, button, icon, defaultResult);
        }

        public MessageBoxResult Show(Window owner, string messageBoxText)
        {
            return MessageBox.Show(owner, messageBoxText);
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption)
        {
            return MessageBox.Show(owner, messageBoxText, caption);
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button)
        {
            return MessageBox.Show(owner, messageBoxText, caption, button);
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return MessageBox.Show(owner, messageBoxText, caption, button, icon);
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return MessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult);
        }

        public Task<MessageBoxResult> ShowAsync(string messageBoxText)
        {
            return MessageBox.ShowAsync(messageBoxText);
        }

        public Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption)
        {
            return MessageBox.ShowAsync(messageBoxText, caption);
        }

        public Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button)
        {
            return MessageBox.ShowAsync(messageBoxText, caption, button);
        }

        public Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return MessageBox.ShowAsync(messageBoxText, caption, button, icon);
        }

        public Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return MessageBox.ShowAsync(messageBoxText, caption, button, icon, defaultResult);
        }

        public Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText)
        {
            return MessageBox.ShowAsync(owner, messageBoxText);
        }

        public Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption)
        {
            return MessageBox.ShowAsync(owner, messageBoxText, caption);
        }

        public Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button)
        {
            return MessageBox.ShowAsync(owner, messageBoxText, caption, button);
        }

        public Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return MessageBox.ShowAsync(owner, messageBoxText, caption, button, icon);
        }

        public Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return MessageBox.ShowAsync(owner, messageBoxText, caption, button, icon, defaultResult);
        }
    }
}
