using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class OpenCondition
    {
        public Predicate precondition;
        public Operator step;

        public OpenCondition ()
        {
            precondition = new Predicate();
            step = new Operator();
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
    }

    [Serializable]
    public class ThreatenedLinkFlaw
    {
        public CausalLink causallink;
        public Operator threatener;
        public ThreatenedLinkFlaw()
        {
            causallink = new CausalLink();
            threatener = new Operator();
        }

        public ThreatenedLinkFlaw(CausalLink _causallink, Operator _threat)
        {
            this.causallink = _causallink;
            this.threatener = _threat;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("CausalLink: " + causallink);

            sb.AppendLine("Threat: " + threatener);

            return sb.ToString();
        }
    }
}
