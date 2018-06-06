
using BoltFreezer.Enums;
using BoltFreezer.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BoltFreezer.PlanTools
{


    public class AddReuseHeuristic : IHeuristic
    {
        public new string ToString()
        {
            return HType.ToString();
        }

        public HeuristicType HType
        {
            get { return HeuristicType.AddReuseHeuristic; }
        }

        public float Heuristic(IPlan plan)
        {
            return HeuristicMethods.AddReuseHeuristic(plan);
        }

    }

    public class NumOpenConditionsHeuristic : IHeuristic
    {
        public HeuristicType HType
        {
            get { return HeuristicType.NumOCsHeuristic; }
        }

        public new string ToString()
        {
            return HType.ToString();
        }

        public float Heuristic(IPlan plan)
        {
            return HeuristicMethods.NumOCs(plan);
        }
    }

    public class ZeroHeuristic : IHeuristic
    {
        public new string ToString()
        {
            return HType.ToString();
        }

        public HeuristicType HType
        {
            get { return HeuristicType.ZeroHeuristic; }
        }

        public float Heuristic(IPlan plan)
        {
            return 0f;
        }
    }


    public static class HeuristicMethods
    {
        // These may be stored in preprocessing step
        public static TupleMap<IPredicate, int> visitedPreds = new TupleMap<IPredicate, int>();

        // h^r_add(pi) = sum_(oc in plan) 0 if exists a step possibly preceding oc.step and h_add(oc.precondition) otherwise.
        public static int AddReuseHeuristic(IPlan plan)
        {
            // we are just taking the sum of the visitedPreds values of the open conditions, unless there is a step that establishes the condition already in plan (reuse).
            int sumo = 0;
            foreach (var oc in plan.Flaws.OpenConditions)
            {
                
                // Does there exist a step in the plan that can establish this needed precondition?
                var existsA = false;
                foreach (var existingStep in plan.Steps)
                {

                    if (existingStep.Height > 0)
                        continue;

                    if (plan.Orderings.IsPath(oc.step, existingStep))
                        continue;

                    if (CacheMaps.IsCndt(oc.precondition, existingStep))
                    {
                        existsA = true;
                        break;
                    }
                }

                // append heuristic for open condition
                if (!existsA)
                {
                    // we should always have the conditions in the visitedPreds dictionary if we processed correctly
                    if (visitedPreds.Get(oc.precondition.Sign).ContainsKey(oc.precondition))
                    {
                        sumo += visitedPreds.Get(oc.precondition.Sign)[oc.precondition];
                        continue;
                    }

                    //currentlyEvaluatedPreds.Add(oc.precondition);
                    //var amountToAdd = AddHeuristic(plan.Initial, oc.precondition, new HashSet<IPredicate>() { oc.precondition });
                    var amountToAdd = AddHeuristic(oc.precondition);
                    sumo += amountToAdd;
                    visitedPreds.Get(oc.precondition.Sign)[oc.precondition] = amountToAdd;
                }
            }
            return sumo;
        }

        public static int AddHeuristic(IPredicate precondition)
        {
            if (visitedPreds.Get(precondition.Sign)[precondition] == 0)
            {
                return 0;
            }

            var bestSoFar = 1000;
            foreach(var cndt in CacheMaps.GetCndts(precondition))
            {
                if (cndt.Height > 0)
                {
                    continue;
                }
                var sumo = cndt.Preconditions.Sum(pre => visitedPreds.Get(pre.Sign)[pre]);
                if (sumo < bestSoFar)
                {
                    bestSoFar = sumo;
                }
            }
            return bestSoFar;
        }

        // h_add(q) = 0 if holds initially, min a in GA, and infinite otherwise
        public static int AddHeuristic(IState initial, IPredicate condition, HashSet<IPredicate> ignorableConditions)
        {
            if (initial.InState(condition))
                return 0;

            // if we have a value for this, return it.
          //  if (visitedPreds.ContainsKey(condition))
          //  {
        //        return visitedPreds[condition];
       //     }

            int minSoFar = 1000;
            // Then this is a static condition that can never be true... we should avoid this plan.
            if (!CacheMaps.CausalTupleMap.Get(condition.Sign).ContainsKey(condition))
            {
                return minSoFar;
            }

            // find the gorund action that minimizes the heuristic estimate
            foreach (var groundAction in CacheMaps.GetCndts(condition))
            {
                if (groundAction.Height > 0)
                {
                    continue;
                }
                int thisVal;
              //  if (visitedOps.ContainsKey(groundAction))
           //     {
            // //       thisVal = visitedOps[groundAction];
            //    }
            //    else
            //    {
                thisVal = AddHeuristic(initial, groundAction, ignorableConditions);
            //    }


                if (thisVal < minSoFar)
                {
                    minSoFar = thisVal;
                }
            }

            //visitedPreds[condition] = minSoFar;
            return minSoFar;
        }

        // h_add(a) = 1 + h_add (Prec(a))
        public static int AddHeuristic(IState initial, IOperator op, HashSet<IPredicate> ignorableConditions)
        {
            //if (visitedOps.ContainsKey(op))
          //  {
         //       return visitedOps[op];
        //    }
            
            // Clone list and add new open conditions to ignorable conditions
            var newIgnorableConditions = new HashSet<IPredicate>();
            foreach( var item in ignorableConditions)
            {
                newIgnorableConditions.Add(item);
            }
            foreach(var precon in op.Preconditions)
            {
                newIgnorableConditions.Add(precon);
            }
           // newIgnorableConditions.AddRange(op.Preconditions);

            int sumo = 1;
            foreach (var precond in op.Preconditions)
            {
                // if its precondition is one that was already ignorable coming in...
                if (ignorableConditions.Contains(precond))
                {
                    continue;
                }
                
               // currentlyEvaluatedPreds.Add(precond);
                sumo += AddHeuristic(initial, precond, newIgnorableConditions);
            }
         //   visitedOps[op] = sumo;
            return sumo;
        }

        // Number of open conditions heuristic
        public static int NumOCs(IPlan plan)
        {
            return plan.Flaws.OpenConditions.Count;
        }
    }
}