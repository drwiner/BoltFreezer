using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;

namespace BoltFreezer.DecompTools
{

    public class HasTermAt : IConstraint
    {
        private IPlanStep HasTerm;
        private ITerm TermToHave;
        private int PositionOfTerm;


        public HasTermAt(IPlanStep s, ITerm t, int pos)
        {
            HasTerm = s;
            TermToHave = t;
            PositionOfTerm = pos;
        }

        // The plan already has this step.
        public List<IPlan> Process(IPlan planToBuildOn)
        {
            // 
            Operator step = planToBuildOn.Find(HasTerm).Action as Operator;

            // Check if 
            if (step.TermAt(PositionOfTerm).Equals(TermToHave))
            {
                return new List<IPlan>() { planToBuildOn };
            }
            else
            {
                var t = step.TermAt(PositionOfTerm);
                step.AddBinding(t, TermToHave.Constant);
            }
            var newPlans = new List<IPlan>();
            //foreach (var precon in thisConstraint.Second.Preconditions)
            //{
            //    if (CacheMaps.IsCndt(precon, thisConstraint.First))
            //    {
            //        var planClone = planToBuildOn.Clone() as IPlan;
            //        planClone.CausalLinks.Add(new CausalLink<IPlanStep>(precon, thisConstraint.First, thisConstraint.Second));
            //        newPlans.Add(planClone);
            //    }
            //}
            return newPlans;
        }
    }

    public class Linked : IConstraint
    {
        public Tuple<IPlanStep, IPlanStep> thisConstraint;

        public Linked(IPlanStep s, IPlanStep t)
        {
            thisConstraint = new Tuple<IPlanStep, IPlanStep>(s, t);
        }

        public List<IPlan> Process(IPlan planToBuildOn)
        {
            var newPlans = new List<IPlan>();
            foreach (var precon in thisConstraint.Second.Preconditions)
            {
                if (CacheMaps.IsCndt(precon, thisConstraint.First))
                {
                    var planClone = planToBuildOn.Clone() as IPlan;
                    planClone.CausalLinks.Add(new CausalLink<IPlanStep>(precon, thisConstraint.First, thisConstraint.Second));
                    newPlans.Add(planClone);
                }
            }
            return newPlans;
        }
    }

    public class LinkedBy : IConstraint
    {
        public CausalLink<IPlanStep> thisConstraint;

        public LinkedBy(IPlanStep s, IPlanStep t, IPredicate p)
        {
            thisConstraint = new CausalLink<IPlanStep>(p, s, t);
        }

        public bool Check()
        {
            if (!CacheMaps.IsCndt(thisConstraint.Predicate, thisConstraint.Head))
                return false;
            return true;
        }

        public List<IPlan> Process(IPlan planToBuildOn)
        {
            var newPlan = planToBuildOn.Clone() as IPlan;
            newPlan.CausalLinks.Add(thisConstraint);
            return new List<IPlan>() { newPlan };
        }
    }
}
