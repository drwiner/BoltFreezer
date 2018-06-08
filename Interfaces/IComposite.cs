using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Interfaces
{
    public interface IComposite : IOperator
    {
        IPlanStep InitialStep { get; set; }
        IPlanStep GoalStep { get; set; }

        // Sub-plan <S, O, L> must be plan steps to distinguish (labels) and fulfill open conditions
        List<IPlanStep> SubSteps { get; set; }
        List<Tuple<IPlanStep, IPlanStep>> SubOrderings { get; set; }
        List<CausalLink<IPlanStep>> SubLinks { get; set; }
        new Object Clone();
    }
}
