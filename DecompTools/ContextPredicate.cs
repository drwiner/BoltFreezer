using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.DecompTools
{
    [Serializable]
    public class ContextPredicate : Predicate, IPredicate
    {
        public IPlanStep ActionRef;
        // avoid bloat for now
        //public List<Tuple<IPlanStep, IPlanStep>> OrderingRefs;
        //public List<CausalLink<IPlanStep>> LinkRefs;
        //public List<Tuple<IPlanStep, IPlanStep>> CntgRefs;

        public ContextPredicate(Predicate p, IPlanStep actionRef) //, List<Tuple<IPlanStep, IPlanStep>> orderingRefs, List<CausalLink<IPlanStep>> linkRefs, List<Tuple<IPlanStep, IPlanStep>> cntgRefs)
            : base(p.Name, p.Terms, p.Sign)
        {
            ActionRef = actionRef;
           // OrderingRefs = orderingRefs;
           // LinkRefs = linkRefs;
           // CntgRefs = cntgRefs;
        }



        // Is consistent

        public new bool IsConsistent(IPredicate other)
        {
            if (other is ContextPredicate xother)
            {
                if (!other.Name.Equals(this.name)) { return false; }
                if (xother.ActionRef.Action.ID != this.ActionRef.Action.ID)
                {
                    return false;
                }
                return true;
            }
            return false;
            
        }

        // Is Equivalent

        public bool IsEquivalent(IPredicate other)
        {
            if (other is ContextPredicate xother)
            {
                
                if (!other.Name.Equals(this.name)) { return false; }
                if (xother.ActionRef.ID != this.ActionRef.ID)
                {
                    return false;
                }
                return true;
            }
            return false;
            

        }

        public override bool Equals(Object obj)
        {
            if (obj is ContextPredicate xobj)
            {
                if (IsConsistent(xobj))
                {
                    return true;
                }
            }
            return false;
        }

        // Returns a hashcode.
        public override int GetHashCode()
        {

            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + Name.GetHashCode();
                hash = hash * 27 + ActionRef.Action.ID;

                return hash;
            }
        }

        public void UpdateActionRef(Dictionary<int, int> idMap)
        {

        }

        // Is Ground


    }
}
