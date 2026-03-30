using MCBS.WpfApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IFolderDialogService
    {
        public SingleselectDialogResult ShowSingleselectDialog();

        public SingleselectDialogResult ShowSingleselectDialog(string title);

        public SingleselectDialogResult ShowSingleselectDialog(string title, string initialDirectory);

        public MultiselectDialogResult ShowMultiselectDialog();

        public MultiselectDialogResult ShowMultiselectDialog(string title);

        public MultiselectDialogResult ShowMultiselectDialog(string title, string initialDirectory);
    }
}
