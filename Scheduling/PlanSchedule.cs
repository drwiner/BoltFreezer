using System.Collections;
using System.Collections.Generic;
using BoltFreezer.PlanTools;
using System;
using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using BoltFreezer.Camera;
using System.Linq;
using BoltFreezer.DecompTools;

namespace BoltFreezer.Scheduling
{
    [Serializable]
    public class PlanSchedule : Plan, IPlan
    {
        //public List<CamPlanStep> CamScheduleSteps {
        //    get
        //    {
        //        return Steps.OfType<CamPlanStep>().ToList();
        //    }
        //}
        public Schedule Cntgs;

        public MergeManager MM;
        public DecompositionLinks DeLinks;

        public PlanSchedule() : base()
        {
            Cntgs = new Schedule();
            MM = new MergeManager();
            DeLinks = new DecompositionLinks();
        }

        public PlanSchedule(IPlan plan, Schedule cntgs, MergeManager mm, DecompositionLinks dlinks) : base(plan.Steps, plan.Initial, plan.Goal, plan.InitialStep, plan.GoalStep, plan.Orderings, plan.CausalLinks, plan.Flaws)
        {
            Cntgs = cntgs;
            MM = mm;
            DeLinks = dlinks;
        }

        public PlanSchedule(IPlan plan, HashSet<Tuple<IPlanStep, IPlanStep>> cntgs, HashSet<Tuple<int, int>> mm, DecompositionLinks dlinks) : base(plan.Steps, plan.Initial, plan.Goal, plan.InitialStep, plan.GoalStep, plan.Orderings, plan.CausalLinks, plan.Flaws)
        {
            Cntgs = new Schedule(cntgs);
            MM = new MergeManager(mm);
            DeLinks = dlinks.Clone();
        }
        public PlanSchedule(IPlan plan, List<Tuple<IPlanStep, IPlanStep>> cntgs, List<Tuple<int, int>> mm, DecompositionLinks dlinks) : base(plan.Steps, plan.Initial, plan.Goal, plan.InitialStep, plan.GoalStep, plan.Orderings, plan.CausalLinks, plan.Flaws)
        {
            Cntgs = new Schedule(cntgs);
            MM = new MergeManager(mm);
            DeLinks = dlinks.Clone();
        }

        public IPlanStep GetStepByID(int id)
        {
            return steps.Single(s => s.ID == id);
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
            dummyInit.InitCndt = newStep.InitialStep.InitCndt;
            
            dummyInit.Depth = newStep.Depth;
            IDMap[newStep.InitialStep.ID] = dummyInit;
            Steps.Add(dummyInit);
            Orderings.Insert(InitialStep, dummyInit);
            Orderings.Insert(dummyInit, GoalStep);


            // Clone, Add, and order Goal step
            var dummyGoal = new PlanStep(newStep.GoalStep) as IPlanStep;
            dummyGoal.Depth = newStep.Depth;
            dummyGoal.InitCndt = dummyInit;
            dummyGoal.GoalCndt = newStep.GoalStep.GoalCndt;
            InsertPrimitiveSubstep(dummyGoal, dummyInit.Effects, true);
            IDMap[newStep.GoalStep.ID] = dummyGoal;
            Orderings.Insert(dummyInit, dummyGoal);

            dummyInit.GoalCndt = dummyGoal;

            this.ID += "([" + dummyInit.ID.ToString() + ',' + dummyGoal.ID.ToString() + "])";
            

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

            DeLinks.Insert(newStep, dummyGoal);
            DeLinks.Insert(newStep, dummyInit);

            //newStepCopy.InitialStep = dummyInit;
            //newStepCopy.GoalStep = dummyGoal;

            var newCamPlanSteps = new List<CamPlanStep>();
            foreach (var substep in newStep.SubSteps)
            {
                // substep is either a IPlanStep or ICompositePlanStep
                if (substep.Height > 0)
                {
                    var compositeSubStep = new CompositeSchedulePlanStep(substep.Clone() as CompositeSchedulePlanStep)
                    {
                        Depth = newStep.Depth + 1
                    };

                    // Avoid the following issue: compositeSubStep's initial and goal step will be reassigned its ID AFTER it is inserted; thus, insert first
                    newSubSteps.Add(compositeSubStep);
                    DeLinks.Insert(newStep, compositeSubStep);
                    Insert(compositeSubStep);

                    Orderings.Insert(compositeSubStep.GoalStep, dummyGoal);
                    Orderings.Insert(dummyInit, compositeSubStep.InitialStep);

                    //this.ID += "(^Oss[" + compositeSubStep.GoalStep.ID.ToString() + ',' + dummyGoal.ID.ToString() + "])";
                 //   this.ID += "(^Oss[" + dummyInit.ID.ToString() + ',' + compositeSubStep.InitialStep.ID.ToString() + "])";

                    IDMap[substep.ID] = compositeSubStep;

                    // The initial step of the sub-step looks to this local-subplan's dummy init as it's init cndt
                    compositeSubStep.InitialStep.InitCndt = dummyInit;
                    // The goal step of the sub-step looks to this local sub-plan's dummy goal step as it's goal candidate
                    compositeSubStep.GoalStep.GoalCndt = dummyGoal;
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
                    //newsubstep.Parent = newStep;
                    DeLinks.Insert(newStep, newsubstep);

                    InsertPrimitiveSubstep(newsubstep, dummyInit.Effects, false);

                    if (newsubstep.Depth > Hdepth)
                    {
                        Hdepth = newsubstep.Depth;
                    }
                }
            }

            newStep.InitialAction = IDMap[newStep.InitialAction.ID];
            if (newStep.InitialAction is CompositeSchedulePlanStep cspsNewStep)
            {
                newStep.InitialCamAction = cspsNewStep.InitialCamAction;
            }
            else
            {
                newStep.InitialCamAction = IDMap[newStep.InitialCamAction.ID] as CamPlanStep;
            }
            
            newStep.FinalAction = IDMap[newStep.FinalAction.ID];

            if (newStep.FinalAction is CompositeSchedulePlanStep cspsNewStepf)
            {
                newStep.FinalCamAction = cspsNewStepf.FinalCamAction;
            }
            else
            {
                newStep.FinalCamAction = IDMap[newStep.FinalCamAction.ID] as CamPlanStep;
            }
    
            // update action seg targets
            foreach(var cps in newCamPlanSteps)
            {
                cps.UpdateActionSegs(IDMap);
            }

            //foreach (var precon in newStep.ContextPrecons)
            //{
            //    // these precons MUST be referencing a local sub-step; or else, they reference a precondition or effect of a sub-step.
            //    precon.ActionRef = IDMap[precon.ActionRef.ID];
            //}
            //foreach (var eff in newStep.ContextEffects)
            //{
            //    eff.ActionRef = IDMap[eff.ActionRef.ID];
            //}

            //foreach(var precon in )

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
                    var temp = head as CompositeSchedulePlanStep;
                    head = temp.GoalStep as IPlanStep;
                }
                if (tail.Height > 0)
                {
                    var temp = tail as CompositeSchedulePlanStep;
                    tail = temp.InitialStep as IPlanStep;
                }
                //this.ID += string.Format("(^Oso[{0},{1}])", head.ID, tail.ID);
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
                    var temp = head as CompositeSchedulePlanStep;
                    //var fas = temp.FinalActionSeg;
                    if (tail is CamPlanStep cps)
                    {
                        // then get final discourse step of temp // This is a HACK - because the last camera substep may be on a grandchild
                        head = temp.FinalCamAction as IPlanStep;
                    }
                    else
                    {
                        // then get the action referenced by the final action segment - already updated actionID and is already in plan.
                        head = temp.FinalAction;
                    }
                }
                if (tail.Height > 0)
                {
                    var temp = tail as CompositeSchedulePlanStep;
                    if (head is CamPlanStep cps)
                    {
                        // then get first discourse step of temp // This is a HACK - because the first camera substep may be on a grandchild
                        tail = temp.InitialCamAction;
                    }
                    else
                    {
                        tail = temp.InitialAction;
                    }
                }
                Cntgs.Insert(head, tail);
                // also add orderings just in case
                Orderings.Insert(head, tail);
               // this.ID += string.Format("(^Osc[{0},{1}])", head.ID, tail.ID);
            }

            foreach (var clink in newStep.SubLinks)
            {
                var head = IDMap[clink.Head.ID];
                var tail = IDMap[clink.Tail.ID];

                
                if (head.Height > 0)
                {
                    var temp = head as CompositeSchedulePlanStep;
                    head = temp.GoalStep as IPlanStep;
                }
                if (tail.Height > 0)
                {
                    var temp = tail as CompositeSchedulePlanStep;
                    tail = temp.InitialStep as IPlanStep;
                }

                var newclink = new CausalLink<IPlanStep>(clink.Predicate, head, tail);
                CausalLinks.Add(newclink);
                Orderings.Insert(head, tail);
                //this.ID += string.Format("(^Osl[{0},{1}])", head.ID, tail.ID);

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
        protected void RewrittenAs(IPlanStep step1, IPlanStep step2)
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
                  //  if (ord.Second.Name.Equals("DummyGoal") || ord.Second.Name.Equals("DummyInit") || ord.Second.Height > 0)
                 //   {
                 //       continue;
                //    }
                    newOrderings.Add(new Tuple<IPlanStep, IPlanStep>(step2, ord.Second));
                }
                else if (ord.Second.Equals(step1))
                {
                    // do not carry over orderings with dummy inits and dummy goals. (Although, this probably should only be for same sub-plan...
                  //  if (ord.First.Name.Equals("DummyGoal") || ord.First.Name.Equals("DummyInit") || ord.First.Height > 0)
                  //  {
                 //       continue;
                 //   }

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


            Orderings.edges = new HashSet<Tuple<IPlanStep, IPlanStep>>(newOrderings);
            //foreach (var newordering in newOrderings)
            //{
            //    Orderings.Insert(newordering.First, newordering.Second);
            //}

            Cntgs.edges = newCntgs;
            CausalLinks = newLinks.ToList();

            //var openConditions = new List<OpenCondition>();
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

        public void RepairWithMerge(IPredicate neededCondition, IPlanStep needStep, CompositeSchedulePlanStep repairStep)
        //public void RepairWithMerge(ContextPredicate neededCondition, ContextPredicate producedCondition, IPlanStep needStep, CompositeSchedulePlanStep repairStep)
        {
            // needStep is initialStep;
            var needStepComposite = GetStepByID(DeLinks.GetParent(needStep)) as CompositeSchedulePlanStep;
            var subStepTermID = neededCondition.Terms[0].Constant;
            var stepThatNeedsToBeMerged = GetStepByID(DeLinks.GetSubSteps(needStepComposite).Single(sid => GetStepByID(sid).Action.ID.ToString().Equals(subStepTermID)));
            //var stepThatNeedsToBeMerged = needStepComposite.SubSteps.Where(s => s.Action.ID.ToString().Equals(subStepTermID)).First();
            //var stepThatNeedsToBeMerged = neededCondition.ActionRef;
            var stepThatDoesMerging = GetStepByID(DeLinks.GetSubSteps(repairStep).Single(sid => GetStepByID(sid).Action.ID.ToString().Equals(subStepTermID)));
            //var stepThatDoesMerging = repairStep.SubSteps.Where(s => s.Action.ID.ToString().Equals(subStepTermID)).First();

            // We assume that no timeline decomposition contains the same exact sub-step twice.

            //var stepThatDoesMerging = producedCondition.ActionRef;

            // In case the sub-step has been merged, find the most recent reference
            //var getID = MM.FindRoot(stepThatDoesMerging.ID);
            //if (getID != stepThatDoesMerging.ID)
            //{
            //    stepThatDoesMerging = Steps.Where(s => s.ID == getID).First();
            //}

            //getID = MM.FindRoot(stepThatNeedsToBeMerged.ID);
            //if (getID != stepThatNeedsToBeMerged.ID)
            //{
            //    stepThatNeedsToBeMerged = Steps.Where(s => s.ID == getID).First();
            //}

            // rewrite the produced sub-step
            RewrittenAs(stepThatNeedsToBeMerged, stepThatDoesMerging);
            //MergeSteps(stepThatDoesMerging, stepThatNeedsToBeMerged);

            // get the produced step's decompositional root
            var repairStepRoot = GetStepByID(DeLinks.GetRoot(repairStep)) as CompositePlanStep;

            // removed the consumed rewritten sub-step
            DeLinks.RemoveSubStep(needStepComposite, stepThatNeedsToBeMerged);
            //needStepComposite.SubSteps.Remove(stepThatNeedsToBeMerged);

            // add decomp link to produced step's root
            DeLinks.Insert(needStepComposite, repairStepRoot);
            //repairStepRoot.Parent = needStepComposite;
            //needStepComposite.SubSteps.Add(repairStepRoot);
            
            // Add orderings to keep repair Step within decompositional borders
            Orderings.Insert(needStepComposite.InitialStep, repairStepRoot.InitialStep);
            //this.ID += string.Format("(^Om[{0},{1}])", needStepComposite.InitialStep.ID, repairStepRoot.InitialStep.ID);
            Orderings.Insert(repairStepRoot.GoalStep, needStepComposite.GoalStep);
            //this.ID += string.Format("(^Om[{0},{1}])", repairStepRoot.GoalStep.ID, needStepComposite.GoalStep.ID);

            Orderings.Insert(needStepComposite.InitialStep, repairStep.InitialStep);
            Orderings.Insert(repairStep.GoalStep, needStepComposite.GoalStep);

            // the needstep camera schedule sub-steps may not be ordered relative to the action being merged.
            foreach(var needCamSubStep in needStepComposite.CamScheduleSubSteps)
            {
                Orderings.Insert(repairStep.GoalStep, needCamSubStep);
                //this.ID += string.Format("(^Ocams[{0},{1}])", needCamSubStep.ID, repairStep.InitialStep.ID);
                //foreach (var repairCamSubStep in repairStep.CamScheduleSubSteps)
                //{
                //    Orderings.Insert(repairCamSubStep, needCamSubStep);
                //    this.ID += string.Format("(^Ocams[{0},{1}])", repairCamSubStep.ID, needCamSubStep.ID);
                //}
            }

        }

        private CompositePlanStep GetStepFromID(int v)
        {
            throw new NotImplementedException();
        }

        public void RepairWithComposite(OpenCondition oc, CompositeSchedulePlanStep repairStep)
        {

            var needStep = Find(oc.step);
            if (!needStep.Name.Equals("DummyGoal") && !needStep.Name.Equals("DummyInit"))
                needStep.Fulfill(oc.precondition);


            // need to merge all steps that are being connected by this predicate:
            if (oc.precondition.Name.Equals("obs-starts"))
            //if (oc.precondition is ContextPredicate cxtp)
            {
                this.ID += "s";
                //var repairEff = repairStep.Effects.Where(e => cxtp.Equals(oc.precondition)).First() ;
                RepairWithMerge(oc.precondition, needStep, repairStep);
                return;
            }
            //if (oc.precondition.Name.Equals("obs-starts"))
            //{
            //    Console.WriteLine("obs-starts");
                
            //}

            orderings.Insert(repairStep.GoalStep as IPlanStep, needStep);
            //this.ID += string.Format("(^Orl[{0},{1}])", repairStep.GoalStep.ID, needStep.ID);

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


                    if (DeLinks.OnDecompPath(clink.Head, step.ID))
                    {
                        // must be ordered within 
                        if (Orderings.IsPath(clink.Tail, stepAsComp.GoalStep))
                        {
                            // already tucked into Q's borders
                            continue;
                        }

                        if (!DeLinks.OnDecompPath(clink.Tail, step.ID))
                        {
                            // Q --> s -p-> t, not p in eff(Q), not Q --> t
                            // then, need to tuck t into Q's borders.
                            var tailRoot = GetStepByID(DeLinks.GetRoot(clink.Tail)) as CompositePlanStep;
                            Orderings.Insert(tailRoot.GoalStep, stepAsComp.InitialStep);
                            this.ID += string.Format("(^Od[{0},{1}])", tailRoot.GoalStep.ID, stepAsComp.InitialStep.ID);
                        }

                        continue;
                    }

                    if (DeLinks.OnDecompPath(clink.Tail, step.ID))
                    {
                        // step cannot threaten
                        continue;
                    }


                    // step is a threat to need precondition
                    if (Orderings.IsPath(clink.Tail, stepAsComp.InitialStep))
                    {
                        continue;
                    }
                    if (Orderings.IsPath(stepAsComp.GoalStep, repairStep.InitialStep as IPlanStep))
                    {
                        continue;
                    }

             
                    Flaws.Add(new ThreatenedLinkFlaw(clink, stepAsComp));
                   // Flaws.Add(new ThreatenedLinkFlaw(clink, compInit));
                }

                else
                {
                    // is it possible that step is a sub-step of repair step? Yes it is.
                    if (DeLinks.OnDecompPath(step, repairStep.ID))
                    {
                        // but, there's nothing we can do about it; and all links to repairStep.GoalStep are there to be threatened
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
        }

        public new void DetectThreats(IPlanStep possibleThreat)
        {
            CompositeSchedulePlanStep possibleThreatComposite = new CompositeSchedulePlanStep();
            if (possibleThreat.Height > 0)
            {
                possibleThreatComposite = possibleThreat as CompositeSchedulePlanStep;
            }

            foreach (var clink in causalLinks)
            {

                if (!CacheMaps.IsThreat(clink.Predicate, possibleThreat))
                {
                    continue;
                }

                if (possibleThreat.Height > 0)
                {

                    if (DeLinks.OnDecompPath(clink.Head, possibleThreat.ID))
                    {
                        // must be ordered within 
                        if (Orderings.IsPath(clink.Tail, possibleThreatComposite.GoalStep))
                        {
                            // already tucked into Q's borders
                            continue;
                        }

                        if (!DeLinks.OnDecompPath(clink.Tail, possibleThreat.ID))
                        {
                            // Q --> s -p-> t, not p in eff(Q), not Q --> t
                            // then, need to tuck t into Q's borders.
                            var tailRoot = GetStepByID(DeLinks.GetRoot(clink.Tail)) as CompositePlanStep;
                            Orderings.Insert(tailRoot.GoalStep, possibleThreatComposite.InitialStep);
                            this.ID += string.Format("(^Od2[{0},{1}])", tailRoot.GoalStep.ID, possibleThreatComposite.InitialStep.ID);
                        }

                        continue;
                    }

                    if (DeLinks.OnDecompPath(clink.Tail, possibleThreat.ID))
                    {
                        continue;
                    }

                    if (Orderings.IsPath(clink.Tail, possibleThreatComposite.InitialStep))
                    {
                        continue;
                    }
                    if (Orderings.IsPath(possibleThreatComposite.GoalStep, clink.Head))
                    {
                        continue;
                    }
                    Flaws.Add(new ThreatenedLinkFlaw(clink, possibleThreat));
                }
                else
                {
                    // don't need to check decomp paths, because causal links and threat are all primitive. 
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

        public new System.Object Clone()
        {
            var basePlanClone = base.Clone() as Plan;
            var newPlan = new PlanSchedule(basePlanClone, new HashSet<Tuple<IPlanStep, IPlanStep>>(Cntgs.edges), new HashSet<Tuple<int, int>>(MM.Merges), DeLinks.Clone())
            {
                Hdepth = hdepth,
                Decomps = Decomps
            };
            newPlan.id = id + newPlan.id;
            return newPlan;
        }
    }
}
