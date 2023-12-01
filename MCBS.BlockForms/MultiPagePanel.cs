using MCBS.Events;
using MCBS.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class MultiPagePanel : ContainerControl<PagePanel>
    {
        public MultiPagePanel()
        {
            ActivePageKey = null;
            PagePanels = [];
        }

        public string? ActivePageKey { get; set; }

        public PagePanel? ActivePage
        {
            get
            {
                if (ActivePageKey is null)
                    return null;

                PagePanels.TryGetValue(ActivePageKey, out var pagePanel);
                return pagePanel;
            }
        }

        public Dictionary<string, PagePanel> PagePanels { get; }

        protected override void OnResize(Control sender, SizeChangedEventArgs e)
        {
            base.OnResize(sender, e);

            PagePanel? activePage = ActivePage;
            if (activePage is null)
                return;

            activePage.Size = e.NewSize;
        }

        protected override void OnBeforeFrame(Control sender, EventArgs e)
        {
            base.OnBeforeFrame(sender, e);

            PagePanel? activePage = ActivePage;
            if (activePage is null || ChildControls.Contains(activePage))
                return;

            ChildControls.Clear();
            ChildControls.Add(activePage);
        }
    }
}
