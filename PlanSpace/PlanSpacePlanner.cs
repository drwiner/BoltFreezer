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
    public class PlanSpacePlanner
    {
        private SimplePriorityQueue<IPlan, float> frontier;
        private Func<IPlan, int> heuristic;
        private SearchType search;
        private bool console_log;
        private int opened, expanded = 0;
        public int problemNumber;
        public string directory;
        public HeuristicType heuristicType;

        // TODO: keep track of plan-space search tree and not just frontier
        //private List<PlanSpaceEdge> PlanSpaceGraph;

        public PlanSpacePlanner(IPlan initialPlan, SearchType _search, Func<IPlan, int> _heuristic, bool consoleLog)
        {
            console_log = consoleLog;
            heuristic = _heuristic;
            search = _search;
            frontier = new SimplePriorityQueue<IPlan, float>();
            Insert(initialPlan);
        }

        public PlanSpacePlanner(IPlan initialPlan)
        {
            console_log = false;
            //heuristic = HeuristicType.AddReuseHeuristic;
            heuristic = new AddReuseHeuristic().Heuristic;
            search = SearchType.BestFirst;
            frontier = new SimplePriorityQueue<IPlan, float>();
            Insert(initialPlan);
        }

        public void Insert(IPlan plan)
        {
            if (!plan.Orderings.HasCycle())
            {
                frontier.Enqueue(plan, EstimatePlan(plan));
                opened++;
            }
            else
                Console.WriteLine("CHeck");
        }

        public int EstimatePlan(IPlan plan)
        {
            var hEstimate = heuristic(plan);
            var cost = plan.Steps.Count;
            return cost + hEstimate;
        }

        public List<IPlan> Solve(int k, float cutoff)
        {
            if (search == SearchType.BestFirst)
            {
                return BestFirst(k, cutoff);
            }
            else if (search == SearchType.DFS)
            {
                return DFS(k, cutoff);
            }
            else if (search == SearchType.BFS)
            {
                return BFS(k, cutoff);
            }
            else
                return null;
        }

        public List<IPlan> BestFirst(int k, float cutoff)
        {
            var Solutions = new List<IPlan>();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            //
            //var elapsedMs = watch.ElapsedMilliseconds;
            //Console.Write(elapsedMs);
            //var t0 = Time.time;
            if (frontier.Count == 0)
            {
                Console.WriteLine("check");
            }

            while (frontier.Count > 0)
            {
                var plan = frontier.Dequeue();
                expanded++;
                var flaw = plan.Flaws.Next();

                if (console_log)
                {
                    Console.WriteLine(plan);
                    Console.WriteLine(flaw);
                }

                // Termination criteria
                if (flaw == null)
                {
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        if (console_log)
                        {
                            //Console.Write(plan.ToStringOrdered());
                        }
                        WriteToFile(elapsedMs, plan as Plan);
                        
                        return Solutions;
                    }
                    continue;
                }

                if (watch.ElapsedMilliseconds > cutoff)
                {
                    watch.Stop();
                    WriteToFile(watch.ElapsedMilliseconds, plan as Plan);
                }

                if (flaw.Ftype == Enums.FlawType.Link)
                {
                    RepairThreat(plan, flaw as ThreatenedLinkFlaw);
                }

                else if (flaw.Ftype == Enums.FlawType.Condition)
                {
                    AddStep(plan, flaw as OpenCondition);
                    Reuse(plan, flaw as OpenCondition);
                }

            }

            return null;
        }

        public List<IPlan> DFS(int k, float cutoff)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var Solutions = new List<IPlan>();
            var Unexplored = new Stack<IPlan>();
            var initialPlan = frontier.Dequeue();
            Unexplored.Push(initialPlan);
            while (Unexplored.Count > 0)
            {
                var plan = Unexplored.Pop();
                expanded++;
                var flaw = plan.Flaws.Next();

                if (console_log)
                {
                    Console.WriteLine(plan);
                    Console.WriteLine(flaw);
                }

                // Termination criteria
                if (flaw == null)
                {
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        WriteToFile(elapsedMs, plan as Plan);
                        return Solutions;
                    }
                    continue;
                }

                if (watch.ElapsedMilliseconds > cutoff)
                {
                    watch.Stop();
                    WriteToFile(watch.ElapsedMilliseconds, plan as Plan);
                }

                if (flaw.Ftype == Enums.FlawType.Link)
                {
                    RepairThreat(plan, flaw as ThreatenedLinkFlaw);
                }

                else if (flaw.Ftype == Enums.FlawType.Condition)
                {
                    AddStep(plan, flaw as OpenCondition);
                    Reuse(plan, flaw as OpenCondition);
                }

                while (frontier.Count > 0)
                {
                    var newPlan = frontier.Dequeue();
                    Unexplored.Push(newPlan);
                }
            }
            
            return null;
        }

        public List<IPlan> BFS(int k, float cutoff)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var Solutions = new List<IPlan>();
            var Unexplored = new Queue<IPlan>();
            var initialPlan = frontier.Dequeue();
            Unexplored.Enqueue(initialPlan);
            while (Unexplored.Count > 0)
            {
                var plan = Unexplored.Dequeue();
                expanded++;
                var flaw = plan.Flaws.Next();

                if (console_log)
                {
                    Console.WriteLine(plan);
                    Console.WriteLine(flaw);
                }

                // Termination criteria
                if (flaw == null)
                {
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        WriteToFile(elapsedMs, plan as Plan);
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        return Solutions;
                    }
                    continue;
                }

                if (watch.ElapsedMilliseconds > cutoff)
                {
                    watch.Stop();
                    WriteToFile(watch.ElapsedMilliseconds, plan as Plan);
                }

                if (flaw.Ftype == Enums.FlawType.Link)
                {
                    RepairThreat(plan, flaw as ThreatenedLinkFlaw);
                }

                else if (flaw.Ftype == Enums.FlawType.Condition)
                {
                    AddStep(plan, flaw as OpenCondition);
                    Reuse(plan, flaw as OpenCondition);
                }

                while (frontier.Count > 0)
                {
                    var newPlan = frontier.Dequeue();
                    Unexplored.Enqueue(newPlan);
                }
            }

            return null;
        }

        public void AddStep(IPlan plan, OpenCondition oc)
        {
                
            foreach(var cndt in CacheMaps.GetCndts(oc.precondition))
            {
                if (cndt == null)
                    continue;
                // only possible for reading in python json
                if (cndt.ID == plan.InitialStep.Action.ID)
                    continue;
                // same with above: cannot insert a dummy step. These will get inserted when composite step is inserted.
                if (cndt.Name.Split(':')[0].Equals("begin") || cndt.Name.Split(':')[0].Equals("finish"))
                    continue;
                if (cndt.Height > 0)
                    continue;

                var planClone = plan.Clone() as IPlan;
                var newStep = new PlanStep(cndt.Clone() as IOperator);
                planClone.Insert(newStep);
                planClone.Repair(oc, newStep);

                // check if inserting new Step (with orderings given by Repair) add cndts/risks to existing open conditions, affecting their status in the heap
                planClone.Flaws.UpdateFlaws(planClone, newStep);
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
                if (CacheMaps.IsCndt(oc.precondition, step)){
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

        public void WriteToFile(long elapsedMs, Plan plan) {
            var primitives = plan.Steps.FindAll(step => step.Height == 0).Count;
            var composites = plan.Steps.FindAll(step => step.Height > 0).Count;
            var decomps = plan.Decomps;
            var namedData = new List<Tuple<string, string>>
                        {
                            new Tuple<string, string>("problem", problemNumber.ToString()),
                            new Tuple<string, string>("heuristic", heuristicType.ToString()),
                            new Tuple<string, string>("search", search.ToString()),
                            new Tuple<string,string>("runtime", elapsedMs.ToString()),
                            new Tuple<string, string>("opened", opened.ToString()),
                            new Tuple<string, string>("expanded", expanded.ToString()),
                            new Tuple<string, string>("primitives", primitives.ToString() ),
                            new Tuple<string, string>("decomps", decomps.ToString() ),
                            new Tuple<string, string>("composites", composites.ToString() ),
                            new Tuple<string, string>("hdepth", plan.Hdepth.ToString() )
                        };

            var file = directory + problemNumber.ToString() + "-" + search.ToString() + "-" + heuristicType.ToString() + ".txt";
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                foreach (Tuple<string, string> dataItem in namedData)
                {
                    writer.WriteLine(dataItem.First + "\t" + dataItem.Second);
                }
            }
        }


    }
}
