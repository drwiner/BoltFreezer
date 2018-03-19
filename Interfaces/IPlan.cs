using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using BoltFreezer.PlanTools;

namespace BoltFreezer.Interfaces
{
    public interface IPlan
    {
        // Plans have a domain.
        Domain Domain { get; set; }

        // Plans have a problem.
        Problem Problem { get; set; }

        // Plans have an ordered list of steps.
        List<IOperator> Steps { get; set; }

        // Ordering Graph
        Graph<IOperator> Orderings { get; set; }
        //IOrderingGraph Orderings { get; set; }

        // CausalLinkGraph
        ICausalLinkGraph CausalLinks { get; set; }

        // The plan will have an initial state.
        IState Initial { get; set; }

        // The plan will have a goal state.
        IState Goal { get; set; }

        Flawque Flaws { get; set; }

        // The plan can be cloned.
        Object Clone();

        float Estimate { get; set; }
    }
}
