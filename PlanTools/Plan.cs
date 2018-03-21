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
        private List<IPlanStep> steps;
        private IState initial;
        private IState goal;
        private IPlanStep initialStep = null;
        private IPlanStep goalStep = null;

        private Graph<IPlanStep> orderings;
        private List<CausalLink<IPlanStep>> causalLinks;
        private Flawque flaws;

        // Access the plan's steps.
        public List<IPlanStep> Steps
        {
            get { return steps; }
            set { steps = value; }
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

        // Access to plan's flaw library
        public Flawque Flaws
        {
            get { return flaws; }
            set { Flaws = value;  }
        }

        // Access the plan's initial step.
        public IPlanStep InitialStep
        {
            get { return initialStep; }
            set { initialStep = value; }
        }

        // Access the plan's goal step.
        public IPlanStep GoalStep
        {
            get { return goalStep; }
            set { goalStep = value; }
        }

        // Access to plan's ordering graph
        public Graph<IPlanStep> Orderings
        {
            get { return orderings; }
            set   { throw new NotImplementedException(); }
        }

        // Access to plan's causal links
        public List<CausalLink<IPlanStep>> CausalLinks
        {
            get { return causalLinks; }
            set { causalLinks = value; }
        }

        public Plan ()
        {
            // S
            steps = new List<IPlanStep>();
            // O
            orderings = new Graph<IPlanStep>();
            // L
            causalLinks = new List<CausalLink<IPlanStep>>();
            
            flaws = new Flawque();
            initial = new State();
            goal = new State();
            initialStep = new PlanStep(new Operator("initial", new List<IPredicate>(), initial.Predicates));
            goalStep = new PlanStep(new Operator("goal", goal.Predicates, new List<IPredicate>()));
        }

        public Plan(IState _initial, IState _goal)
        {
            steps = new List<IPlanStep>();
            causalLinks = new List<CausalLink<IPlanStep>>();
            orderings = new Graph<IPlanStep>();
            flaws = new Flawque();
            initial = _initial;
            goal = _goal;
            initialStep = new PlanStep(new Operator("initial", new List<IPredicate>(), initial.Predicates));
            goalStep = new PlanStep(new Operator("goal", goal.Predicates, new List<IPredicate>()));
        }

        // Used when cloning a plan: <S, O, L>, F
        public Plan(List<IPlanStep> steps, IState initial, IState goal, IPlanStep initialStep, IPlanStep goalStep, Graph<IPlanStep> orderings, List<CausalLink<IPlanStep>> causalLinks, Flawque flaws)
        {
            this.steps = steps;
            this.causalLinks = causalLinks;
            this.orderings = orderings;
            this.flaws = flaws;
            this.initial = initial;
            this.goal = goal;
            this.initialStep = initialStep;
            this.goalStep = goalStep;
        }

        public void Insert(IPlanStep newStep)
        {
            steps.Add(newStep);
            orderings.Insert(InitialStep, newStep);
            orderings.Insert(newStep, GoalStep);

            // Add new flaws
            foreach (var pre in newStep.OpenConditions)
            {
                Flaws.Insert(this, new OpenCondition(pre, newStep));
            }

            // Don't check for threats when inserting.
            
        }

        public void DetectThreats(IPlanStep possibleThreat)
        {
            foreach (var clink in causalLinks)
            {
                // Let it be for now that a newly inserted step cannot already be in a causal link in the plan (a head or tail). If not true, then check first.
                if (!CacheMaps.IsThreat(clink.Predicate, possibleThreat))
                {
                    continue;
                }
                // new step can possibly threaten 
                if (Orderings.IsPath(clink.Tail as IPlanStep, possibleThreat))
                {
                    continue;
                }
                if (Orderings.IsPath(possibleThreat, clink.Head as IPlanStep))
                {
                    continue;
                }
                
                Flaws.Insert(new ThreatenedLinkFlaw(clink, possibleThreat));
            }
        }

        public void Repair(IPlanStep needStep, IPredicate needPrecond, IPlanStep repairStep)
        {
            needStep.Fulfill(needPrecond);
            orderings.Insert(repairStep, needStep);
            var clink = new CausalLink<IPlanStep>(needPrecond as Predicate, repairStep, needStep);
            causalLinks.Add(clink);

            foreach (var step in Steps)
            {
                if (step.ID == repairStep.ID || step.ID == needStep.ID)
                {
                    continue;
                }
                if (!CacheMaps.IsThreat(needPrecond, step))
                {
                    continue;
                }
                // step is a threat to need precondition
                if (Orderings.IsPath(needStep, step))
                {
                    continue;
                }
                if (Orderings.IsPath(step, repairStep))
                {
                    continue;
                }
                Flaws.Insert(new ThreatenedLinkFlaw(clink, step));
            }
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
            List<IPlanStep> newSteps = new List<IPlanStep>();

            //foreach (IPlanStep step in steps)
            //    newSteps.Add((IPlanStep)step.Clone());
            foreach (var step in steps)
                newSteps.Add(step);

            //IState newInitial = initial.Clone() as IState;
            //IState newGoal = goal.Clone() as IState;

            IPlanStep newInitialStep = initialStep.Clone() as IPlanStep;
            IPlanStep newGoalStep = goalStep.Clone() as IPlanStep;

            // Assuming for now that members of the ordering graph are never mutated.  If they are, then a clone will keep references to mutated members.
            // ToDo: Sanity check after HTN implementation
            Graph<IPlanStep> newOrderings = orderings.Clone() as Graph<IPlanStep>;

            List<CausalLink<IPlanStep>> newLinks = new List<CausalLink<IPlanStep>>();
            foreach (var cl in causalLinks)
                newLinks.Add(cl);
                //newLinks.Add(cl.Clone() as CausalLink<IPlanStep>);

            // Inherit all flaws
            Flawque flawList = flaws.Clone();

            return new Plan(newSteps, initial, goal, initialStep, goalStep, newOrderings, newLinks, flawList);
        }
    }
}