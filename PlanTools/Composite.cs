using BoltFreezer.DecompTools;
using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    public class Composite : Operator, IComposite
    {
        private IOperator initialStep;
        private IOperator goalStep;
        private List<Tuple<IPlanStep, IPlanStep>> subOrderings;
        private List<CausalLink<IPlanStep>> subLinks;
        private List<IPlanStep> subSteps;

        public IOperator InitialStep {
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
            get { return subSteps;  }
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

        public Composite() : base()
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = new Operator();
            goalStep = new Operator();
        }

        public Composite(string name, List<ITerm> terms, IOperator init, IOperator dummy, List<IPredicate> Preconditions, List<IPredicate> Effects, int ID) 
            : base(name, terms, new Hashtable(), Preconditions, Effects, ID)
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = new Operator();
            goalStep = new Operator();
        }

        public Composite(IOperator core, IOperator init, IOperator goal, List<IPlanStep> substeps, List<Tuple<IPlanStep, IPlanStep>> suborderings, List<CausalLink<IPlanStep>> sublinks)
            : base(core.Name, core.Terms, new Hashtable(), core.Preconditions, core.Effects, core.ID)
        {
            subOrderings = suborderings;
            subLinks = sublinks;
            subSteps = substeps;
            initialStep = init;
            goalStep = goal;
            Height = core.Height;
        }

        public void ApplyDecomposition(Decomposition decomp)
        {
            subSteps = decomp.SubSteps;
            subOrderings = decomp.SubOrderings;
            subLinks = decomp.SubLinks;
            // Match terms (by constants) before this... but how?
        }

        public new Object Clone()
        {
            var op = base.Clone() as IOperator;
            return new Composite(op, InitialStep, GoalStep, SubSteps, SubOrderings, SubLinks)
            {
                Height = this.Height
            };
        }
    }
}
