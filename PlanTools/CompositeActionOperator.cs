using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class CompositeActionOperator : Composite, IComposite, IAction
    {
        public new ActionOperator initialStep;
        public new ActionOperator goalStep;

        public new ActionOperator InitialStep
        {
            get { return initialStep; }
            set { initialStep = value; }
        }

        public new ActionOperator GoalStep
        {
            get { return goalStep; }
            set { goalStep = value; }
        }


        public new List<Literal> preconditions;
        public new List<Literal> effects;

        public new List<Literal> Preconditions
        {
            get { return preconditions; }
            set { preconditions = value; }
        }

        public new List<Literal> Effects
        {
            get { return effects; }
            set { effects = value; }
        }

        public CompositeActionOperator() : base()
        {
            initialStep = new ActionOperator();
            goalStep = new ActionOperator();
        }

        public CompositeActionOperator(Composite comp) : base(comp)
        {
            initialStep = new ActionOperator(comp.InitialStep as Operator);
            goalStep = new ActionOperator(comp.GoalStep as Operator);
            foreach(var precon in comp.Preconditions)
            {
                preconditions.Add(new Literal(precon));
            }
            foreach(var eff in comp.Effects)
            {
                effects.Add(new Literal(eff));
            }
        }

        public CompositeActionOperator(Composite core, ActionOperator initStep, ActionOperator goalStep) : base(core)
        {
            this.initialStep = initStep;
            this.goalStep = goalStep;
        }

        List<Literal> IAction.Preconditions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        List<Literal> IAction.Effects { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public new System.Object Clone()
        {
            var compCloneBase = base.Clone() as Composite;
            var initStep = initialStep.Clone() as ActionOperator;
            var gStep = goalStep.Clone() as ActionOperator;
            return new CompositeActionOperator(compCloneBase, initStep, gStep);
        }

    }
}
