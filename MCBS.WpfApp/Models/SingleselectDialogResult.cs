using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MCBS.WpfApp.Models
{
    public class SingleselectDialogResult
    {
        public static readonly SingleselectDialogResult Empty = new(null);

        public SingleselectDialogResult(string? selectedItem)
        {
            IsSuccess = selectedItem is not null;
            SelectedItem = selectedItem;
        }

        [MemberNotNullWhen(true, nameof(SelectedItem))]
        public bool IsSuccess { get; }

        public string? SelectedItem { get; }
    }
}
