using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.DecompTools
{
    public interface IConstraint
    {
        string Name { get; set; }

        bool Check(Term term);
        bool Check(IPredicate predicate);
        bool Check(IPlanStep step);
        bool Check(Tuple<IPlanStep, IPlanStep> ordering);
        bool Check(CausalLink<IPlanStep> causalLink);
        
    }
}
