using System;
using System.Collections.Generic;
using BoltFreezer.Interfaces;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public static class CacheMaps
    {
        // Stored mappings for repair applicability
        public static Dictionary<IPredicate, List<IOperator>> CausalMap = new Dictionary<IPredicate, List<IOperator>>();

        // Stored mappings for threat detection
        public static Dictionary<IPredicate, List<IOperator>> ThreatMap = new Dictionary<IPredicate, List<IOperator>>();

        // Checks for mappings pairwise
        public static void CacheLinks(List<IOperator> groundSteps)
        {
            foreach (var tstep in groundSteps)
            {
                foreach (var tprecond in tstep.Preconditions)
                {

                    foreach (var hstep in groundSteps)
                    {
                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!CausalMap.ContainsKey(tprecond))
                                CausalMap.Add(tprecond, new List<IOperator>() { hstep });
                            else
                                CausalMap[tprecond].Add(hstep);
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!ThreatMap.ContainsKey(tprecond))
                                ThreatMap.Add(tprecond, new List<IOperator>() { hstep });
                            else
                                ThreatMap[tprecond].Add(hstep);
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
                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!CausalMap.ContainsKey(tprecond))
                                CausalMap.Add(tprecond, new List<IOperator>() { hstep });
                            else
                                CausalMap[tprecond].Add(hstep);
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!ThreatMap.ContainsKey(tprecond))
                                ThreatMap.Add(tprecond, new List<IOperator>() { hstep });
                            else
                                ThreatMap[tprecond].Add(hstep);
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
                    if (gstep.Effects.Contains(goalCondition))
                    {
                        if (!CausalMap.ContainsKey(goalCondition))
                            CausalMap.Add(goalCondition, new List<IOperator>() { gstep });
                        else
                            CausalMap[goalCondition].Add(gstep);
                    }
                    if (gstep.Effects.Contains(goalCondition.GetReversed()))
                    {
                        if (!ThreatMap.ContainsKey(goalCondition))
                            ThreatMap.Add(goalCondition, new List<IOperator>() { gstep });
                        else
                            ThreatMap[goalCondition].Add(gstep);
                    }
                }
            }
        }
    }
}