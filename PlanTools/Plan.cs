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
        private Domain domain;
        private Problem problem;
        private List<IOperator> steps;
        private IState initial;
        private IState goal;
        private float estimate;
        private Graph<IOperator> orderings;
        private ICausalLinkGraph causalLinks;

        public float Estimate
        {
            get { return Estimate; }
            set { estimate = value;  }
        }

        // Access the plan's domain.
        public Domain Domain
        {
            get { return domain; }
            set { domain = value; }
        }

        // Access the plan's problem.
        public Problem Problem
        {
            get { return problem; }
            set { problem = value; }
        }

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
            domain = new Domain();
            problem = new Problem();
            steps = new List<IOperator>();
            causalLinks = new CausalLinkGraph();
            orderings = new Graph<IOperator>();
            initial = new State();
            goal = new State();
        }

        public Plan (Domain domain, Problem problem, List<IOperator> steps)
        {
            this.domain = domain;
            this.problem = problem;
            this.steps = steps;
            this.causalLinks = new CausalLinkGraph();
            this.orderings = new Graph<IOperator>();
            initial = new State();
            goal = new State();
        }

        public Plan(Domain domain, Problem problem, List<IOperator> steps, IState initial)
        {
            this.domain = domain;
            this.problem = problem;
            this.steps = steps;
            this.causalLinks = new CausalLinkGraph();
            this.initial = initial;
            goal = new State();
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

            return new Plan(domain, problem, newSteps, newInitial);
        }
    }
}