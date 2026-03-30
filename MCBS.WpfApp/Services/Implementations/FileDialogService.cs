using MCBS.WpfApp.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class FileDialogService : IFileDialogService
    {
        public SingleselectDialogResult ShowSingleselectDialog()
        {
            OpenFileDialog dialog = new();

            bool result = dialog.ShowDialog() ?? false;
            return result ? new SingleselectDialogResult(dialog.FileName) : SingleselectDialogResult.Empty;
        }

        public SingleselectDialogResult ShowSingleselectDialog(string title)
        {
            ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));

            OpenFileDialog dialog = new()
            {
                Title = title
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new SingleselectDialogResult(dialog.FileName) : SingleselectDialogResult.Empty;
        }

        public SingleselectDialogResult ShowSingleselectDialog(string title, string initialDirectory)
        {
            ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));
            ArgumentException.ThrowIfNullOrEmpty(initialDirectory, nameof(initialDirectory));

            OpenFileDialog dialog = new()
            {
                Title = title,
                InitialDirectory = initialDirectory
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new SingleselectDialogResult(dialog.FileName) : SingleselectDialogResult.Empty;
        }

        public SingleselectDialogResult ShowSingleselectDialog(string title, string initialDirectory, string filter, int filterIndex = 1)
        {
            ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));
            ArgumentException.ThrowIfNullOrEmpty(initialDirectory, nameof(initialDirectory));
            ArgumentException.ThrowIfNullOrEmpty(filter, nameof(filter));

            OpenFileDialog dialog = new()
            {
                Title = title,
                InitialDirectory = initialDirectory,
                Filter = filter,
                FilterIndex = filterIndex
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new SingleselectDialogResult(dialog.FileName) : SingleselectDialogResult.Empty;
        }

        public MultiselectDialogResult ShowMultiselectDialog()
        {
            OpenFileDialog dialog = new();

            bool result = dialog.ShowDialog() ?? false;
            return result ? new MultiselectDialogResult(dialog.FileNames) : MultiselectDialogResult.Empty;
        }

        public MultiselectDialogResult ShowMultiselectDialog(string title)
        {
            ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));

            OpenFileDialog dialog = new()
            {
                Title = title
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new MultiselectDialogResult(dialog.FileNames) : MultiselectDialogResult.Empty;
        }

        public MultiselectDialogResult ShowMultiselectDialog(string title, string initialDirectory)
        {
            ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));
            ArgumentException.ThrowIfNullOrEmpty(initialDirectory, nameof(initialDirectory));

            OpenFileDialog dialog = new()
            {
                Title = title,
                InitialDirectory = initialDirectory
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new MultiselectDialogResult(dialog.FileNames) : MultiselectDialogResult.Empty;
        }

        public MultiselectDialogResult ShowMultiselectDialog(string title, string initialDirectory, string filter, int filterIndex = 1)
        {
            ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));
            ArgumentException.ThrowIfNullOrEmpty(initialDirectory, nameof(initialDirectory));
            ArgumentException.ThrowIfNullOrEmpty(filter, nameof(filter));

            OpenFileDialog dialog = new()
            {
                Title = title,
                InitialDirectory = initialDirectory,
                Filter = filter,
                FilterIndex = filterIndex
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new MultiselectDialogResult(dialog.FileNames) : MultiselectDialogResult.Empty;
        }
    }
}
