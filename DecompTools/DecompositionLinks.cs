using BoltFreezer.Interfaces;
using BoltFreezer.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.DecompTools
{
    [Serializable]
    public class DecompositionLinks
    {
        protected Dictionary<int, List<int>> SubStepMap;
        protected Dictionary<int, int> ParentMap;

        public DecompositionLinks()
        {
            SubStepMap = new Dictionary<int, List<int>>();
            ParentMap = new Dictionary<int, int>();
        }

        public void Insert(CompositeSchedulePlanStep csps, IPlanStep substep)
        {
            ParentMap[substep.ID] = csps.ID;

            if (!SubStepMap.ContainsKey(csps.ID))
            {
                SubStepMap[csps.ID] = new List<int>();
            }
            SubStepMap[csps.ID].Add(substep.ID);
            if (!ParentMap.ContainsKey(csps.ID))
            {
                ParentMap[csps.ID] = -1;
            }
        }

        public int GetRoot(IPlanStep step)
        {
            return GetRoot(step.ID);
        }

        protected int GetRoot(int stepID)
        {
            if (!ParentMap.ContainsKey(stepID))
            {
                return stepID;
            }
            var parent = ParentMap[stepID];
            
            if (parent == -1)
            {
                return stepID;
            }
            return GetRoot(parent);
        }

        public List<int> GetSubSteps(IPlanStep step)
        {
            if (!SubStepMap.ContainsKey(step.ID))
            {
                return new List<int>();
            }
            return SubStepMap[step.ID];
        }

        public void RemoveSubStep(IPlanStep parent, IPlanStep child)
        {
            SubStepMap[parent.ID].Remove(child.ID);
            ParentMap[child.ID] = -2;
        }

        public bool OnDecompPath(IPlanStep a, int target)
        {
            return OnDecompPath(a.ID, target);
        }

        protected bool OnDecompPath(int a, int target)
        {
            if (a == target)
            {
                return true;
            }

            if (!ParentMap.ContainsKey(a))
            {
                return false;
            }

            if (ParentMap[a] == -1)
            {
                return false;
            }

            if (ParentMap[a] == target)
            {
                return true;
            }

            var parent = ParentMap[a];
            if (parent == -1)
            {
                return false;
            }

            if (parent == -2)
            {
                throw new System.Exception("traveled up wrong tree");
            }
            return OnDecompPath(parent, target);
        }

        protected int GetParent(int stepID)
        {
            if (!ParentMap.ContainsKey(stepID))
            {
                return -2;
            }
            return ParentMap[stepID];
        }

        public int GetParent(IPlanStep step)
        {
            return GetParent(step.ID);
        }

        public DecompositionLinks Clone()
        {
            var newDeLinks = new DecompositionLinks();
            var newDescMap = SubStepMap.ToDictionary(keyvalue => keyvalue.Key, keyvalue => keyvalue.Value.ToList());
            var newrootmap = ParentMap.ToDictionary(keyvalue => keyvalue.Key, keyvalue => keyvalue.Value);
            newDeLinks.SubStepMap = newDescMap;
            newDeLinks.ParentMap = newrootmap;
            return newDeLinks;
        }


    }
}
