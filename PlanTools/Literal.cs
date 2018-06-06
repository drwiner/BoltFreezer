using BoltFreezer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class Literal : Predicate, IPredicate, ITerm
    {
        public Literal(IPredicate predicate) : base(predicate.Name, predicate.Terms, predicate.Sign)
        {
            var newTerms = new List<ITerm>();
            foreach (var term in Terms)
            {
                if (term is IPredicate tryPredicate)
                {
                    var newterm = new Literal(tryPredicate);
                    newTerms.Add(newterm as ITerm);
                }
                else
                {
                    newTerms.Add(term);
                }
            }
            Terms = newTerms;
        }

        public Literal(Predicate predicate) : base(predicate.Name, predicate.Terms, predicate.Sign)
        {
            var newTerms = new List<ITerm>();
            foreach (var term in Terms)
            {
                if (term is Predicate tryPredicate)
                {
                    var newterm = new Literal(tryPredicate);
                    newTerms.Add(newterm as ITerm);
                }
                else
                {
                    newTerms.Add(term);
                }
            }
            Terms = newTerms;
        }

        public Literal(Literal lit) : base(lit.Name, lit.Terms, lit.Sign)
        {
            
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + Name.GetHashCode();
                hash = hash * 17 + sign.GetHashCode();

                for (int i = 0; i < Terms.Count; i++)
                {
                    var term = Terms[i];
                    if (term is Literal tryLiteral)
                    {
                        hash = hash * 23 + (i * 17) + tryLiteral.GetHashCode();
                    }
                    else
                    {
                        hash = hash * 23 + (i * 17) + term.GetHashCode();
                    }
                }

                return hash;
            }
        }
    }


}
