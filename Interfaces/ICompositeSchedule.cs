using BoltFreezer.Camera;
using BoltFreezer.DecompTools;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Interfaces
{
    public interface ICompositeSchedule : IComposite
    {
        int NumberSegments { get; }
        int NumberCamSteps { get; }

        //List<ContextPredicate> ContextPrecons { get; set; }
        //List<ContextPredicate> ContextEffects { get; set; }

        CamPlanStep InitialCamAction { get; set; }
        CamPlanStep FinalCamAction { get; set; }

        IPlanStep InitialAction { get; set; }
        IPlanStep FinalAction { get; set; }

        ActionSeg InitialActionSeg { get; set; }
        ActionSeg FinalActionSeg { get; set; }

        // Sub-plan <S, O, L> must be plan steps to distinguish (labels) and fulfill open conditions
        new Object Clone();
    }
}
