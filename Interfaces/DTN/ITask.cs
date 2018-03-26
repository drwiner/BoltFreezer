using BoltFreezer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.DecompTools
{
    public interface ITask
    {
        int ID { get; set; }

        // Effects inherited from SubPlan
        List<IPredicate> LegalEffects { get; set; }

        // Underlying story world, inherits content from sub-tasks
        IPlan SubPlan { get; set; }

        // Possible decomposition into 
        List<ITask> SubTasks { get; set; }

        // Decomp --> which has subtasks and constraints.
        IDecomposition Decomposition { get; set; }

        // Constraints on sub-tasks, or constraints on self 
        List<IConstraint> TaskConstraints { get; set; }
    }
}
