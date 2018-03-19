
using BoltFreezer.Interfaces;

namespace BoltFreezer.PlanTools
{
    public class AddReuseHeuristic : IHeuristic
    {

        public int Heuristic(IPlan plan)
        {
            return HeuristicMethods.AddReuseHeuristic(plan);
        }

    }

    public class NumOpenConditionsHeuristic : IHeuristic
    {
        public int Heuristic(IPlan plan)
        {
            return HeuristicMethods.NumOCs(plan);
        }
    }

    public class ZeroHeuristic : IHeuristic
    {
        public int Heuristic(IPlan plan)
        {
            return 0;
        }
    }


    public static class HeuristicMethods
    {
        // h^r_add(pi) = sum_(oc in plan) 0 if exists a step possibly preceding oc.step and h_add(oc.precondition) otherwise.
        public static int AddReuseHeuristic(IPlan plan)
        {
            int sumo = 0;
            foreach (var oc in plan.Flaws.OpenConditionGenerator())
            {
                var existsA = false;
                foreach (var existingStep in plan.Steps)
                {
                    if (plan.Orderings.IsPath(oc.step, existingStep))
                        continue;
                    if (CacheMaps.CausalMap[oc.precondition].Contains(existingStep))
                    {
                        existsA = true;
                        break;
                    }
                }
                if (!existsA)
                    sumo += AddHeuristic(plan.Initial, oc.precondition);
            }
            return sumo;
        }

        // h_add(q) = 0 if holds initially, min a in GA, and infinite otherwise
        public static int AddHeuristic(IState initial, IPredicate condition)
        {
            if (initial.InState(condition))
                return 0;

            int minSoFar = 1000;
            foreach (var groundAction in CacheMaps.CausalMap[condition])
            {
                var thisVal = AddHeuristic(initial, groundAction);
                if (thisVal < minSoFar)
                {
                    minSoFar = thisVal;
                }
            }
            return minSoFar;
        }

        // h_add(a) = 1 + h_add (Prec(a))
        public static int AddHeuristic(IState initial, IOperator op)
        {
            int sumo = 1;
            foreach (var precond in op.Preconditions)
            {
                sumo += AddHeuristic(initial, precond);
            }
            return sumo;
        }

        // Number of open conditions heuristic
        public static int NumOCs(IPlan plan)
        {
            return plan.Flaws.Count;
        }
    }
}