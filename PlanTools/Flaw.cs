using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BoltFreezer.PlanTools
{

    [Serializable]
    public class OpenCondition : IComparable<OpenCondition>, IFlaw
    {
        public Predicate precondition;
        public Operator step;
        public bool isStatic = false;
        public bool isInit = false;
        public int risks = 0;
        public int cndts = 0;
        
        private string flawType;

        string IFlaw.FlawType
        {
            get { return flawType; }
            set{flawType = value;}
        }

        public OpenCondition ()
        {
            precondition = new Predicate();
            step = new Operator();
            flawType = "oc";
        }

        public OpenCondition (Predicate precondition, Operator step)
        {
            this.precondition = precondition;
            this.step = step;
        }

        // Displays the contents of the flaw.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Flaw: " + precondition);
                
            sb.AppendLine("Step: " + step);

            return sb.ToString();
        }

        public int CompareTo(OpenCondition other)
        {
            if (other == null)
            {
                return 1;
            }

            // check static
            if (isStatic && !other.isStatic)
                return -1;
            else if (other.isStatic && !isStatic)
                return 1;

            // check risks
            if (risks > 0 || other.risks > 0)
            {
                if (risks == 0 && isInit && !other.isInit)
                    return -1;
                if (other.risks == 0 && other.isInit)
                    return 1;
                if (risks > other.risks)
                    return -1;
                else if (risks < other.risks)
                    return 1;
            }
            
            // check init
            if (isInit && !other.isInit)
                return -1;
            else if (other.isInit && !isInit)
                return 1;

            // check cndts
            if (cndts > other.cndts)
                return -1;
            else if (cndts < other.cndts)
                return 1;

            // resort to tiebreak criteria
            if (step.ID == other.step.ID)
            {
                if (precondition.Equals(other.precondition))
                {
                    throw new System.Exception();
                }
                else
                    return PredicateComparer.CompareTo(precondition, other.precondition);

            }
            else if (step.ID < other.step.ID)
            {
                return -1;
            }
            else
                return 1;

            throw new NotImplementedException();
        }

        public OpenCondition Clone()
        {
            var oc = new OpenCondition(precondition.Clone() as Predicate, step.Clone() as Operator);
            oc.risks = risks;
            oc.isInit = isInit;
            oc.isStatic = isStatic;
            oc.cndts = cndts;
            return oc;
        }
    }

    [Serializable]
    public class ThreatenedLinkFlaw : IComparable<ThreatenedLinkFlaw>, IFlaw
    {
        public CausalLink causallink;
        public Operator threatener;
        private string flawType;

        public ThreatenedLinkFlaw()
        {
            causallink = new CausalLink();
            threatener = new Operator();
        }

        public ThreatenedLinkFlaw(CausalLink _causallink, Operator _threat)
        {
            this.causallink = _causallink;
            this.threatener = _threat;
            flawType = "tclf";
        }

        string IFlaw.FlawType
        {
            get { return flawType; }
            set { flawType = value; }
        }

        public int CompareTo(ThreatenedLinkFlaw other)
        {
            if (other == null)
            {
                return 1;
            }
            if (threatener.ID == other.threatener.ID)
            {
                if (causallink.Predicate.Equals(other.causallink.Predicate))
                {
                    if (!causallink.Head.Equals(other.causallink.Head))
                    {
                        if (causallink.Head.ID < other.causallink.Head.ID)
                            return -1;
                        else
                            return 1;
                    }
                    else if (!causallink.Tail.Equals(other.causallink.Tail))
                    {
                        if (causallink.Tail.ID < other.causallink.Tail.ID)
                            return -1;
                        else
                            return 1;
                    }
                    // causal link is the same, and the threat is the same
                    throw new System.Exception();
                }
                else
                    return PredicateComparer.CompareTo(causallink.Predicate, other.causallink.Predicate);
            }
            else if (threatener.ID < other.threatener.ID)
            {
                return -1;
            }
            else
                return 1;

            throw new NotImplementedException();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("CausalLink: " + causallink);

            sb.AppendLine("Threat: " + threatener);

            return sb.ToString();
        }
    }


    public class Flawque
    {
        private Heap<OpenCondition> openConditions;
        private Heap<ThreatenedLinkFlaw> threatenedLinks;


        public Flawque()
        {
            openConditions = new Heap<OpenCondition>(HeapType.MinHeap);
            threatenedLinks = new Heap<ThreatenedLinkFlaw>(HeapType.MinHeap);
        }

        public void Insert(Plan plan, OpenCondition oc)
        {
            // Static? 
            if (oc.precondition.IsStatic)
            {
                oc.isStatic = true;
                return;
            }

            // accumulate risks and cndts
            foreach( var step in plan.Steps)
            {
                if (step.ID == oc.step.ID)
                    continue;
                if (plan.Orderings.IsPath(oc.step, step))
                    continue;
                // CHECK THAT THIS WORKS AS INTENDED: or else I will have created a causal and threat map
                if (step.Effects.Contains(oc.precondition))
                    oc.cndts += 1;

                if (step.Effects.Any(x=> oc.precondition.IsInverse(x)))
                    oc.risks += 1;

            }

            if (oc.risks == 0 && plan.Initial.InState(oc.precondition))
            {
                oc.isInit = true;
            }

            openConditions.Insert(oc);
        }

        public void Insert(ThreatenedLinkFlaw tclf)
        {

            threatenedLinks.Insert(tclf);
        }

        public IEnumerable<OpenCondition> OpenConditionGenerator()
        {
            foreach (var item in openConditions.ToList())
            {
                yield return item.Clone();
            }
        }

        public IFlaw Next()
        {
            // repair threatened links first
            if (!threatenedLinks.IsEmpty())
                return threatenedLinks.PopRoot();

            if (!openConditions.IsEmpty())
                return openConditions.PopRoot();

            return null;
        }

        // What to do here --> how can we reassign?
        public void AddCndtsAndRisks(Plan plan, IOperator action)
        {
            foreach(var oc in OpenConditionGenerator())
            {
                // ignore any open conditions that cannot possibly be affected by this action's effects, such as those occurring after
                if (plan.Orderings.IsPath(oc.step, action))
                    continue;

                if (action.Effects.Contains(oc.precondition))
                    oc.cndts += 1;
                
                if (action.Effects.Any(x => oc.precondition.IsInverse(x)))
                    oc.risks += 1;
            }
        }

    }
}
