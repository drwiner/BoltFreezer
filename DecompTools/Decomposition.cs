using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.DecompTools
{
    public class Decomposition : Operator, IComposite
    {
        private IOperator initialStep;
        private IOperator goalStep;
        private List<Tuple<IPlanStep, IPlanStep>> subOrderings;
        private List<CausalLink<IPlanStep>> subLinks;
        private List<IPlanStep> subSteps;
        private List<IPredicate> literals;

        public List<IPredicate> Literals
        {
            get { return literals; }
            set { literals = value; }
        }

        public IOperator InitialStep
        {
            get { return initialStep; }
            set { initialStep = value; }
        }

        public IOperator GoalStep
        {
            get { return goalStep; }
            set { goalStep = value; }
        }

        public List<IPlanStep> SubSteps
        {
            get { return subSteps; }
            set { subSteps = value; }
        }

        public List<Tuple<IPlanStep, IPlanStep>> SubOrderings
        {
            get { return subOrderings; }
            set { subOrderings = value; }
        }

        public List<CausalLink<IPlanStep>> SubLinks
        {
            get { return subLinks; }
            set { subLinks = value; }
        }

        public Decomposition() : base()
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = new Operator();
            goalStep = new Operator();
            literals = new List<IPredicate>();
        }

        public Decomposition(string name, List<ITerm> terms, IOperator init, IOperator dummy, List<IPredicate> Preconditions, List<IPredicate> Effects, int ID)
            : base(name, terms, new Hashtable(), Preconditions, Effects, ID)
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = new Operator();
            goalStep = new Operator();
        }

        public Decomposition(IOperator core, List<IPredicate> literals, List<IPlanStep> substeps, List<Tuple<IPlanStep, IPlanStep>> suborderings, List<CausalLink<IPlanStep>> sublinks)
            : base(core.Name, core.Terms, new Hashtable(), core.Preconditions, core.Effects, core.ID)
        {
            this.literals = literals;
            subOrderings = suborderings;
            subLinks = sublinks;
            subSteps = substeps;
            initialStep = new Operator();
            goalStep = new Operator();
        }

        public static List<Composite> Plannify(Decomposition decomp)
        {
            var compList = new List<Composite>();

            // For each substep, find consistent ground step, check if it is "arg consistent". Each sub-step arg has unique ID. predicate-based args cannot be unique instances. 
                                                                // Also check if non equality constraints are observed. But, it may be that this isn't needed. it is in cinepydpocl not in pydpocl
            // Then, filter and add orderings. When we add orderings, we also add orderings for all sub-steps
            // Then add links and check if links are possible. If the linked condition is null, then any link condition will do.
            // Finally, Create a Composite step out of this by propagating preconditions and effects to the top-level. 

            return compList;
        }

        public static List<IOperator> ConsistentSteps(IPlanStep substep)
        {
            var opList = new List<IOperator>();

            return opList;
        }


        public new Object Clone()
        {
            return new Decomposition(this as IOperator, Literals, SubSteps, SubOrderings, SubLinks);
        }
    }
}
