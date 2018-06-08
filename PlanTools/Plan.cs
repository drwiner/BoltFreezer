using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class Plan : IEquatable<Plan>, IPlan
    {
        protected List<IPlanStep> steps;
        protected IState initial;
        protected IState goal;
        protected IPlanStep initialStep = null;
        protected IPlanStep goalStep = null;

        protected Graph<IPlanStep> orderings;
        protected List<CausalLink<IPlanStep>> causalLinks;
        protected Flawque flaws;
        protected int decomps;
        protected int hdepth;

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

        public int Decomps
        {
            get { return decomps; }
            set { decomps = value; }
        }

        public int Hdepth
        {
            get { return hdepth; }
            set { hdepth = value; }
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

        public Plan(Operator _initial, Operator _goal)
        {
            steps = new List<IPlanStep>();
            causalLinks = new List<CausalLink<IPlanStep>>();
            orderings = new Graph<IPlanStep>();
            flaws = new Flawque();
            initial = new State(_initial.Effects);
            goal = new State(_goal.Preconditions);
            initialStep = new PlanStep(_initial);
            goalStep = new PlanStep(_goal);
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
            if (newStep.Height > 0)
            {
                var ns = newStep as ICompositePlanStep;
                InsertDecomp(ns);
            }
            else
            {
                InsertPrimitive(newStep);
            }
        }

        public void InsertPrimitive(IPlanStep newStep)
        {
            steps.Add(newStep);
            orderings.Insert(InitialStep, newStep);
            orderings.Insert(newStep, GoalStep);

            // Add new flaws
            foreach (var pre in newStep.OpenConditions)
            {
                Flaws.Add(this, new OpenCondition(pre, newStep));
            }

            //Flaws.UpdateFlaws(this, newStep);
            // Don't update open conditions until this newStep has ordering wrt s_{need}

            // Don't check for threats when inserting.
        }

        public void InsertPrimitiveSubstep(IPlanStep newStep, List<IPredicate> init, bool isGoal)
        {
            //    public bool hasDummyInit = false;
            //public bool isDummyGoal = false;
            steps.Add(newStep);
            orderings.Insert(InitialStep, newStep);
            orderings.Insert(newStep, GoalStep);

            // Add new flaws
            foreach (var pre in newStep.OpenConditions)
            {
                var newOC = new OpenCondition(pre, newStep);
                
                if (isGoal)
                {
                    newOC.isDummyGoal = true;
                }

                if (init.Contains(pre))
                {
                    newOC.hasDummyInit = true;
                    CausalLinks.Add(new CausalLink<IPlanStep>(pre, newStep.InitCndt, newStep));
                  //  newStep.Fulfill(pre);
                    continue;
                }

                Flaws.Add(this, newOC);
            }
        }

        public void InsertDecomp(ICompositePlanStep newStep)
        {
            decomps += 1;
            var IDMap = new Dictionary<int, IPlanStep>();

            // Clone, Add, and Order Initial step
            var dummyInit = newStep.InitialStep.Clone() as IPlanStep;
            dummyInit.Depth = newStep.Depth;
            IDMap[newStep.InitialStep.ID] = dummyInit;
            steps.Add(dummyInit);
            orderings.Insert(InitialStep, dummyInit);
            orderings.Insert(dummyInit, GoalStep);
            
            //foreach (var oc in newStep.OpenConditions)
            //{
            //    Flaws.Add(this, new OpenCondition(oc, dummyInit));
            //}


            // Clone, Add, and order Goal step
            var dummyGoal = newStep.GoalStep.Clone() as IPlanStep;
            dummyGoal.Depth = newStep.Depth;
            dummyGoal.InitCndt = dummyInit;
            InsertPrimitiveSubstep(dummyGoal, dummyInit.Effects, true);
            IDMap[newStep.GoalStep.ID] = dummyGoal;
            orderings.Insert(dummyInit, dummyGoal);
            
            // Dont need these here because its added when inserted as primitive
            //orderings.Insert(InitialStep, dummyGoal);
            //orderings.Insert(dummyGoal, GoalStep);

            // This code block is used for debugging and may possibly be ommitted. 
            // guarantee that newStepCopy cannot be used for re-use by changing ID (by casting as new step)
           
            var newStepCopy = new PlanStep(new Operator(newStep.Action.Predicate as Predicate, new List<IPredicate>(), new List<IPredicate>()));
            steps.Add(newStepCopy);
            //  orderings.Insert(dummyInit, newStepCopy);
            //  orderings.Insert(newStepCopy, dummyGoal);
            newStepCopy.Height = newStep.Height;
            var newSubSteps = new List<IPlanStep>();
            
            foreach (var substep in newStep.SubSteps)
            {
                // substep is either a IPlanStep or ICompositePlanStep
                if (substep.Height > 0)
                {
                    var compositeSubStep = new CompositePlanStep(substep.Clone() as IPlanStep)
                    {
                        Depth = newStep.Depth + 1
                    };

                    Orderings.Insert(compositeSubStep.GoalStep, dummyGoal);
                    Orderings.Insert(dummyInit, compositeSubStep.InitialStep);
                    IDMap[substep.ID] = compositeSubStep;
                    compositeSubStep.InitialStep.InitCndt = dummyInit;
                    newSubSteps.Add(compositeSubStep);
                    Insert(compositeSubStep);
                    // Don't bother updating hdepth yet because we will check on recursion
                }
                else
                {
                    var newsubstep = new PlanStep(substep.Clone() as IPlanStep)
                    {
                        Depth = newStep.Depth + 1
                    };
                    Orderings.Insert(newsubstep, dummyGoal);
                    Orderings.Insert(dummyInit, newsubstep);
                    IDMap[substep.ID] = newsubstep;
                    newSubSteps.Add(newsubstep);
                    newsubstep.InitCndt = dummyInit;
                    InsertPrimitiveSubstep(newsubstep, dummyInit.Effects, false);
                    
                    if (newsubstep.Depth > Hdepth)
                    {
                        Hdepth = newsubstep.Depth;
                    }
                }
            }

            foreach (var tupleOrdering in newStep.SubOrderings)
            {

                // Don't bother adding orderings to dummies
                if (tupleOrdering.First.Equals(newStep.InitialStep))
                    continue;
                if (tupleOrdering.Second.Equals(newStep.GoalStep))
                    continue;

                var head = IDMap[tupleOrdering.First.ID];
                var tail = IDMap[tupleOrdering.Second.ID];
                if (head.Height > 0)
                {
                    // can you pass it back?
                    var temp = head as ICompositePlanStep;
                    head = temp.GoalStep as IPlanStep;
                }
                if (tail.Height > 0)
                {
                    var temp = tail as ICompositePlanStep;
                    tail = temp.InitialStep as IPlanStep;
                }
                Orderings.Insert(head, tail);
            }

            foreach (var clink in newStep.SubLinks)
            {
                var head = IDMap[clink.Head.ID];
                var tail = IDMap[clink.Tail.ID];
                if (head.Height > 0)
                {
                    var temp = head as CompositePlanStep;
                    head = temp.GoalStep as IPlanStep;
                }
                if (tail.Height > 0)
                {
                    var temp = tail as CompositePlanStep;
                    tail = temp.InitialStep as IPlanStep;
                }

                var newclink = new CausalLink<IPlanStep>(clink.Predicate, head, tail);
                if (tail.OpenConditions.Contains(clink.Predicate))
                {
                    tail.OpenConditions.Remove(clink.Predicate);
                }
                CausalLinks.Add(newclink);
                Orderings.Insert(head, tail);

                // check if this causal links is threatened by a step in subplan
                foreach (var step in newSubSteps)
                {
                    // Prerequisite criteria 1
                    if (step.ID == head.ID || step.ID == tail.ID)
                    {
                        continue;
                    }

                    // Prerequisite criteria 2
                    if (!CacheMaps.IsThreat(clink.Predicate, step))
                    {
                        continue;
                    }

                    // If the step has height, need to evaluate differently
                    if (step.Height > 0)
                    {
                        var temp = step as ICompositePlanStep;
                        if (Orderings.IsPath(head, temp.InitialStep))
                        {
                            continue;
                        }
                        if (Orderings.IsPath(temp.GoalStep, tail))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (Orderings.IsPath(head, step))
                        {
                            continue;
                        }
                        if (Orderings.IsPath(step, tail))
                        {
                            continue;
                        }
                    }

                    if (step.Height > 0)
                    {
                        // Then we need to dig deeper to find the step that threatens
                        DecomposeThreat(clink, step as ICompositePlanStep);
                    }
                    else
                    {
                        Flaws.Add(new ThreatenedLinkFlaw(newclink, step));
                    }
                }
            }


            // This is needed because we'll check if these substeps are threatening links
            newStep.SubSteps = newSubSteps;
            newStep.InitialStep = dummyInit;
            newStep.GoalStep = dummyGoal;

            foreach (var pre in newStep.OpenConditions)
            {
                Flaws.Add(this, new OpenCondition(pre, dummyInit as IPlanStep));
            }

        }

        public IPlanStep Find(IPlanStep stepClonedFromOpenCondition)
        {
            if (GoalStep.Equals(stepClonedFromOpenCondition))
                return GoalStep;

            // For now, this condition is impossible
            if (InitialStep.Equals(stepClonedFromOpenCondition))
                return InitialStep;

            if (!Steps.Contains(stepClonedFromOpenCondition))
            {
                throw new System.Exception();
            }
            return Steps.Single(s => s.Equals(stepClonedFromOpenCondition));
        }

        // This method is used when a composite step may threaten a causal link.
        protected void DecomposeThreat(CausalLink<IPlanStep> causalLink, ICompositePlanStep ThisIsAThreat)
        {
            //if (CacheMaps.IsThreat(causalLink.Predicate, ThisIsAThreat))
            //{
            //    Flaws.Add(new ThreatenedLinkFlaw(causalLink, ThisIsAThreat.GoalStep));
            //    return;
            //}

            foreach (var substep in ThisIsAThreat.SubSteps)
            {
                
                if (substep.Height > 0)
                {
                    //var compositeSubStep = substep as ICompositePlanStep;
                    DecomposeThreat(causalLink, substep as ICompositePlanStep);
                }
                else
                {
                    if (substep.Equals(causalLink.Head) || substep.Equals(causalLink.Tail))
                    {
                        continue;
                    }
                    if (!CacheMaps.IsThreat(causalLink.Predicate, substep))
                    {
                        continue;
                    }

                    Flaws.Add(new ThreatenedLinkFlaw(causalLink, substep));
                }
            }
        }

        public void DetectThreats(IPlanStep possibleThreat)
        {
            ICompositePlanStep possibleThreatComposite = new CompositePlanStep();
            if (possibleThreat.Height > 0)
            {
                possibleThreatComposite = possibleThreat as ICompositePlanStep;
            }

            foreach (var clink in causalLinks)
            {

                if (possibleThreat.Height > 0)
                {
                    
                    if (possibleThreatComposite.SubSteps.Contains(clink.Head) || possibleThreatComposite.SubSteps.Contains(clink.Tail))
                    {
                        continue;
                    }

                    var threatGoal = possibleThreatComposite.GoalStep;

                    var threatInit = possibleThreatComposite.InitialStep;
                    
                    if (Orderings.IsPath(clink.Tail, threatInit))
                    {
                        continue;
                    }
                    if (Orderings.IsPath(threatGoal, clink.Head))
                    {
                        continue;
                    }

                    //if (CacheMaps.IsThreat(clink.Predicate, possibleThreat))
                    //{
                    //    // no need to decompose if this composite step is the explicit threat.
                    //    Flaws.Add(new ThreatenedLinkFlaw(clink, threatGoal));
                    //    return;
                    //}

                    // Now we need to consider all sub-steps since any one of them could interfere.
                    DecomposeThreat(clink, possibleThreatComposite);
                }
                else
                {
                    if (!CacheMaps.IsThreat(clink.Predicate, possibleThreat))
                    {
                        continue;
                    }

                    if (Orderings.IsPath(clink.Tail, possibleThreat))
                    {
                        continue;
                    }
                    if (Orderings.IsPath(possibleThreat, clink.Head))
                    {
                        continue;
                    }
                    Flaws.Add(new ThreatenedLinkFlaw(clink, possibleThreat));
                }
                
                
            }
        }

        public void Repair(OpenCondition oc, IPlanStep repairStep)
        {
            if (repairStep.Height > 0)
            {
                RepairWithComposite(oc, repairStep as CompositePlanStep);
            }
            else
            {
                RepairWithPrimitive(oc, repairStep);
            }
        }

        public void RepairWithPrimitive(OpenCondition oc, IPlanStep repairStep)
        {
            // oc = <needStep, needPrecond>. Need to find needStep in plan, because open conditions have been mutated before arrival.
            var needStep = Find(oc.step);

            //// we are fulfilling open conditions because open conditions can be used to add flaws.
            if (!needStep.Name.Equals("DummyGoal") && !needStep.Name.Equals("DummyInit"))
                needStep.Fulfill(oc.precondition);

            orderings.Insert(repairStep, needStep);
            var clink = new CausalLink<IPlanStep>(oc.precondition as Predicate, repairStep, needStep);
            causalLinks.Add(clink);

            foreach (var step in Steps)
            {
                if (step.Height > 0)
                {
                    continue;
                }
                if (step.ID == repairStep.ID || step.ID == needStep.ID)
                {
                    continue;
                }
                if (!CacheMaps.IsThreat(oc.precondition, step))
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
                
                Flaws.Add(new ThreatenedLinkFlaw(clink, step));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Plan objAsPart = obj as Plan;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public override int GetHashCode()
        {
            int sumo = 0;
            foreach(var step in Steps)
            {
                sumo += 23 * step.GetHashCode();
            }
            foreach(var ordering in Orderings.edges)
            {
                sumo += 23 * ordering.GetHashCode();
            }
            foreach(var clink in CausalLinks)
            {
                sumo += 23 * clink.GetHashCode();
            }
            return sumo;
        }

        public bool Equals(Plan otherPlan)
        {
            // two plans are equal if they have the same steps, orderings, and causal links
            if (otherPlan.Steps.Count != this.Steps.Count)
            {
                return false;
            }
            if (otherPlan.Orderings.edges.Count != this.Orderings.edges.Count)
            {
                return false;
            }
            if (otherPlan.CausalLinks.Count != this.CausalLinks.Count)
            {
                return false;
            }

            // we need to check the step operator, not the planstep
            var stepActions = this.Steps.Select(step => step.Action);
            var otherStepActions = otherPlan.Steps.Select(step => step.Action);
            if (stepActions.Any(clink => !otherStepActions.Contains(clink)))
            {
                return false;
            }

            var orderingActions = this.Orderings.edges.Select(tup => new Tuple<IOperator, IOperator>(tup.First.Action, tup.Second.Action));
            var otherOrderingActions = otherPlan.Orderings.edges.Select(tup => new Tuple<IOperator, IOperator>(tup.First.Action, tup.Second.Action));
            if (orderingActions.Any(clink => !otherOrderingActions.Contains(clink)))
            {
                return false;
            }
            
            var clinkActions = this.CausalLinks.Select(clink => new CausalLink<IOperator>(clink.Predicate, clink.Head.Action, clink.Tail.Action));
            var otherClinkActions = otherPlan.CausalLinks.Select(clink => new CausalLink<IOperator>(clink.Predicate, clink.Head.Action, clink.Tail.Action));
            if (clinkActions.Any(clink=> !otherClinkActions.Contains(clink)))
            {
                return false;
            }


            return true;
        }

        public void RepairWithComposite(OpenCondition oc, CompositePlanStep repairStep)
        {
            // oc = <needStep, needPrecond>. Need to find needStep in plan, because open conditions have been mutated before arrival.
            var needStep = Find(oc.step);
            if (!needStep.Name.Equals("DummyGoal") && !needStep.Name.Equals("DummyInit"))
                needStep.Fulfill(oc.precondition);

            orderings.Insert(repairStep.GoalStep as IPlanStep, needStep);
            var clink = new CausalLink<IPlanStep>(oc.precondition as Predicate, repairStep.GoalStep as IPlanStep, needStep);
            causalLinks.Add(clink);

            foreach (var step in Steps)
            {
                if (step.Height > 0)
                {
                    continue;
                }
                if (step.ID == repairStep.ID || step.ID == needStep.ID)
                {
                    continue;
                }
                if (!CacheMaps.IsThreat(oc.precondition, step))
                {
                    continue;
                }
                // step is a threat to need precondition
                if (Orderings.IsPath(needStep, step))
                {
                    continue;
                }
                if (Orderings.IsPath(step, repairStep.InitialStep as IPlanStep))
                {
                    continue;
                }

                Flaws.Add(new ThreatenedLinkFlaw(clink, step));
            }
        }

        // Return the first state of the plan.
        public State GetFirstState ()
        {
            return (State)Initial.Clone();
        }

        public List<IPlanStep> TopoSort()
        {
            List<IPlanStep> sortedList = new List<IPlanStep>();

            foreach (var item in Orderings.TopoSort(InitialStep))
            {
                if (item.Equals(InitialStep) || item.Equals(GoalStep))
                    continue;

                sortedList.Add(item);
            }

            return sortedList;

        }

        // Displays the contents of the plan.
        public override string ToString ()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var step in steps)
                sb.AppendLine(step.ToString());

            return sb.ToString();
        }

        // Displays the contents of the plan.
        public string ToStringOrdered ()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var step in TopoSort())
                sb.AppendLine(step.ToString());

            return sb.ToString();
        }

        // Creates a clone of the plan. (orderings, and Links are Read-only, so only their host containers are replaced)
        public Object Clone ()
        {
            List<IPlanStep> newSteps = new List<IPlanStep>();

            foreach (var step in steps)
            {
                // need clone because these have fulfilled conditions that are mutable.
                newSteps.Add(step.Clone() as IPlanStep);
            }

            // these are static read only things
            //IState newInitial = initial.Clone() as IState;
            //IState newGoal = goal.Clone() as IState;


            IPlanStep newInitialStep = initialStep.Clone() as IPlanStep;
            // need clone of goal step because this as fulfillable conditions
            IPlanStep newGoalStep = goalStep.Clone() as IPlanStep;

            // Assuming for now that members of the ordering graph are never mutated.  If they are, then a clone will keep references to mutated members
            Graph<IPlanStep> newOrderings = orderings.Clone() as Graph<IPlanStep>;

            // Causal Links are containers whose members are not mutated.
            List<CausalLink<IPlanStep>> newLinks = new List<CausalLink<IPlanStep>>();
            foreach (var cl in causalLinks)
            {
                newLinks.Add(cl as CausalLink<IPlanStep>);
                //newLinks.Add(cl.Clone() as CausalLink<IPlanStep>);
            }

            // Inherit all flaws, must clone very flaw
            Flawque flawList = flaws.Clone() as Flawque;

            //return new Plan(newSteps, newInitial, newGoal, newInitialStep, newGoalStep, newOrderings, newLinks, flawList);
            return new Plan(newSteps, Initial, Goal, newInitialStep, newGoalStep, newOrderings, newLinks, flawList)
            {
                Hdepth = hdepth,
                Decomps = decomps
            };
        }
    }
}