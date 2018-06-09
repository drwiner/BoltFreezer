using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System.Collections;
using System.Collections.Generic;

using System;

namespace BoltFreezer.Scheduling {

    // An instantiation of CompositeSchedule
	[Serializable]
    public class CompositeSchedulePlanStep : CompositePlanStep, ICompositePlanStep
    {
        // has cntgs in addition to sub orderings
        public List<Tuple<IPlanStep, IPlanStep>> Cntgs;

        public CompositeSchedulePlanStep(CompositeSchedule comp) : base(comp as Composite)
        {
            Cntgs = comp.Cntgs;
        }

        public CompositeSchedulePlanStep(IComposite comp, List<Tuple<IPlanStep, IPlanStep>> cntgs) : base(comp)
        {
            Cntgs = cntgs;
        }

        public CompositeSchedulePlanStep(IPlanStep ps) : base(ps)
        {
            if (ps is CompositeSchedule cs)
            {
                Cntgs = cs.Cntgs;
            }
            else
            {
                Cntgs = new List<Tuple<IPlanStep, IPlanStep>>();
            }
        }

        public CompositeSchedulePlanStep(Operator op) : base(new Composite(op))
        {

            Cntgs = new List<Tuple<IPlanStep, IPlanStep>>();
        }

        public CompositeSchedulePlanStep(CompositePlanStep cps) : base(cps.CompositeAction, cps.OpenConditions, cps.InitialStep, cps.GoalStep, cps.SubSteps, cps.SubOrderings, cps.SubLinks, cps.ID)
        {
            Cntgs = new List<Tuple<IPlanStep, IPlanStep>>();
        }

        public CompositeSchedulePlanStep(CompositePlanStep cps, List<Tuple<IPlanStep, IPlanStep>> cntgs) : base(cps.CompositeAction, cps.OpenConditions, cps.InitialStep, cps.GoalStep, cps.SubSteps, cps.SubOrderings, cps.SubLinks, cps.ID)
        {
            Cntgs = cntgs;
        }


        public new System.Object Clone()
        {
            var cps = base.Clone() as CompositePlanStep;
            var newCntgs = new List<Tuple<IPlanStep, IPlanStep>>();
            foreach (var cntg in Cntgs)
            {
                // due dilligence
                newCntgs.Add(new Tuple<IPlanStep, IPlanStep>(cntg.First.Clone() as IPlanStep, cntg.Second.Clone() as IPlanStep));
            }
            return new CompositeSchedulePlanStep(cps, newCntgs);
        }

    }
}