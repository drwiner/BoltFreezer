using BoltFreezer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class PlanStepAction : PlanStep, IPlanStep
    {
        protected new IAction action;
        protected new List<Literal> openConditions;


        public new IAction Action
        {
            get { return action; }
            set { action = value; }
        }

        // Access the operator's preconditions.
        public new List<Literal> OpenConditions
        {
            get { return openConditions; }
            set { openConditions = value; }
        }

        public PlanStepAction()
        {
            action = new ActionOperator();
            openConditions = new List<Literal>();

        }

        public PlanStepAction(IAction groundAction) : base(groundAction as IOperator)
        {
            action = groundAction;
            openConditions = new List<Literal>();
            foreach (var precondition in groundAction.Preconditions)
            {
                openConditions.Add(precondition);
            }
        }

        public PlanStepAction(IPlanStep planStep) : base(planStep)
        {
            action = planStep.Action as IAction;
            openConditions = new List<Literal>();
            foreach (var precondition in planStep.OpenConditions)
            {
                openConditions.Add(new Literal(precondition));
            }
        }

        public PlanStepAction(IOperator groundAction, List<Literal> ocs, int _id)
        {
            action = groundAction;
            id = _id;
            openConditions = new List<Literal>();
            foreach (var precondition in ocs)
            {
                openConditions.Add(precondition);
            }
        }

        public PlanStepAction(IOperator groundAction, int _id)
        {
            action = groundAction;
            id = _id;
        }

        public PlanStep(IPlanStep planStep, int _id)
        {
            action = planStep.Action;
            id = _id;
            openConditions = new List<IPredicate>();
            foreach (var precondition in planStep.Preconditions)
            {
                openConditions.Add(precondition);
            }
        }

        public void Fulfill(IPredicate condition)
        {
            if (!action.Preconditions.Contains(condition))
            {
                throw new System.Exception();
            }

            if (!OpenConditions.Contains(condition))
            {
                throw new System.Exception();
            }

            OpenConditions.Remove(condition);
        }

        // A special method for displaying fully ground steps.
        public override string ToString()
        {
            return String.Format("{0}-{1}", Action.ToString(), ID);
        }

        // Checks if two operators are equal.
        public override bool Equals(Object obj)
        {
            // Store the object as a Plan Step
            PlanStep step = obj as PlanStep;

            if (step.ID == ID)
            {
                return true;
            }
            //if (step.Action.ID == Action.ID)
            //{
            //    return true;
            //}

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

                foreach (ITerm term in Terms)
                    if (term.Bound)
                        hash = hash * 23 + term.Constant.GetHashCode();
                    else
                        hash = hash * 23 + term.Variable.GetHashCode();

                hash = hash * 23 + ID.GetHashCode();

                return hash;
            }
        }

        // the clone doesn't need to mutate the underlying action (Action)
        public Object Clone()
        {
            return new PlanStep(Action, OpenConditions, ID)
            {
                Depth = depth,
                InitCndt = initCndt
            };
        }

        public string TermAt(int position)
        {
            return Action.TermAt(position);
        }

        public object Template()
        {
            return Action.Template();
        }
    }
}

