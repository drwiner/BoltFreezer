using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BoltFreezer.Interfaces
{


    public interface IOrderingGraph
    {
        List<Tuple<IOperator, IOperator>> Edges { get; set; }

        bool HasCycle();
        void Insert(Tuple<IOperator, IOperator> ordering);

        Object Clone();
    }

    public interface ICausalLinkGraph
    {
        List<CausalLink> Edges { get; set; }

        void Insert(CausalLink causalLink);

        Object Clone();
    }
}