using System.Collections;
using System.Collections.Generic;
using BoltFreezer.PlanTools;
using System;
using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using BoltFreezer.Camera;
using System.Linq;

namespace BoltFreezer.Scheduling
{
    [Serializable]
    public class PlanSchedule : Plan, IPlan
    {

        public Schedule Cntgs;
        public MergeManager MM;

        public PlanSchedule() : base()
        {
            Cntgs = new Schedule();
            MM = new MergeManager();
        }

        public PlanSchedule(IPlan plan, Schedule cntgs, MergeManager mm) : base(plan.Steps, plan.Initial, plan.Goal, plan.InitialStep, plan.GoalStep, plan.Orderings, plan.CausalLinks, plan.Flaws)
        {
            Cntgs = cntgs;
            MM = mm;
        }

        public PlanSchedule(IPlan plan, HashSet<Tuple<IPlanStep, IPlanStep>> cntgs, HashSet<Tuple<int, int>> mm) : base(plan.Steps, plan.Initial, plan.Goal, plan.InitialStep, plan.GoalStep, plan.Orderings, plan.CausalLinks, plan.Flaws)
        {
            Cntgs = new Schedule(cntgs);
            MM = new MergeManager(mm);
        }
        public PlanSchedule(IPlan plan, List<Tuple<IPlanStep, IPlanStep>> cntgs, List<Tuple<int, int>> mm) : base(plan.Steps, plan.Initial, plan.Goal, plan.InitialStep, plan.GoalStep, plan.Orderings, plan.CausalLinks, plan.Flaws)
        {
            Cntgs = new Schedule(cntgs);
            MM = new MergeManager(mm);
        }

        public new void Insert(IPlanStep newStep)
        {
            if (newStep.Height > 0)
            {
               // var ns = newStep as ICompositePlanStep;
                InsertDecomp(newStep as CompositeSchedulePlanStep);
            }
            else
            {
                InsertPrimitive(newStep);
            }
        }

        public void Insert(CompositeSchedulePlanStep newStep)
        {
            InsertDecomp(newStep);
        }

        public void InsertDecomp(CompositeSchedulePlanStep newStep)
        {
            Decomps += 1;
            var IDMap = new Dictionary<int, IPlanStep>();

            // Clone, Add, and Order Initial step
            var dummyInit = new PlanStep(newStep.InitialStep) as IPlanStep;
            dummyInit.Depth = newStep.Depth;
            IDMap[newStep.InitialStep.ID] = dummyInit;
            Steps.Add(dummyInit);
            Orderings.Insert(InitialStep, dummyInit);
            Orderings.Insert(dummyInit, GoalStep);

            // Clone, Add, and order Goal step
            var dummyGoal = new PlanStep(newStep.GoalStep) as IPlanStep;
            dummyGoal.Depth = newStep.Depth;
            dummyGoal.InitCndt = dummyInit;
            InsertPrimitiveSubstep(dummyGoal, dummyInit.Effects, true);
            IDMap[newStep.GoalStep.ID] = dummyGoal;
            Orderings.Insert(dummyInit, dummyGoal);

            dummyInit.GoalCndt = dummyGoal;

            // needs same operator ID as newStep, in order to still be referenced for primary-effect-based open conditions
            //var newStepCopy = new CompositeSchedulePlanStep(new Operator(newStep.Action.Predicate.Name, newStep.Action.Terms, new Hashtable(), new List<IPredicate>(), new List<IPredicate>(), newStep.Action.ID));
            Steps.Add(newStep);
            
            //newStepCopy.Height = newStep.Height;
            //newStepCopy.Depth = newStep.Depth;
            var newSubSteps = new List<IPlanStep>();
            newStep.Preconditions = new List<IPredicate>();
            newStep.Effects = new List<IPredicate>();
            newStep.InitialStep = dummyInit;
            newStep.GoalStep = dummyGoal;
            dummyGoal.Parent = newStep;
            dummyInit.Parent = newStep;
            //newStepCopy.InitialStep = dummyInit;
            //newStepCopy.GoalStep = dummyGoal;

            var newCamPlanSteps = new List<CamPlanStep>();
            foreach (var substep in newStep.SubSteps)
            {
                // substep is either a IPlanStep or ICompositePlanStep
                if (substep.Height > 0)
                {
                    var compositeSubStep = new CompositeSchedulePlanStep(substep.Clone() as IPlanStep)
                    {
                        Depth = newStep.Depth + 1
                    };

                    Orderings.Insert(compositeSubStep.GoalStep, dummyGoal);
                    Orderings.Insert(dummyInit, compositeSubStep.InitialStep);
                    IDMap[substep.ID] = compositeSubStep;
                    compositeSubStep.InitialStep.InitCndt = dummyInit;
                    compositeSubStep.InitialStep.GoalCndt = dummyGoal;
                    compositeSubStep.GoalStep.InitCndt = dummyInit;
                    compositeSubStep.GoalStep.GoalCndt = dummyGoal;
                    compositeSubStep.Parent = newStep;
                    
                    newSubSteps.Add(compositeSubStep);
                    Insert(compositeSubStep);
                    
                    // Don't bother updating hdepth yet because we will check on recursion
                }
                else
                {
                    IPlanStep newsubstep;
                    // new substep is either CamPlanStep or PlanStep
                    if (substep is CamPlanStep cps)
                    {
                        newsubstep = new CamPlanStep(cps)
                        {
                            Depth = newStep.Depth + 1
                        };
                        newCamPlanSteps.Add(newsubstep as CamPlanStep);
                    }
                    else
                    { 
                        newsubstep = new PlanStep(substep.Clone() as IPlanStep)
                        {
                            Depth = newStep.Depth + 1
                        };
                    }

                    Orderings.Insert(newsubstep, dummyGoal);
                    Orderings.Insert(dummyInit, newsubstep);
                    IDMap[substep.ID] = newsubstep;
                    newSubSteps.Add(newsubstep);
                    newsubstep.InitCndt = dummyInit;
                    newsubstep.GoalCndt = dummyGoal;
                    newsubstep.Parent = newStep;
                    InsertPrimitiveSubstep(newsubstep, dummyInit.Effects, false);

                    if (newsubstep.Depth > Hdepth)
                    {
                        Hdepth = newsubstep.Depth;
                    }
                }
            }

            // update action seg targets
            foreach(var cps in newCamPlanSteps)
            {
                cps.UpdateActionSegs(IDMap);
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

            // in this world, all composite plan steps are composite schedule plan steps.
            var schedulingStepComponent = newStep as CompositeSchedulePlanStep;
            foreach (var cntg in schedulingStepComponent.Cntgs)
            {
                var head = IDMap[cntg.First.ID];
                var tail = IDMap[cntg.Second.ID];
                if (head.Height > 0)
                {
                    // how do we describe a composite as being contiguous with another step?
                    var temp = head as ICompositePlanStep;
                    head = temp.GoalStep as IPlanStep;
                }
                if (tail.Height > 0)
                {
                    var temp = tail as ICompositePlanStep;
                    tail = temp.InitialStep as IPlanStep;
                }
                Cntgs.Insert(head, tail);
                // also add orderings just in case
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
                    Flaws.Add(new ThreatenedLinkFlaw(newclink, step));
                    //if (step.Height > 0)
                    //{
                    //    // Then we need to dig deeper to find the step that threatens
                    //    DecomposeThreat(clink, step as ICompositePlanStep);
                    //}
                    //else
                    //{
                    //    Flaws.Add(new ThreatenedLinkFlaw(newclink, step));
                    //}
                }
            }


            // This is needed because we'll check if these substeps are threatening links
            newStep.SubSteps = newSubSteps;
            //newStepCopy.SubSteps = newSubSteps;
            // inital
            

            newStep.InitialStep = dummyInit;

            // goal
            
            newStep.GoalStep = dummyGoal;

            foreach (var pre in newStep.OpenConditions)
            {
                Flaws.Add(this, new OpenCondition(pre, dummyInit as IPlanStep));
            }
        }

        /// <summary>
        /// throws all cntgs, orderings, and causal links containing step1 swaps with step2
        /// </summary>
        /// <param name="step1"></param>
        /// <param name="step2"></param>
        public void MergeSteps(IPlanStep step1, IPlanStep step2)
        {

            // Update Merge Manager
            MM.Insert(step2.ID, step1.ID);

            //Debug.Log(string.Format("Merged steps: {0}, {1}", step1, step2));
            var newCntgs = new HashSet<Tuple<IPlanStep, IPlanStep>>();
            foreach (var cntg in Cntgs.edges)
            {
                if (cntg.First.Equals(step1))
                {
                    newCntgs.Add(new Tuple<IPlanStep, IPlanStep>(step2, cntg.Second));
                }
                else if (cntg.Second.Equals(step1))
                {
                    newCntgs.Add(new Tuple<IPlanStep, IPlanStep>(cntg.First, step2));
                }
                else
                {
                    newCntgs.Add(cntg);
                }
            }
            var newOrderings = new HashSet<Tuple<IPlanStep, IPlanStep>>();

            foreach (var ord in Orderings.edges)
            {
                if (ord.First.Equals(step1))
                {
                    // do not carry over orderings with dummy inits and dummy goals. (Although, this probably should only be for same sub-plan...
                    if (ord.Second.Name.Equals("DummyGoal") || ord.Second.Name.Equals("DummyInit") || ord.Second.Height > 0)
                    {
                        continue;
                    }
                    newOrderings.Add(new Tuple<IPlanStep, IPlanStep>(step2, ord.Second));
                }
                else if (ord.Second.Equals(step1))
                {
                    // do not carry over orderings with dummy inits and dummy goals. (Although, this probably should only be for same sub-plan...
                    if (ord.First.Name.Equals("DummyGoal") || ord.First.Name.Equals("DummyInit") || ord.First.Height > 0)
                    {
                        continue;
                    }

                    newOrderings.Add(new Tuple<IPlanStep, IPlanStep>(ord.First, step2));
                }
                else
                {
                    newOrderings.Add(ord);
                }
            }
            var newLinks = new HashSet<CausalLink<IPlanStep>>();
            foreach (var cntg in CausalLinks)
            {
                if (cntg.Head.Equals(step1))
                {
                    newLinks.Add(new CausalLink<IPlanStep>(cntg.Predicate, step2, cntg.Tail));
                }
                else if (cntg.Tail.Equals(step1))
                {
                    newLinks.Add(new CausalLink<IPlanStep>(cntg.Predicate, cntg.Head, step2));
                }
                else
                {
                    newLinks.Add(cntg);
                }
            }

            Orderings.edges = new HashSet<Tuple<IPlanStep, IPlanStep>>();
            foreach (var newordering in newOrderings)
            {
                Orderings.Insert(newordering.First, newordering.Second);
            }

            Cntgs.edges = newCntgs;
            CausalLinks = newLinks.ToList();

            var openConditions = new List<OpenCondition>();
            // we need to take intersection of open conditions between two steps 
            var ocsWithStep1 = Flaws.OpenConditions.Where(oc => oc.step.Equals(step1)).Select(oc=> oc.precondition);
            var ocintersection = Flaws.OpenConditions.Where(oc => oc.step.Equals(step2) && ocsWithStep1.Contains(oc.precondition));
            var newOcs = Flaws.OpenConditions.Where(oc => (!oc.step.Equals(step1) && !oc.step.Equals(step2)) || ocintersection.Contains(oc));
            
            Flaws.OpenConditions = newOcs.ToList();
            steps.Remove(step1);
            // can there be a threatened causal link flaw? it would be prioritize and addressed prior
        }

        public new void Repair(OpenCondition oc, IPlanStep repairStep)
        {
            if (repairStep.Height > 0)
            {
                RepairWithComposite(oc, repairStep as CompositeSchedulePlanStep);
            }
            else
            {
                RepairWithPrimitive(oc, repairStep);
            }
        }

        public void RepairWithComposite(OpenCondition oc, CompositeSchedulePlanStep repairStep)
        {

            var needStep = Find(oc.step);
            if (!needStep.Name.Equals("DummyGoal") && !needStep.Name.Equals("DummyInit"))
                needStep.Fulfill(oc.precondition);


            // need to merge all steps that are being connected by this predicate:
            if (oc.precondition.Name.Equals("obs-starts"))
            {
                var stepThatNeedsToBeMerged = oc.precondition.Terms[0].Constant;

                IPlanStep ReferencedStep1 = new PlanStep();
                IPlanStep ReferencedStep2 = new PlanStep();
                foreach (var step in Steps)
                {
                    if (step.Action.ID.ToString().Equals(stepThatNeedsToBeMerged))
                    {
                        // Repair Step must keep active reference of root merge
                        if (repairStep.SubSteps.Contains(step))
                        {
                            // Subsumed
                            ReferencedStep1 = step;
                        }
                        // Assumes need step is 
                        else if (step.InitCndt.Equals(needStep))
                        {
                            // Subsumer
                            ReferencedStep2 = step;
                        }
                    }
                }
                if (ReferencedStep1.Name.Equals("") || ReferencedStep2.Name.Equals(""))
                {
                    //Debug.Log("never found steps to merge");
                    throw new System.Exception("Never found steps to merge");
                }

                if (ReferencedStep1.ID == ReferencedStep2.ID)
                {
                    throw new System.Exception("Steps to be merged are already equal");
                }

                MergeSteps(ReferencedStep1, ReferencedStep2);
                // var parent = ReferencedStep2.Parent as CompositeSchedulePlanStep;
                // parent.SubSteps.Remove(ReferencedStep2);
                // parent.SubSteps.Add(ReferencedStep1);
                //MergeSteps(ReferencedStep1, ReferencedStep2);

                // Here, we keep active reference of Root Merge Node
                repairStep.SubSteps.Remove(ReferencedStep1);
                repairStep.SubSteps.Add(ReferencedStep2);
                //orderings.Insert(ReferencedStep2, needStep);
                //  orderings.Insert(ReferencedStep2.GoalCndt, needStep);

                //if (ReferencedStep1.OpenConditions.Count > ReferencedStep2.OpenConditions.Count)
                //{
                //    MergeSteps(ReferencedStep1, ReferencedStep2);

                //}
                //else
                //{
                //    MergeSteps(ReferencedStep2, ReferencedStep1);
                //}


            }

            orderings.Insert(repairStep.GoalStep as IPlanStep, needStep);

            var clink = new CausalLink<IPlanStep>(oc.precondition as Predicate, repairStep.GoalStep as IPlanStep, needStep);
            causalLinks.Add(clink);
     

            foreach (var step in Steps)
            {

                if (step.ID == repairStep.ID || step.ID == needStep.ID)
                {
                    continue;
                }
                if (!CacheMaps.IsThreat(oc.precondition, step))
                {
                    continue;
                }

                if (step.Height > 0)
                {

                    // we need to check that this step's goal step
                    var stepAsComp = step as CompositeSchedulePlanStep;

                    if (stepAsComp.SubSteps.Contains(clink.Head) || stepAsComp.SubSteps.Contains(clink.Tail))
                    {
                        continue;
                        // replace this with... is Decompositional Link-based path from step to clink.Head or clink.Tail
                    }

                    var compGoal = stepAsComp.GoalStep;
                    var compInit = stepAsComp.InitialStep;

                    if (clink.Head.Parent.ID == stepAsComp.ID || clink.Tail.Parent.ID == stepAsComp.ID)
                    {
                        continue;
                    }

                    // step is a threat to need precondition
                    if (Orderings.IsPath(needStep, compInit))
                    {
                        continue;
                    }
                    if (Orderings.IsPath(compGoal, repairStep.InitialStep as IPlanStep))
                    {
                        continue;
                    }

                    
                    Flaws.Add(new ThreatenedLinkFlaw(clink, stepAsComp));
                   // Flaws.Add(new ThreatenedLinkFlaw(clink, compInit));
                }

                else
                {
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
        }

        public new System.Object Clone()
        {
            var basePlanClone = base.Clone() as Plan;
            var newPlan = new PlanSchedule(basePlanClone, new HashSet<Tuple<IPlanStep, IPlanStep>>(Cntgs.edges), new HashSet<Tuple<int, int>>(MM.Merges))
            {
                Hdepth = hdepth,
                Decomps = Decomps
            };
            newPlan.id = id + newPlan.id;
            return newPlan;
        }
    }
}
