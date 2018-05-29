using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    public class CompositePlanStep : PlanStep, ICompositePlanStep
    {
        protected IComposite compositeAction;
        protected IPlanStep initialStep;
        protected IPlanStep goalStep;
        protected List<Tuple<IPlanStep, IPlanStep>> subOrderings;
        protected List<CausalLink<IPlanStep>> subLinks;
        protected List<IPlanStep> subSteps;

        public IComposite CompositeAction {
            get { return compositeAction; }
            set { compositeAction = value; }
        }

        public List<IPlanStep> SubSteps
        {
            get { return subSteps; }
            set { subSteps = value; }
        }

        public List<Tuple<IPlanStep, IPlanStep>> SubOrderings
        {
            get { return subOrderings; }
        }

        public List<CausalLink<IPlanStep>> SubLinks
        {
            get { return subLinks; }
        }

        public IPlanStep InitialStep
        {
            get { return initialStep; }
            set { initialStep = value; }
        }

        public IPlanStep GoalStep
        {
            get { return goalStep; }
            set { goalStep = value; }
        }

        public CompositePlanStep()
        {
            compositeAction = new Composite();
        }

        public CompositePlanStep(IComposite comp) : base(comp as IOperator)
        {
            compositeAction = comp;
            initialStep = new PlanStep(comp.InitialStep);
            goalStep = new PlanStep(comp.GoalStep);

            // Do not bother changing the PlanStep skin of these, as this has to happen during insertion
            subSteps = comp.SubSteps;
            subLinks = comp.SubLinks;
            subOrderings = comp.SubOrderings;
        }

        public CompositePlanStep(ICompositePlanStep comp) : base(comp as IPlanStep)
        {
            compositeAction = comp.Action as IComposite;
            initialStep = new PlanStep(comp.InitialStep);
            goalStep = new PlanStep(comp.GoalStep);

            // Do not bother changing the PlanStep skin of these, as this has to happen during insertion
            subSteps = comp.SubSteps;
            subLinks = comp.SubLinks;
            subOrderings = comp.SubOrderings;
        }

        public CompositePlanStep(IComposite comp, List<IPredicate> openconditions, IPlanStep init, IPlanStep goal, 
            List<IPlanStep> substeps, List<Tuple<IPlanStep, IPlanStep>> suborderings, List<CausalLink<IPlanStep>> clinks, int ID) 
            : base(comp as IOperator, openconditions, ID)
        {
            compositeAction = comp;
            initialStep = init;
            goalStep = goal;
            subSteps = substeps;
            subOrderings = suborderings;
            subLinks = clinks;
        }

        public CompositePlanStep(ICompositePlanStep comp, List<IPredicate> openconditions, IPlanStep init, IPlanStep goal, int ID) : base(comp as IPlanStep, openconditions, ID)
        {
            compositeAction = comp.Action as IComposite;
            initialStep = init;
            goalStep = goal;
            subSteps = comp.SubSteps;
            subOrderings = comp.SubOrderings;
            subLinks = comp.SubLinks;
        }

        // Declaring new Composite Plan Step from sub-step, base(ps) will assign new ID to plan step.
        public CompositePlanStep(IPlanStep ps) : base(ps)
        {
            compositeAction = ps.Action as Composite;

            // This is the moment the initial step and goal step are instantiated. They will be cloned before inserted
            initialStep = new PlanStep(compositeAction.InitialStep);
            goalStep = new PlanStep(compositeAction.GoalStep);

            // Sub-steps are to be newly instantiated later during inset decomp. 
            subSteps = compositeAction.SubSteps;
            subOrderings = compositeAction.SubOrderings;
            subLinks = compositeAction.SubLinks;
        }

        public new Object Clone()
        {
            return new CompositePlanStep(CompositeAction, OpenConditions, InitialStep, GoalStep, SubSteps, SubOrderings, SubLinks, ID)
            {
                Depth = base.Depth
            };
        }

    }
}
