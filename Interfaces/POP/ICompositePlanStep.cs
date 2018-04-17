using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Interfaces
{
    public interface ICompositePlanStep : IPlanStep
    {
        // Represented action
        IComposite CompositeAction { get; set; }

        // Dummy initial step
        IPlanStep InitialStep { get; set; }

        // Dummy goal step
        IPlanStep GoalStep { get; set; }

        // Sub-plan <S, O, L>
        List<IPlanStep> SubSteps { get; set; }

        List<Tuple<IPlanStep, IPlanStep>> SubOrderings { get; }

        List<CausalLink<IPlanStep>> SubLinks { get; }

        new Object Clone();
    }
}
