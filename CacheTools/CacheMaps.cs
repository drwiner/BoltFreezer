using System;
using System.Collections.Generic;
using BoltFreezer.Interfaces;
using System.Linq;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public static class CacheMaps
    {
        /// <summary>
        /// Stored mappings for repair applicability. Do Not Use unless you are serializing
        /// </summary>
        public static Dictionary<Literal, List<int>> CausalMap = new Dictionary<Literal, List<int>>();

        /// <summary>
        /// Stored mappings for threat detection. Do Not Use unless you are serializing
        /// </summary>
        public static Dictionary<Literal, List<int>> ThreatMap = new Dictionary<Literal, List<int>>();

        public static void Reset()
        {
            CausalMap = new Dictionary<Literal, List<int>>();
            ThreatMap = new Dictionary<Literal, List<int>>();
        }

        public static IEnumerable<IOperator> GetCndts(IPredicate pred)
        {
            var toLiteral = new Literal(pred);
            if (CausalMap.ContainsKey(toLiteral))
                return from intID in CausalMap[toLiteral] select GroundActionFactory.GroundLibrary[intID];

            return new List<IOperator>();
        }

        public static IEnumerable<IOperator> GetThreats(IPredicate pred)
        {
            var toLiteral = new Literal(pred);
            if (ThreatMap.ContainsKey(toLiteral))
                return ThreatMap[toLiteral].Select(intID => GroundActionFactory.GroundLibrary[intID]);
            //        Where(intID => x.First.Equals(elm)).Select(x => x.Second);
            //return from intID in ThreatMap[pred] select GroundActionFactory.GroundLibrary[intID];
            return new List<IOperator>();
        }        

        public static bool IsCndt(IPredicate pred, IPlanStep ps)
        {
            var toLiteral = new Literal(pred);
            if (!CausalMap.ContainsKey(toLiteral))
                return false;

            return CausalMap[toLiteral].Contains(ps.Action.ID);
        }

        public static bool IsThreat(IPredicate pred, IPlanStep ps)
        {
            var toLiteral = new Literal(pred);
            if (!ThreatMap.ContainsKey(toLiteral))
                return false;

            return ThreatMap[toLiteral].Contains(ps.Action.ID);
        }

        // Checks for mappings pairwise
        public static void CacheLinks(List<IOperator> groundSteps)
        {
            foreach (var tstep in groundSteps)
            {
                foreach (var tprecond in tstep.Preconditions)
                {
                    var toLiteral = new Literal(tprecond);
                    if (CausalMap.ContainsKey(toLiteral) || ThreatMap.ContainsKey(toLiteral))
                    {
                        // Then this precondition has already been evaluated.
                        continue;
                    }

                    foreach (var hstep in groundSteps)
                    {
                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!CausalMap.ContainsKey(toLiteral))
                                CausalMap.Add(toLiteral, new List<int>() { hstep.ID });
                            else
                                if (!CausalMap[toLiteral].Contains(hstep.ID))
                            {
                                CausalMap[toLiteral].Add(hstep.ID);
                            }
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!ThreatMap.ContainsKey(toLiteral))
                                ThreatMap.Add(toLiteral, new List<int>() { hstep.ID });
                            else
                                ThreatMap[toLiteral].Add(hstep.ID);
                        }
                    }
                }

                // also need to load composite effects because these are dummy goal step open conditions.
                if (tstep.Height > 0)
                {
                    foreach (var teff in tstep.Effects)
                    {
                        var effLiteral = new Literal(teff);
                        if (CausalMap.ContainsKey(effLiteral) || ThreatMap.ContainsKey(effLiteral))
                        {
                            // Then this precondition has already been evaluated.
                            continue;
                        }

                        foreach (var hstep in groundSteps)
                        {
                            if (hstep.Effects.Contains(teff))
                            {
                                if (!CausalMap.ContainsKey(effLiteral))
                                    CausalMap.Add(effLiteral, new List<int>() { hstep.ID });
                                else
                                    if (!CausalMap[effLiteral].Contains(hstep.ID))
                                    {
                                        CausalMap[effLiteral].Add(hstep.ID);
                                    }
                                //CausalMap[teff].Add(hstep.ID);
                            }
                            if (hstep.Effects.Contains(teff.GetReversed()))
                            {
                                if (!ThreatMap.ContainsKey(effLiteral))
                                    ThreatMap.Add(effLiteral, new List<int>() { hstep.ID });
                                else
                                    ThreatMap[effLiteral].Add(hstep.ID);
                            }
                        }
                    }
                }
            }
        }

        // Limits checks to just those from heads to tails.
        public static void CacheLinks(List<IOperator> heads, List<IOperator> tails)
        {
            foreach (var tstep in tails)
            {
                foreach (var tprecond in tstep.Preconditions)
                {
                    var preLiteral = new Literal(tprecond);
                    foreach (var hstep in heads)
                    {
                        //if (CausalMap[tprecond].Contains(hstep.ID) || ThreatMap[tprecond].Contains(hstep.ID))
                        //{
                        //    // then this head step has already been checked for this condition
                        //    continue;
                        //}

                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!CausalMap.ContainsKey(preLiteral))
                                CausalMap.Add(preLiteral, new List<int>() { hstep.ID });
                            else
                            {
                                if (!CausalMap[preLiteral].Contains(hstep.ID)){
                                    CausalMap[preLiteral].Add(hstep.ID);
                                }

                            }
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!ThreatMap.ContainsKey(preLiteral))
                                ThreatMap.Add(preLiteral, new List<int>() { hstep.ID });
                            else
                                ThreatMap[preLiteral].Add(hstep.ID);
                        }
                    }
                }
                // also need to load composite effects because these are dummy goal step open conditions.
                if (tstep.Height > 0)
                {
                    foreach (var teff in tstep.Effects)
                    {
                        //if (CausalMap.ContainsKey(teff) || ThreatMap.ContainsKey(teff))
                        //{
                        //    // Then this precondition has already been evaluated.
                        //    continue;
                        //}
                        var effLiteral = new Literal(teff);
                        foreach (var hstep in heads)
                        {
                            if (hstep.Effects.Contains(teff))
                            {
                                if (!CausalMap.ContainsKey(effLiteral)) 
                                    CausalMap.Add(effLiteral, new List<int>() { hstep.ID });
                                else
                                    CausalMap[effLiteral].Add(hstep.ID);
                            }
                            if (hstep.Effects.Contains(teff.GetReversed()))
                            {
                                if (!ThreatMap.ContainsKey(effLiteral))
                                    ThreatMap.Add(effLiteral, new List<int>() { hstep.ID });
                                else
                                    ThreatMap[effLiteral].Add(hstep.ID);
                            }
                        }
                    }
                }
            }
        }

        public static void CacheGoalLinks(List<IOperator> groundSteps, List<IPredicate> goal)
        {

            foreach( var goalCondition in goal)
            {
                var goalLiteral = new Literal(goalCondition);
                if (CausalMap.ContainsKey(goalLiteral) || ThreatMap.ContainsKey(goalLiteral))
                {
                    continue;
                }

                foreach (var gstep in groundSteps)
                {
                    if (gstep.Height > 0)
                    {
                        //Console.WriteLine("debug");
                    }
                    if (gstep.Effects.Contains(goalCondition))
                    {
                        if (!CausalMap.ContainsKey(goalLiteral))
                            CausalMap.Add(goalLiteral, new List<int>() { gstep.ID });
                        else
                            CausalMap[goalLiteral].Add(gstep.ID);

                    }
                    if (gstep.Effects.Contains(goalCondition.GetReversed()))
                    {
                        if (!ThreatMap.ContainsKey(goalLiteral))
                            ThreatMap.Add(goalLiteral, new List<int>() { gstep.ID });
                        else
                            ThreatMap[goalLiteral].Add(gstep.ID);
                    }
                }
            }
        }

        private static Dictionary<Literal, int> RecursiveHeuristicCache(Dictionary<Literal, int> currentMap, List<IPredicate> InitialConditions)
        {
            // Steps that are executable given the initial conditions. These conditions can represent a state that is logically inconsistent (and (at bob store) (not (at bob store)))
            var initiallyRelevant = GroundActionFactory.GroundActions.Where(action => action.Height == 0 && action.Preconditions.All(pre => InitialConditions.Contains(pre)));

            // a boolean tag to decide whether to continue recursively. If checked, then there is some new effect that isn't in initial conditions.
            bool toContinue = false;

            // for each step whose preconditions are executable given the initial conditions
            foreach (var newStep in initiallyRelevant)
            {
                // sum_{pre in newstep.preconditions} currentMap[pre]
                var thisStepsValue = newStep.Preconditions.Sum(pre => currentMap[new Literal(pre)]);

                foreach(var eff in newStep.Effects)
                {
                    var effLiteral = new Literal(eff);
                    // ignore effects we've already seen; these occur "earlier" in planning graph
                    if (currentMap.ContainsKey(effLiteral))
                        continue;

                    // If we make it this far, then we've reached an unexplored literal effect
                    toContinue = true;

                    // The current value of this effect is 1 (this new step) + the sum of the preconditions of this step in the map.
                    currentMap[effLiteral] = 1 + thisStepsValue;

                    // Add this effect to the new initial Condition for subsequent round
                    InitialConditions.Add(eff);
                }
            }

            // Only continue recursively if we've explored a new literal effect. Pass the map along to maintain a global item.
            if (toContinue)
                return RecursiveHeuristicCache(currentMap, InitialConditions);

            // Otherwise, return our current map
            return currentMap;
            
        }

        public static void CacheAddReuseHeuristic(IState InitialState)
        {
            // Use dynamic programming
            var initialMap = new Dictionary<Literal, int>();
            foreach(var item in InitialState.Predicates)
            {
                initialMap[new Literal(item)]= 0;
            }
            List<IPredicate> newInitialList = InitialState.Predicates;
            foreach(var pre in CacheMaps.CausalMap.Keys)
            {
                if (pre.Sign)
                {
                    continue;
                }
                if (!newInitialList.Contains(pre.GetReversed()))
                {
                    newInitialList.Add(pre);
                    initialMap[new Literal(pre)] = 0;
                }
            }

            HeuristicMethods.visitedPreds = RecursiveHeuristicCache(initialMap, newInitialList);
         
        }
    }
}