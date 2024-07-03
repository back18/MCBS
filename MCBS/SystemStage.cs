using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public enum SystemStage
    {
        ScreenScheduling,

        ProcessScheduling,

        FormScheduling,

        InteractionScheduling,

        RightClickObjectiveScheduling,

        ScreenBuildScheduling,

        HandleScreenInput,

        HandleScreenEvent,

        HandleBeforeFrame,

        HandleFrameDrawing,

        HandleFrameUpdate,

        HandleScreenOutput,

        HandleAfterFrame
    }
}
