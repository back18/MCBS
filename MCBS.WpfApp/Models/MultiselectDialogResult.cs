using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Models
{
    public class MultiselectDialogResult
    {
        public static readonly MultiselectDialogResult Empty = new(Array.Empty<string>());

        public MultiselectDialogResult(IReadOnlyList<string> selectedItems)
        {
            ArgumentNullException.ThrowIfNull(selectedItems, nameof(selectedItems));

            IsSuccess = selectedItems.Count > 0;
            SelectedItems = selectedItems;
        }

        public bool IsSuccess { get; }

        public IReadOnlyList<string> SelectedItems { get; }
    }
}
