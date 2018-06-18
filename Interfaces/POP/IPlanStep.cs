using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;

namespace BoltFreezer.Interfaces
{

    public interface IPlanStep : IOperator
    {
        // The representative operator   
        IOperator Action { get; set; }

        // Identification
        new int ID { get; set;  }

        IPlanStep InitCndt { get; set; }

        IPlanStep GoalCndt { get; set; }

        IPlanStep Parent { get; set; }

        int Depth { get; set; }

        // Actions keep track of open preconditions
        List<IPredicate> OpenConditions { get; set; }

        // remove from open conditions
        void Fulfill(IPredicate condition);

        // Actions can be cloned.
        new Object Clone();
    }

    
}
