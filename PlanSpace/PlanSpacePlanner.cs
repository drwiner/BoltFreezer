using BoltFreezer.Interfaces;
using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoltFreezer.PlanSpace
{
    public class PlanSpacePlanner
    {
        private SimplePriorityQueue<IPlan, float> frontier;

        // TODO: keep track of plan-space search and not just frontier

        //private List<PlanSpaceEdge> PlanSpaceGraph;

        public PlanSpacePlanner(List<IOperator> groundSteps, IPlan initialPlan)
        {
            frontier = new SimplePriorityQueue<IPlan, float>();
            Insert(initialPlan);
        }


        public void Insert(IPlan plan)
        {
            if (!plan.Orderings.HasCycle())
            {
                frontier.Enqueue(plan, plan.Estimate);
            }
        }

        public List<IPlan> Solve(int k=4, float cutoff=6000f)
        {
            var Solutions = new List<IPlan>();
            int leaves = 0;
            int expanded = 0;

            var t0 = Time.time;

            while (frontier.Count > 0)
            {
                var plan = frontier.Dequeue();
                //plan.flaws
            }

            return Solutions;
        }

    }
}
