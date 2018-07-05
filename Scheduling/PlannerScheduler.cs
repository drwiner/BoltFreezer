using BoltFreezer.CacheTools;
using BoltFreezer.DecompTools;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoltFreezer.Scheduling
{
    [Serializable]
    public class PlannerScheduler : PlanSpacePlanner, IPlanner
    {
        
        public PlannerScheduler(IPlan initialPlan, ISelection _selection, ISearch _search) : base(initialPlan, _selection, _search, false)
        {
            
        }

        public new static IPlan CreateInitialPlan(ProblemFreezer PF)
        {
            return CreateInitialPlan(PF.testProblem);
        }

        public static IPlan CreateInitialPlan(Problem problem)
        {
            var initialPlan = new PlanSchedule(new Plan(new State(problem.Initial) as IState, new State(problem.Goal) as IState), new List<Tuple<IPlanStep, IPlanStep>>(), new List<Tuple<int, int>>(), new DecompositionLinks());
            foreach (var goal in problem.Goal)
                initialPlan.Flaws.Add(initialPlan, new OpenCondition(goal, initialPlan.GoalStep as IPlanStep));
            initialPlan.Orderings.Insert(initialPlan.InitialStep, initialPlan.GoalStep);
            return initialPlan;
        }

        public static IPlan CreateInitialPlan(List<IPredicate> Initial, List<IPredicate> Goal)
        {
            var initialPlan = new PlanSchedule(new Plan(new State(Initial) as IState, new State(Goal) as IState), new List<Tuple<IPlanStep, IPlanStep>>(), new List<Tuple<int, int>>(), new DecompositionLinks());
            foreach (var goal in Goal)
                initialPlan.Flaws.Add(initialPlan, new OpenCondition(goal, initialPlan.GoalStep as IPlanStep));
            initialPlan.Orderings.Insert(initialPlan.InitialStep, initialPlan.GoalStep);
            return initialPlan;
        }

        public new void Insert(IPlan plan)
        {
            var planschedule = plan as PlanSchedule;

            long before = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            before = watch.ElapsedMilliseconds;
            if (planschedule.Cntgs.HasFault(plan.Orderings))
            {
                LogTime("CheckFaults", watch.ElapsedMilliseconds - before);
                plan = null;
                return;
            }
            LogTime("CheckFaults", watch.ElapsedMilliseconds - before);

            base.Insert(plan);
        }

        public new void AddStep(IPlan plan, OpenCondition oc)
        {
            long before = 0;
            // check oc step depth.
           
            var watch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var cndt in CacheMaps.GetCndts(oc.precondition))
            {
                if (cndt == null)
                    continue;
                if (cndt.Height == 0)
                {
                    continue;
                }

                before = watch.ElapsedMilliseconds;
                
                
                var planClone = plan.Clone() as PlanSchedule;
                //if (planClone.ID.Equals("1335"))
                //{
                //    Console.WriteLine("Here");
                //}
                
                planClone.ID += "a";
                IPlanStep newStep;
                if (cndt.Height > 0)
                {
                    //continue;
                    var compCndt = cndt as CompositeSchedule;
                    newStep = new CompositeSchedulePlanStep(compCndt.Clone() as CompositeSchedule)
                    {
                        Depth = oc.step.Depth
                    };
                    
                }
                else
                {
                    // only add composite steps...
                    //continue;
                    newStep = new PlanStep(cndt.Clone() as IOperator)
                    {
                        Depth = oc.step.Depth
                    };
                }
                LogTime("CloneCndt", watch.ElapsedMilliseconds - before);

                

                before = watch.ElapsedMilliseconds;
                planClone.Insert(newStep);
                LogTime("InsertDecomp", watch.ElapsedMilliseconds - before);

                //newStep.Height = cndt.Height;
                

               // planClone.Insert(newStep);
                

                before = watch.ElapsedMilliseconds;
                planClone.Repair(oc, newStep);
                LogTime("RepairDecomp", watch.ElapsedMilliseconds - before);

                // check if inserting new Step (with orderings given by Repair) add cndts/risks to existing open conditions, affecting their status in the heap
                //planClone.Flaws.UpdateFlaws(planClone, newStep);

                if (oc.isDummyGoal)
                {
                    if (newStep.Height > 0)
                    {
                        var compNewStep = newStep as CompositeSchedulePlanStep;
                        planClone.Orderings.Insert(oc.step.InitCndt, compNewStep.InitialStep);
                    }
                    else
                    {
                        planClone.Orderings.Insert(oc.step.InitCndt, newStep);
                    }
                }
                

                before = watch.ElapsedMilliseconds;
                planClone.DetectThreats(newStep);
                LogTime("DetectThreats", watch.ElapsedMilliseconds - before);

                before = watch.ElapsedMilliseconds;
                Insert(planClone);
                LogTime("InsertPlan", watch.ElapsedMilliseconds - before);
            }
        }

        public new void Reuse(IPlan plan, OpenCondition oc)
        {
            // if repaired by initial state
            if (plan.Initial.InState(oc.precondition))
            {
                var planClone = plan.Clone() as IPlan;
                planClone.Repair(oc, planClone.InitialStep);
                planClone.ID += "ri";
                Insert(planClone);
            }

            foreach (var step in plan.Steps)
            {
                if (oc.step.ID == step.ID)
                {
                    continue;
                }

                if (step.Height > 0)
                {
                    if (CacheMaps.IsCndt(oc.precondition, step))
                    {

                        var stepAsComposite = step as CompositeSchedulePlanStep;

                        if (stepAsComposite.SubSteps.Contains(oc.step))
                        {
                            continue;
                        }
                        // before adding a repair, check if there is a path.
                        if (plan.Orderings.IsPath(oc.step, stepAsComposite.GoalStep))
                            continue;

                        if (plan.Orderings.IsPath(oc.step, stepAsComposite.InitialStep))
                            continue;

                        var planClone = plan.Clone() as IPlan;
                        if (planClone.ID.Equals("1335a"))
                        {
                            Console.WriteLine("Here");
                        }
                        // need to modify stepAsComposite, so going to rereference on cloned plan.
                        var stepAsCompositeClone = planClone.Steps.First(s => s.ID == stepAsComposite.ID) as CompositeSchedulePlanStep;
                        planClone.Repair(oc, stepAsCompositeClone);
                        planClone.ID += "r";
                        Insert(planClone);
                    }
                    continue;
                }
                else
                {

                    if (step == oc.step.InitCndt && oc.hasDummyInit)
                    {
                        var planClone = plan.Clone() as IPlan;
                        planClone.Repair(oc, step);
                        planClone.ID += "r_";
                        Insert(planClone);
                        continue;
                    }

                    if (CacheMaps.IsCndt(oc.precondition, step))
                    {
                        // before adding a repair, check if there is a path.
                        if (plan.Orderings.IsPath(oc.step, step))
                            continue;

                        var planClone = plan.Clone() as IPlan;
                        planClone.Repair(oc, step);
                        planClone.ID += "r";
                        Insert(planClone);
                    }
                }
            }
        }

        public new void RepairThreat(IPlan plan, ThreatenedLinkFlaw tclf)
        {

            var cl = tclf.causallink;
            var threat = tclf.threatener;
            if (threat is CompositeSchedulePlanStep cps)
            {
                if (!plan.Orderings.IsPath(cps.InitialStep, cl.Tail))
                {
                    var promote = plan.Clone() as IPlan;
                    promote.ID += "p";

                    if (cl.Tail.Name.Equals("DummyInit"))
                    {
                        promote.Orderings.Insert(cl.Tail.GoalCndt, cps.InitialStep);
                    }

                    promote.Orderings.Insert(cl.Tail, cps.InitialStep);
                    // because no guaranteed ordering between head and tail
                    promote.Orderings.Insert(cl.Head, cps.InitialStep);
                    Insert(promote);
                }
                if (!plan.Orderings.IsPath(cl.Head, cps.GoalStep))
                {
                    var demote = plan.Clone() as IPlan;
                    demote.ID += "d";

                    if (cl.Head.Name.Equals("DummyGoal"))
                    {
                        demote.Orderings.Insert(cps.GoalStep, cl.Head.InitCndt);
                    }
                    demote.Orderings.Insert(cps.GoalStep, cl.Head);
                    // because no guaranteed ordering between head and tail
                    demote.Orderings.Insert(cps.GoalStep, cl.Tail);
                    Insert(demote);
                }
            }
            else
            {
                // Promote
                if (!plan.Orderings.IsPath(threat, cl.Tail))
                {
                    var promote = plan.Clone() as IPlan;
                    promote.ID += "p";

                    if (cl.Tail.Name.Equals("DummyInit"))
                    {
                        promote.Orderings.Insert(cl.Tail.GoalCndt, threat);
                    }
                    promote.Orderings.Insert(cl.Tail, threat);
                    Insert(promote);
                }

                // Demote
                if (!plan.Orderings.IsPath(cl.Head, threat))
                {
                    var demote = plan.Clone() as IPlan;
                    demote.ID += "d";
                    if (cl.Head.Name.Equals("DummyGoal"))
                    {
                        demote.Orderings.Insert(threat, cl.Head.InitCndt);
                    }
                    demote.Orderings.Insert(threat, cl.Head);
                    demote.Orderings.Insert(threat, cl.Tail);
                    Insert(demote);
                }
            }
        }

    }
    
}