using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public enum Stage
    {
        ScreenScheduling,

        ProcessScheduling,

        FormScheduling,

        InteractionScheduling,

        RightClickObjectiveScheduling,

        ScreenBuildScheduling,

        HandleScreenInput,

        HandleBeforeFrame,

        HandleUIRendering,

        HandleScreenOutput,

        HandleAfterFrame,

        HandleSystemInterrupt,

        Other,
    }
}
