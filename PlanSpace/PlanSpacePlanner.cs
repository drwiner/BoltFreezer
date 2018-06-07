using BoltFreezer.CacheTools;
using BoltFreezer.Enums;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.IO;

namespace BoltFreezer.PlanSpace
{
    public class PlanSpacePlanner : IPlanner
    {
        protected ISelection selection;
        protected ISearch search;

        protected List<Plan> Visited = new List<Plan>();

        protected bool console_log;
        protected int opened, expanded = 0;
        public int problemNumber;
        public string directory;

        public List<Tuple<string, string>> timeCollections = new List<Tuple<string, string>>();

        // TODO: keep track of plan-space search tree and not just frontier
        //private List<PlanSpaceEdge> PlanSpaceGraph;

        public bool Console_log
        {
            get { return console_log; }
            set { console_log = value; }
        }

        public int Expanded
        {
            get { return expanded; }
            set { expanded = value; }
        }

        public int Open
        {
            get { return opened; }
            set { opened = value; }
        }

        public ISearch Search
        {
            get { return search; }
        }

        public PlanSpacePlanner(IPlan initialPlan, ISelection _selection, ISearch _search, bool consoleLog)
        {
            console_log = consoleLog;
            selection = _selection;
            search = _search;
            Insert(initialPlan);
        }

        public PlanSpacePlanner(IPlan initialPlan)
        {
            console_log = false;
            selection = new E0(new AddReuseHeuristic());
            search = new ADstar();
            Insert(initialPlan);
        }

        public static IPlan CreateInitialPlan(ProblemFreezer PF)
        {
            var initialPlan = new Plan(new State(PF.testProblem.Initial) as IState, new State(PF.testProblem.Goal) as IState);
            foreach (var goal in PF.testProblem.Goal)
                initialPlan.Flaws.Add(initialPlan, new OpenCondition(goal, initialPlan.GoalStep as IPlanStep));
            initialPlan.Orderings.Insert(initialPlan.InitialStep, initialPlan.GoalStep);
            return initialPlan;
        }

        public void LogTime(string operationName, long timeItTook)
        {
            timeCollections.Add(new Tuple<string, string>(operationName, timeItTook.ToString()));
        }

        public void Insert(IPlan plan)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            long before = watch.ElapsedMilliseconds;
            if (!plan.Orderings.HasCycle())
            {
                LogTime("checkOrderings", watch.ElapsedMilliseconds - before);
                //if (Visited.Contains(plan as Plan))
                //{
                //    return;
                //}

                //Visited.Add(plan as Plan);
                Search.Frontier.Enqueue(plan, Score(plan));
                opened++;
            }
            LogTime("checkOrderings", watch.ElapsedMilliseconds - before);
        }

        public float Score(IPlan plan)
        {
            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            long before = watch2.ElapsedMilliseconds;
            var answer =  selection.Evaluate(plan);
            LogTime("evaluatePlanToInsert", watch2.ElapsedMilliseconds - before);
            LogTime("expanded", Expanded);
            return answer;
        }

        public List<IPlan> Solve(int k, float cutoff)
        {
            return search.Search(this, k, cutoff);
        }

        public void AddStep(IPlan plan, OpenCondition oc)
        {

            foreach(var cndt in CacheMaps.GetCndts(oc.precondition))
            {
                if (cndt == null)
                    continue;

                var planClone = plan.Clone() as IPlan;
                IPlanStep newStep;
                if (cndt.Height > 0)
                {
                    //continue;
                    var compCndt = cndt as IComposite;
                    newStep = new CompositePlanStep(compCndt.Clone() as IComposite)
                    {
                        Depth = oc.step.Depth
                    };
                }
                else
                {
                    newStep = new PlanStep(cndt.Clone() as IOperator)
                    {
                        Depth = oc.step.Depth
                    };
                }
                //newStep.Height = cndt.Height;

                planClone.Insert(newStep);
                planClone.Repair(oc, newStep);


                // check if inserting new Step (with orderings given by Repair) add cndts/risks to existing open conditions, affecting their status in the heap
                //planClone.Flaws.UpdateFlaws(planClone, newStep);

                if (oc.isDummyGoal)
                {
                    if (newStep.Height > 0)
                    {
                        var compNewStep = newStep as ICompositePlanStep;
                        planClone.Orderings.Insert(oc.step.InitCndt, compNewStep.InitialStep);
                    }
                    else
                    {
                        planClone.Orderings.Insert(oc.step.InitCndt, newStep);
                    }
                }
                planClone.DetectThreats(newStep);
                Insert(planClone);
            }
        }

        public void Reuse(IPlan plan, OpenCondition oc)
        {
            // if repaired by initial state
            if (plan.Initial.InState(oc.precondition))
            {
                var planClone = plan.Clone() as IPlan;
                planClone.Repair(oc, planClone.InitialStep);
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
                    continue;
                }

                if (step == oc.step.InitCndt && oc.hasDummyInit)
                {
                    var planClone = plan.Clone() as IPlan;
                    planClone.Repair(oc, step);
                    Insert(planClone);
                    continue;
                }

                //if (step.Effects.Contains(oc.precondition))
                //{
                //    Console.Write("here");
                //}

                if (CacheMaps.IsCndt(oc.precondition, step)){
                    // before adding a repair, check if there is a path.
                    if (plan.Orderings.IsPath(oc.step, step))
                        continue;
                    
                    var planClone = plan.Clone() as IPlan;
                    planClone.Repair(oc, step);
                    Insert(planClone);
                }
            }
        }

        public void RepairThreat(IPlan plan, ThreatenedLinkFlaw tclf)
        {
            
            var cl = tclf.causallink;
            var threat = tclf.threatener;

            // Promote
            if (!plan.Orderings.IsPath(threat, cl.Tail))
            {
                var promote = plan.Clone() as IPlan;
                promote.Orderings.Insert(cl.Tail, threat);
                Insert(promote);
            }

            // Demote
            if (!plan.Orderings.IsPath(cl.Head, threat))
            {
                var demote = plan.Clone() as IPlan;
                demote.Orderings.Insert(threat, cl.Head);
                Insert(demote);
            }

        }

        public void WriteTimesToFile()
        {
            var file = directory + @"/Times/" +  problemNumber.ToString() + "-" + search.ToString() + "-" + selection.ToString() + ".txt";
            Directory.CreateDirectory(directory + @"/Times/");
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                foreach (Tuple<string, string> dataItem in timeCollections)
                {
                    writer.WriteLine(dataItem.First + "\t" + dataItem.Second);
                }
                writer.WriteLine("\n");
            }
        }

        public void WriteToFile(long elapsedMs, Plan plan) {
            var primitives = plan.Steps.FindAll(step => step.Height == 0).Count;
            var composites = plan.Steps.FindAll(step => step.Height > 0).Count;
            var decomps = plan.Decomps;
            var namedData = new List<Tuple<string, string>>
                        {
                            new Tuple<string, string>("problem", problemNumber.ToString()),
                            new Tuple<string, string>("selection", selection.ToString()),
                            new Tuple<string, string>("search", search.ToString()),
                            new Tuple<string,string>("runtime", elapsedMs.ToString()),
                            new Tuple<string, string>("opened", opened.ToString()),
                            new Tuple<string, string>("expanded", expanded.ToString()),
                            new Tuple<string, string>("primitives", primitives.ToString() ),
                            new Tuple<string, string>("decomps", decomps.ToString() ),
                            new Tuple<string, string>("composites", composites.ToString() ),
                            new Tuple<string, string>("hdepth", plan.Hdepth.ToString() ),
                        };
            
            var file = directory + problemNumber.ToString() + "-" + search.ToString() + "-" + selection.ToString() + ".txt";
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                foreach (Tuple<string, string> dataItem in namedData)
                {
                    writer.WriteLine(dataItem.First + "\t" + dataItem.Second);
                }
                writer.WriteLine("\n");
                writer.WriteLine(plan.ToStringOrdered());
            }
        }


    }
}
