using MCBS.WpfApp.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class FolderDialogService : IFolderDialogService
    {
        public SingleselectDialogResult ShowSingleselectDialog()
        {
            OpenFolderDialog dialog = new()
            {
                Multiselect = false
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new SingleselectDialogResult(dialog.FolderName) : SingleselectDialogResult.Empty;
        }

        public SingleselectDialogResult ShowSingleselectDialog(string title)
        {
            ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));

            OpenFolderDialog dialog = new()
            {
                Multiselect = false,
                Title = title
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new SingleselectDialogResult(dialog.FolderName) : SingleselectDialogResult.Empty;
        }

        public SingleselectDialogResult ShowSingleselectDialog(string title, string initialDirectory)
        {
            ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));
            ArgumentException.ThrowIfNullOrEmpty(initialDirectory, nameof(initialDirectory));

            OpenFolderDialog dialog = new()
            {
                Multiselect = false,
                Title = title,
                InitialDirectory = initialDirectory
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new SingleselectDialogResult(dialog.FolderName) : SingleselectDialogResult.Empty;
        }

        public MultiselectDialogResult ShowMultiselectDialog()
        {
            OpenFolderDialog dialog = new()
            {
                Multiselect = true
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new MultiselectDialogResult(dialog.FolderNames) : MultiselectDialogResult.Empty;
        }

        public MultiselectDialogResult ShowMultiselectDialog(string title)
        {
            ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));

            OpenFolderDialog dialog = new()
            {
                Multiselect = true,
                Title = title
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new MultiselectDialogResult(dialog.FolderNames) : MultiselectDialogResult.Empty;
        }

        public MultiselectDialogResult ShowMultiselectDialog(string title, string initialDirectory)
        {
            ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));
            ArgumentException.ThrowIfNullOrEmpty(initialDirectory, nameof(initialDirectory));

            OpenFolderDialog dialog = new()
            {
                Multiselect = true,
                Title = title,
                InitialDirectory = initialDirectory
            };

            bool result = dialog.ShowDialog() ?? false;
            return result ? new MultiselectDialogResult(dialog.FolderNames) : MultiselectDialogResult.Empty;
        }
    }
}
