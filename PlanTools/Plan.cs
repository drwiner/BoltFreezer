using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

using BoltFreezer.Interfaces;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class Plan : IPlan
    {
        private List<IOperator> steps;
        private IState initial;
        private IState goal;

        private Graph<IOperator> orderings;
        private ICausalLinkGraph causalLinks;
        private Flawque flaws;

        // Access the plan's steps.
        public List<IOperator> Steps
        {
            get { return steps; }
            set { steps = value; }
        }

        public ICausalLinkGraph CausalLinks
        {
            get
            {
                return causalLinks;
            }
            set { causalLinks = value; }
        }   

        // Access the plan's initial state.
        public IState Initial
        {
            get { return initial; }
            set { initial = value; }
        }

        // Access the plan's goal state.
        public IState Goal
        {
            get { return goal; }
            set { goal = value; }
        }

        public Flawque Flaws
        {
            get { return flaws; }
            set { Flaws = value;  }
        }

        // Access the plan's initial step.
        public Operator InitialStep
        {
            get { return new Operator("initial", new List<IPredicate>(), initial.Predicates); }
            set { Initial.Predicates = value.Effects; }
        }

        // Access the plan's goal step.
        public Operator GoalStep
        {
            get { return new Operator("goal", goal.Predicates, new List<IPredicate>()); }
            set { Goal.Predicates = value.Preconditions; }
        }

        public Graph<IOperator> Orderings
        {
            get { return Orderings; }
            set   { throw new NotImplementedException(); }
        }

        public Plan ()
        {
            // S
            steps = new List<IOperator>();
            // O
            orderings = new Graph<IOperator>();
            // L
            causalLinks = new CausalLinkGraph();
            
            flaws = new Flawque();
            initial = new State();
            goal = new State();
        }

        public Plan(IState _initial, IState _goal)
        {
            steps = new List<IOperator>();
            causalLinks = new CausalLinkGraph();
            orderings = new Graph<IOperator>();
            flaws = new Flawque();
            initial = _initial;
            goal = _goal;
        }

        //// MARK for delete
        //public Plan(List<IOperator> steps, IState initial)
        //{
        //    this.steps = steps;
        //    this.causalLinks = new CausalLinkGraph();
        //    this.initial = initial;
        //    goal = new State();
        //    flaws = new Flawque();
        //}

        // Used when cloning a plan: <S, O, L>, F
        public Plan(List<IOperator> steps, IState initial, IState goal, Graph<IOperator> og, ICausalLinkGraph clg, Flawque flawQueue)
        {
            this.steps = steps;
            this.causalLinks = clg;
            this.orderings = og;
            this.flaws = flawQueue;
            this.initial = initial;
            this.goal = goal;
        }

        public void Insert(IOperator newStep)
        {
            steps.Add(newStep);
            orderings.Insert(InitialStep, newStep);
            orderings.Insert(newStep, GoalStep);
        }

        public void Repair(IOperator needStep, IPredicate needPrecond, IOperator repairStep)
        {
            causalLinks.Insert(new CausalLink(needPrecond as Predicate, repairStep, needStep));
        }

        // Return the first state of the plan.
        public State GetFirstState ()
        {
            return (State)Initial.Clone();
        }

        // Displays the contents of the plan.
        public override string ToString ()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Operator step in steps)
                sb.AppendLine(step.ToString());

            return sb.ToString();
        }

        // Displays the contents of the plan.
        public string ToStringDetailed ()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Operator step in steps)
                sb.AppendLine(step.ToString());

            return sb.ToString();
        }

        // Creates a clone of the plan.
        public Object Clone ()
        {
            List<IOperator> newSteps = new List<IOperator>();

            foreach (IOperator step in steps)
                newSteps.Add((IOperator)step.Clone());

            IState newInitial = initial.Clone() as IState;
            IState newGoal = goal.Clone() as IState;

            // Assuming for now that members of the ordering graph are never mutated.  If they are, then a clone will keep references to mutated members.
            // ToDo: Sanity check after HTN implementation
            Graph<IOperator> newOrderings = orderings.Clone() as Graph<IOperator>;
            ICausalLinkGraph newlinks = causalLinks.Clone() as ICausalLinkGraph;

            // Inherit all flaws
            Flawque flawList = flaws.Clone();

            return new Plan(newSteps, newInitial, newGoal, newOrderings, newlinks, flawList);
        }
    }
}