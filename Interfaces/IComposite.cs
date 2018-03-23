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
        IOperator InitialStep { get; set; }
        IOperator GoalStep { get; set; }
        List<IOperator> SubSteps { get; set; }
        List<Tuple<IOperator,IOperator>> SubOrderings { get; set; }
        List<CausalLink<IOperator>> SubLinks { get; set; }
        new Object Clone();
    }
}
