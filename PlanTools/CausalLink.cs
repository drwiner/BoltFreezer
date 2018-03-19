using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using BoltFreezer.Interfaces;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class CausalLink
    {
        private IPredicate predicate;
        private IOperator head;
        private IOperator tail;
        //private List<IOperator> span;

        // Access the link's predicate.
        public IPredicate Predicate
        {
            get { return predicate; }
            set { predicate = value; }
        }

        // Access the link's head.
        public IOperator Head
        {
            get { return head; }
            set { head = value; }
        }

        // Access the link's tail.
        public IOperator Tail
        {
            get { return tail; }
            set { tail = value; }
        }

        //// Access the link's span.
        //public List<IOperator> Span
        //{
        //    get { return span; }
        //    set { span = value; }
        //}

        public CausalLink ()
        {
            predicate = new Predicate();
            head = new Operator();
            tail = new Operator();
            //span = new List<IOperator>();
        }

        public CausalLink (IPredicate predicate, IOperator head, IOperator tail)
        {
            this.predicate = predicate;
            this.head = head;
            this.tail = tail;
            //this.span = new List<IOperator>();
        }

        //public CausalLink(IPredicate predicate, IOperator head,I Operator tail, List<IOperator> span)
        //{
        //    this.predicate = predicate;
        //    this.head = head;
        //    this.tail = tail;
        //    //this.span = span;
        //}

        // Returns a bound copy of the predicate.
        public Predicate GetBoundPredicate ()
        {
            // Clone the predicate.
            Predicate boundPred = (Predicate)predicate.Clone();

            // Bind the predicate to the tail's bindings.
            /*if (tail.Bindings.Keys.Count > 0)
                boundPred.BindTerms(tail.Bindings);
            else
                boundPred.BindTerms(head.Bindings);*/

            // Return the bound predicate.
            return boundPred;
        }

        // Displays the contents of the causal link.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Predicate: " + predicate);

            sb.AppendLine("Tail: " + tail.ToString());
            
            sb.AppendLine("Head: " + head.ToString());

            return sb.ToString();
        }

        // Create a clone of the causal link.
        public Object Clone ()
        {
            return new CausalLink(predicate.Clone() as IPredicate, head.Clone() as IOperator, tail.Clone() as IOperator);
        }
    }
}
