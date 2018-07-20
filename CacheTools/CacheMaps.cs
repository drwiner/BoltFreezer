using System;
using System.Collections.Generic;
using BoltFreezer.Interfaces;
using System.Linq;
using BoltFreezer.Utilities;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public static class CacheMaps
    {
        /// <summary>
        /// Stored mappings for repair applicability. Do Not Use unless you are serializing
        /// </summary>
        /// 
        public static TupleMap<IPredicate, List<int>> CausalTupleMap = new TupleMap<IPredicate, List<int>>();
        public static TupleMap<IPredicate, List<int>> ThreatTupleMap = new TupleMap<IPredicate, List<int>>();

        public static List<IPredicate> PrimaryEffects = new List<IPredicate>();

        //public static Dictionary<IPredicate, List<int>> PosCausalMap = new Dictionary<IPredicate, List<int>>();
        //public static Dictionary<IPredicate, List<int>> NegCausalMap = new Dictionary<IPredicate, List<int>>();

        ///// <summary>
        ///// Stored mappings for threat detection. Do Not Use unless you are serializing
        ///// </summary>
        //public static Dictionary<IPredicate, List<int>> PosThreatMap = new Dictionary<IPredicate, List<int>>();
        //public static Dictionary<IPredicate, List<int>> NegThreatMap = new Dictionary<IPredicate, List<int>>();

        public static void Reset()
        {
            CausalTupleMap = new TupleMap<IPredicate, List<int>>();
            ThreatTupleMap = new TupleMap<IPredicate, List<int>>();
            //PosCausalMap = new Dictionary<IPredicate, List<int>>();
            //NegCausalMap = new Dictionary<IPredicate, List<int>>();
            //PosThreatMap = new Dictionary<IPredicate, List<int>>();
            //NegThreatMap = new Dictionary<IPredicate, List<int>>();
        }

        public static IEnumerable<IOperator> GetCndts(IPredicate pred)
        {
            var whichMap = CausalTupleMap.Get(pred.Sign);
            if (whichMap.ContainsKey(pred))
                return from intID in whichMap[pred] select GroundActionFactory.GroundLibrary[intID];
            
            return new List<IOperator>();
        }

        public static IEnumerable<IOperator> GetThreats(IPredicate pred)
        {
            var whichMap = ThreatTupleMap.Get(pred.Sign);
            if (whichMap.ContainsKey(pred))
                return whichMap[pred].Select(intID => GroundActionFactory.GroundLibrary[intID]);
            
            //        Where(intID => x.First.Equals(elm)).Select(x => x.Second);
            //return from intID in ThreatMap[pred] select GroundActionFactory.GroundLibrary[intID];
            return new List<IOperator>();
        }        

        public static bool IsCndt(IPredicate pred, IPlanStep ps)
        {
            var whichMap = CausalTupleMap.Get(pred.Sign);

            if (!whichMap.ContainsKey(pred))
                return false;
            return whichMap[pred].Contains(ps.Action.ID);
           
        }

        public static bool IsThreat(IPredicate pred, IPlanStep ps)
        {
            var whichMap = ThreatTupleMap.Get(pred.Sign);
            if (!whichMap.ContainsKey(pred))
                return false;
            return whichMap[pred].Contains(ps.Action.ID);
        }

        // Checks for mappings pairwise
        public static void CacheLinks(List<IOperator> groundSteps)
        {
            foreach (var tstep in groundSteps)
            {
                foreach (var tprecond in tstep.Preconditions)
                {
                    var causemap = CausalTupleMap.Get(tprecond.Sign);
                    var threatmap = ThreatTupleMap.Get(tprecond.Sign);

                    if (causemap.ContainsKey(tprecond) || threatmap.ContainsKey(tprecond))
                    {
                        // Then this precondition has already been evaluated.
                        continue;
                    }
                        
                    foreach (var hstep in groundSteps)
                    {
                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!causemap.ContainsKey(tprecond))
                                causemap.Add(tprecond, new List<int>() { hstep.ID });
                            else if (!causemap[tprecond].Contains(hstep.ID))
                            {
                                causemap[tprecond].Add(hstep.ID);
                            }
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!threatmap.ContainsKey(tprecond))
                                threatmap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                threatmap[tprecond].Add(hstep.ID);
                        }
                    }

                }

                // also need to load composite effects because these are dummy goal step open conditions.
                if (tstep.Height > 0)
                {
                    foreach (var teff in tstep.Effects)
                    {
                        var causeMap = CausalTupleMap.Get(teff.Sign);
                        var threatmap = ThreatTupleMap.Get(teff.Sign);
                        if (causeMap.ContainsKey(teff) || threatmap.ContainsKey(teff))
                        {
                            // Then this precondition has already been evaluated.
                            continue;
                        }

                        foreach (var hstep in groundSteps)
                        {
                            if (hstep.Effects.Contains(teff))
                            {
                                if (!causeMap.ContainsKey(teff))
                                    causeMap.Add(teff, new List<int>() { hstep.ID });
                                else
                                    if (!causeMap[teff].Contains(hstep.ID))
                                    {
                                    causeMap[teff].Add(hstep.ID);
                                    }
                                //CausalMap[teff].Add(hstep.ID);
                            }
                            if (hstep.Effects.Contains(teff.GetReversed()))
                            {
                                if (!threatmap.ContainsKey(teff))
                                    threatmap.Add(teff, new List<int>() { hstep.ID });
                                else
                                    threatmap[teff].Add(hstep.ID);
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
                    var causeMap = CausalTupleMap.Get(tprecond.Sign);
                    var threatmap = ThreatTupleMap.Get(tprecond.Sign);

                    foreach (var hstep in heads)
                    {

                        if ((causeMap.ContainsKey(tprecond) && causeMap[tprecond].Contains(hstep.ID)) || (threatmap.ContainsKey(tprecond) && threatmap[tprecond].Contains(hstep.ID)))
                        {
                            // then this head step has already been checked for this condition
                            continue;
                        }

                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!causeMap.ContainsKey(tprecond))
                                causeMap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                            {
                                if (!causeMap[tprecond].Contains(hstep.ID)){
                                    causeMap[tprecond].Add(hstep.ID);
                                }

                            }
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!threatmap.ContainsKey(tprecond))
                                threatmap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                threatmap[tprecond].Add(hstep.ID);
                        }
                    }
                }
                // also need to load composite effects because these are dummy goal step open conditions.
                if (tstep.Height > 0)
                {
                    foreach (var teff in tstep.Effects)
                    {
                        var causeMap = CausalTupleMap.Get(teff.Sign);
                        var threatmap = ThreatTupleMap.Get(teff.Sign);

                        if (causeMap.ContainsKey(teff) || threatmap.ContainsKey(teff))
                        {
                            // Then this precondition has already been evaluated.
                            continue;
                        }

                        foreach (var hstep in heads)
                        {
                            if (hstep.Effects.Contains(teff))
                            {
                                if (!causeMap.ContainsKey(teff))
                                    causeMap.Add(teff, new List<int>() { hstep.ID });
                                else
                                    causeMap[teff].Add(hstep.ID);
                            }
                            if (hstep.Effects.Contains(teff.GetReversed()))
                            {
                                if (!threatmap.ContainsKey(teff))
                                    threatmap.Add(teff, new List<int>() { hstep.ID });
                                else
                                    threatmap[teff].Add(hstep.ID);
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
                var causeMap = CausalTupleMap.Get(goalCondition.Sign);
                var threatmap = ThreatTupleMap.Get(goalCondition.Sign);

                // Hence, it's been processed already
                if (causeMap.ContainsKey(goalCondition) || threatmap.ContainsKey(goalCondition))
                {
                    continue;
                }

                foreach (var gstep in groundSteps)
                {

                    if (gstep.Effects.Contains(goalCondition))
                    {
                        if (!causeMap.ContainsKey(goalCondition))
                            causeMap.Add(goalCondition, new List<int>() { gstep.ID });
                        else
                            causeMap[goalCondition].Add(gstep.ID);
                    }

                    if (gstep.Effects.Contains(goalCondition.GetReversed()))
                    {
                        if (!threatmap.ContainsKey(goalCondition))
                            threatmap.Add(goalCondition, new List<int>() { gstep.ID });
                        else
                            threatmap[goalCondition].Add(gstep.ID);
                    }

                }
            }
        }

        private static TupleMap<IPredicate, int> RecursiveHeuristicCache(TupleMap<IPredicate, int> currentMap, List<IPredicate> InitialConditions)
        {
            // Steps that are executable given the initial conditions. These conditions can represent a state that is logically inconsistent (and (at bob store) (not (at bob store)))
            var initiallyRelevant = GroundActionFactory.GroundActions.Where(action => action.Height == 0 && action.Preconditions.All(pre => InitialConditions.Contains(pre)));

            // a boolean tag to decide whether to continue recursively. If checked, then there is some new effect that isn't in initial conditions.
            bool toContinue = false;

            // for each step whose preconditions are executable given the initial conditions
            foreach (var newStep in initiallyRelevant)
            {
                // sum_{pre in newstep.preconditions} currentMap[pre]
                var thisStepsValue = newStep.Preconditions.Sum(pre => currentMap.Get(pre.Sign)[pre]);

                foreach(var eff in newStep.Effects)
                {
                    // ignore effects we've already seen; these occur "earlier" in planning graph
                    if (currentMap.Get(eff.Sign).ContainsKey(eff))
                        continue;

                    // If we make it this far, then we've reached an unexplored literal effect
                    toContinue = true;

                    // The current value of this effect is 1 (this new step) + the sum of the preconditions of this step in the map.
                    currentMap.Get(eff.Sign)[eff] = 1 + thisStepsValue;

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
            var initialMap = new TupleMap<IPredicate, int>();
            //var initialMap = new Dictionary<IPredicate, int>();
            foreach(var item in InitialState.Predicates)
            {
                initialMap.Get(true)[item] = 0;
            }
            List<IPredicate> newInitialList = InitialState.Predicates;
            foreach(var pre in CacheMaps.CausalTupleMap.Get(false).Keys)
            {
                if (!newInitialList.Contains(pre.GetReversed()))
                {
                    newInitialList.Add(pre);
                    initialMap.Get(false)[pre] = 0;
                }
            }

            HeuristicMethods.visitedPreds = RecursiveHeuristicCache(initialMap, newInitialList);
         
        }

        public static bool IsPrimaryEffect(IPredicate condition)
        {
            return PrimaryEffects.Contains(condition);
        }

        /// <summary>
        /// Given a primary effect (one that is not the effect of a primitive step), calculate heuristic value.
        /// Let that heuristic value be the shortest (height) step that can contribute, plus all of its preconditions.
        /// Recursively, if any of its preconditions are primary effects, then repeat until we have either a step that is true in the initial state or has no primary effects as preconditions.
        /// </summary>
        /// <param name="InitialState"></param>
        /// <param name="primaryEffect"></param>
        /// <returns></returns>
        public static void PrimaryEffectHack(IState InitialState)
        {
            // Finds primary effects: for each composite step, if its precondition has no visitedPreds value.
            CalculatePrimaryEffects();

            var initialMap = new TupleMap<IPredicate, int>();
            var primaryEffectsInInitialState = new List<IPredicate>();
            foreach (var item in InitialState.Predicates)
            {
                if (IsPrimaryEffect(item))
                {
                    primaryEffectsInInitialState.Add(item);
                    initialMap.Get(item.Sign)[item] = 0;
                }
            }

            var heurDict = PrimaryEffectRecursiveHeuristicCache(initialMap, primaryEffectsInInitialState);

            foreach (var keyvalue in heurDict.Get(true))
            {
                HeuristicMethods.visitedPreds.Get(true)[keyvalue.Key] = keyvalue.Value;
            }
            foreach (var keyvalue in heurDict.Get(false))
            {
                HeuristicMethods.visitedPreds.Get(false)[keyvalue.Key] = keyvalue.Value;
            }
        }

        private static void CalculatePrimaryEffects()
        {
            PrimaryEffects = new List<IPredicate>();
            var CompositeOps = GroundActionFactory.GroundActions.Where(act => act.Height > 0);

            foreach (var compOp in CompositeOps)
            {
                foreach (var precon in compOp.Preconditions)
                {
                    if (!HeuristicMethods.visitedPreds.Get(precon.Sign).ContainsKey(precon))
                    {
                        PrimaryEffects.Add(precon);
                        HeuristicMethods.visitedPreds.Get(precon.Sign)[precon] = 200;
                    }
                }
                foreach(var eff in compOp.Effects)
                {
                    if (!HeuristicMethods.visitedPreds.Get(eff.Sign).ContainsKey(eff))
                    {
                        PrimaryEffects.Add(eff);
                        HeuristicMethods.visitedPreds.Get(eff.Sign)[eff] = 200;
                    }
                }
            }

        }

        private static TupleMap<IPredicate, int> PrimaryEffectRecursiveHeuristicCache(TupleMap<IPredicate, int> currentMap, List<IPredicate> InitialConditions)
        {
            var initiallyRelevant = new List<IOperator>();
            var CompositeOps = GroundActionFactory.GroundActions.Where(act => act.Height > 0);
            foreach (var compOp in CompositeOps)
            {
                var initiallySupported = true;
                foreach (var precond in compOp.Preconditions)
                {
                    if (IsPrimaryEffect(precond))
                    {
                        // then this is a primary effect.
                        if (!InitialConditions.Contains(precond))
                        {
                            initiallySupported = false;
                            break;
                        }
                    }
                }
                if (initiallySupported)
                {
                    initiallyRelevant.Add(compOp);
                }
            }

            // a boolean tag to decide whether to continue recursively. If checked, then there is some new effect that isn't in initial conditions.
            bool toContinue = false;

            // for each step whose preconditions are executable given the initial conditions
            foreach (var newStep in initiallyRelevant)
            {
                // sum_{pre in newstep.preconditions} currentMap[pre]
                int thisStepsValue = 0;
                foreach (var precon in newStep.Preconditions)
                {
                    if (IsPrimaryEffect(precon))
                    {
                        thisStepsValue += currentMap.Get(precon.Sign)[precon];
                    }
                    else
                    {
                        thisStepsValue += HeuristicMethods.visitedPreds.Get(precon.Sign)[precon];
                    }
                }

                foreach (var eff in newStep.Effects)
                {
                    if (!IsPrimaryEffect(eff))
                    {
                        continue;
                    }

                    // ignore effects we've already seen; these occur "earlier" in planning graph
                    if (currentMap.Get(eff.Sign).ContainsKey(eff))
                        continue;

                    // If we make it this far, then we've reached an unexplored literal effect
                    toContinue = true;

                    // The current value of this effect is 1 (this new step) + the sum of the preconditions of this step in the map.
                    currentMap.Get(eff.Sign)[eff] = 1 + thisStepsValue;

                    // Add this effect to the new initial Condition for subsequent round
                    InitialConditions.Add(eff);
                }
            }

            // Only continue recursively if we've explored a new literal effect. Pass the map along to maintain a global item.
            if (toContinue)
                return PrimaryEffectRecursiveHeuristicCache(currentMap, InitialConditions);

            // Otherwise, return our current map
            return currentMap;

        }
    }

    
}