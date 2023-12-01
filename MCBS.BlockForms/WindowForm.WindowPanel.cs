using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
	public abstract partial class WindowForm
	{
		public class WindowPanel : MultiPagePanel
        {
            public WindowPanel(WindowForm owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                BorderWidth = 0;
            }

            private readonly WindowForm _owner;
        }
	}
}
