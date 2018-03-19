using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BoltFreezer.PlanTools
{
    public class Flawque
    {
        private Heap<OpenCondition> openConditions;
        private Heap<ThreatenedLinkFlaw> threatenedLinks;


        public Flawque()
        {
            openConditions = new Heap<OpenCondition>(HeapType.MinHeap);
            threatenedLinks = new Heap<ThreatenedLinkFlaw>(HeapType.MinHeap);
        }

        public void Insert(Plan plan, OpenCondition oc)
        {
            // Static? 
            if (oc.precondition.IsStatic)
            {
                oc.isStatic = true;
                return;
            }

            // accumulate risks and cndts
            foreach (var step in plan.Steps)
            {
                if (step.ID == oc.step.ID)
                    continue;
                if (plan.Orderings.IsPath(oc.step, step))
                    continue;
                // CHECK THAT THIS WORKS AS INTENDED: or else I will have created a causal and threat map
                if (step.Effects.Contains(oc.precondition))
                    oc.cndts += 1;

                if (step.Effects.Any(x => oc.precondition.IsInverse(x)))
                    oc.risks += 1;

            }

            if (oc.risks == 0 && plan.Initial.InState(oc.precondition))
            {
                oc.isInit = true;
            }

            openConditions.Insert(oc);
        }

        public void Insert(ThreatenedLinkFlaw tclf)
        {

            threatenedLinks.Insert(tclf);
        }

        public IEnumerable<OpenCondition> OpenConditionGenerator()
        {
            foreach (var item in openConditions.ToList())
            {
                yield return item.Clone();
            }
        }

        public IFlaw Next()
        {
            // repair threatened links first
            if (!threatenedLinks.IsEmpty())
                return threatenedLinks.PopRoot();

            if (!openConditions.IsEmpty())
                return openConditions.PopRoot();

            return null;
        }

        // What to do here --> how can we reassign?
        public void AddCndtsAndRisks(Plan plan, IOperator action)
        {
            foreach (var oc in OpenConditionGenerator())
            {
                // ignore any open conditions that cannot possibly be affected by this action's effects, such as those occurring after
                if (plan.Orderings.IsPath(oc.step, action))
                    continue;

                if (action.Effects.Contains(oc.precondition))
                    oc.cndts += 1;

                if (action.Effects.Any(x => oc.precondition.IsInverse(x)))
                    oc.risks += 1;
            }
        }

    }
}