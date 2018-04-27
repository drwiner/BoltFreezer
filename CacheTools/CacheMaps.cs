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
        public static Dictionary<IPredicate, List<int>> CausalMap = new Dictionary<IPredicate, List<int>>();

        /// <summary>
        /// Stored mappings for threat detection. Do Not Use unless you are serializing
        /// </summary>
        public static Dictionary<IPredicate, List<int>> ThreatMap = new Dictionary<IPredicate, List<int>>();

        public static void Reset()
        {
            CausalMap = new Dictionary<IPredicate, List<int>>();
            ThreatMap = new Dictionary<IPredicate, List<int>>();
        }

        public static IEnumerable<IOperator> GetCndts(IPredicate pred)
        {
            if (CausalMap.ContainsKey(pred))
                return from intID in CausalMap[pred] select GroundActionFactory.GroundLibrary[intID];

            return new List<IOperator>();
        }

        public static IEnumerable<IOperator> GetThreats(IPredicate pred)
        {
            if (ThreatMap.ContainsKey(pred))
                return ThreatMap[pred].Select(intID => GroundActionFactory.GroundLibrary[intID]);
            //        Where(intID => x.First.Equals(elm)).Select(x => x.Second);
            //return from intID in ThreatMap[pred] select GroundActionFactory.GroundLibrary[intID];
            return new List<IOperator>();
        }        

        public static bool IsCndt(IPredicate pred, IPlanStep ps)
        {
            if (!CausalMap.ContainsKey(pred))
                return false;

            return CausalMap[pred].Contains(ps.Action.ID);
        }

        public static bool IsThreat(IPredicate pred, IPlanStep ps)
        {
            if (!ThreatMap.ContainsKey(pred))
                return false;

            return ThreatMap[pred].Contains(ps.Action.ID);
        }

        // Checks for mappings pairwise
        public static void CacheLinks(List<IOperator> groundSteps)
        {
            foreach (var tstep in groundSteps)
            {
                foreach (var tprecond in tstep.Preconditions)
                {
                    if (CausalMap.ContainsKey(tprecond) || ThreatMap.ContainsKey(tprecond))
                    {
                        // Then this precondition has already been evaluated.
                        continue;
                    }

                    foreach (var hstep in groundSteps)
                    {
                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!CausalMap.ContainsKey(tprecond))
                                CausalMap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                CausalMap[tprecond].Add(hstep.ID);
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!ThreatMap.ContainsKey(tprecond))
                                ThreatMap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                ThreatMap[tprecond].Add(hstep.ID);
                        }
                    }
                }

                // also need to load composite effects because these are dummy goal step open conditions.
                if (tstep.Height > 0)
                {
                    foreach (var teff in tstep.Effects)
                    {
                        if (CausalMap.ContainsKey(teff) || ThreatMap.ContainsKey(teff))
                        {
                            // Then this precondition has already been evaluated.
                            continue;
                        }

                        foreach (var hstep in groundSteps)
                        {
                            if (hstep.Effects.Contains(teff))
                            {
                                if (!CausalMap.ContainsKey(teff))
                                    CausalMap.Add(teff, new List<int>() { hstep.ID });
                                else
                                    CausalMap[teff].Add(hstep.ID);
                            }
                            if (hstep.Effects.Contains(teff.GetReversed()))
                            {
                                if (!ThreatMap.ContainsKey(teff))
                                    ThreatMap.Add(teff, new List<int>() { hstep.ID });
                                else
                                    ThreatMap[teff].Add(hstep.ID);
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

                    foreach (var hstep in heads)
                    {
                        if (CausalMap[tprecond].Contains(hstep.ID) || ThreatMap[tprecond].Contains(hstep.ID))
                        {
                            // then this head step has already been checked for this condition
                            continue;
                        }

                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!CausalMap.ContainsKey(tprecond))
                                CausalMap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                CausalMap[tprecond].Add(hstep.ID);
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!ThreatMap.ContainsKey(tprecond))
                                ThreatMap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                ThreatMap[tprecond].Add(hstep.ID);
                        }
                    }
                }
                // also need to load composite effects because these are dummy goal step open conditions.
                if (tstep.Height > 0)
                {
                    foreach (var teff in tstep.Effects)
                    {
                        if (CausalMap.ContainsKey(teff) || ThreatMap.ContainsKey(teff))
                        {
                            // Then this precondition has already been evaluated.
                            continue;
                        }

                        foreach (var hstep in heads)
                        {
                            if (hstep.Effects.Contains(teff))
                            {
                                if (!CausalMap.ContainsKey(teff))
                                    CausalMap.Add(teff, new List<int>() { hstep.ID });
                                else
                                    CausalMap[teff].Add(hstep.ID);
                            }
                            if (hstep.Effects.Contains(teff.GetReversed()))
                            {
                                if (!ThreatMap.ContainsKey(teff))
                                    ThreatMap.Add(teff, new List<int>() { hstep.ID });
                                else
                                    ThreatMap[teff].Add(hstep.ID);
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
                foreach(var gstep in groundSteps)
                {
                    if (gstep.Height > 0)
                    {
                        //Console.WriteLine("debug");
                    }
                    if (gstep.Effects.Contains(goalCondition))
                    {
                        if (!CausalMap.ContainsKey(goalCondition))
                            CausalMap.Add(goalCondition, new List<int>() { gstep.ID });
                        else
                            CausalMap[goalCondition].Add(gstep.ID);

                    }
                    if (gstep.Effects.Contains(goalCondition.GetReversed()))
                    {
                        if (!ThreatMap.ContainsKey(goalCondition))
                            ThreatMap.Add(goalCondition, new List<int>() { gstep.ID });
                        else
                            ThreatMap[goalCondition].Add(gstep.ID);
                    }
                }
            }
        }

        private static Dictionary<IPredicate, int> RecursiveHeuristicCache(Dictionary<IPredicate, int> currentMap, List<IPredicate> InitialConditions)
        {
            // Steps that are executable given the initial conditions. These conditions can represent a state that is logically inconsistent (and (at bob store) (not (at bob store)))
            var initiallyRelevant = GroundActionFactory.GroundActions.Where(action => action.Height == 0 && action.Preconditions.All(pre => InitialConditions.Contains(pre)));

            // a boolean tag to decide whether to continue recursively. If checked, then there is some new effect that isn't in initial conditions.
            bool toContinue = false;

            // for each step whose preconditions are executable given the initial conditions
            foreach (var newStep in initiallyRelevant)
            {
                // sum_{pre in newstep.preconditions} currentMap[pre]
                var thisStepsValue = newStep.Preconditions.Sum(pre => currentMap[pre]);

                foreach(var eff in newStep.Effects)
                {
                    // ignore effects we've already seen; these occur "earlier" in planning graph
                    if (currentMap.ContainsKey(eff))
                        continue;

                    // If we make it this far, then we've reached an unexplored literal effect
                    toContinue = true;

                    // The current value of this effect is 1 (this new step) + the sum of the preconditions of this step in the map.
                    currentMap[eff] = 1 + thisStepsValue;

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
            var initialMap = new Dictionary<IPredicate, int>();
            foreach(var item in InitialState.Predicates)
            {
                initialMap[item]= 0;
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
                    initialMap[pre] = 0;
                }
            }

            HeuristicMethods.visitedPreds = RecursiveHeuristicCache(initialMap, newInitialList);
         
        }
    }
}