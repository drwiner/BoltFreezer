using BoltFreezer.Enums;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using Priority_Queue;
using System;
using System.Collections.Generic;

namespace BoltFreezer.PlanSpace
{
    public class PlanSpacePlanner
    {
        private SimplePriorityQueue<IPlan, float> frontier;
        private Func<IPlan, int> heuristic;
        private SearchType search;
        private bool console_log;

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

            //var t0 = Time.time;
            if (frontier.Count == 0)
            {
                Console.WriteLine("check");
            }

            while (frontier.Count > 0)
            {
                var plan = frontier.Dequeue();

                var flaw = plan.Flaws.Next();

                if (console_log)
                {
                    Console.WriteLine(plan);
                    Console.WriteLine(flaw);
                }

                // Termination criteria
                if (flaw == null)
                {
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        return Solutions;
                    }
                    continue;
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
            var Solutions = new List<IPlan>();
            var Unexplored = new Stack<IPlan>();
            var initialPlan = frontier.Dequeue();
            Unexplored.Push(initialPlan);
            while (Unexplored.Count > 0)
            {
                var plan = Unexplored.Pop();

                var flaw = plan.Flaws.Next();

                // Termination criteria
                if (flaw == null)
                {
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        return Solutions;
                    }
                    continue;
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
            var Solutions = new List<IPlan>();
            var Unexplored = new Queue<IPlan>();
            var initialPlan = frontier.Dequeue();
            Unexplored.Enqueue(initialPlan);
            while (Unexplored.Count > 0)
            {
                var plan = Unexplored.Dequeue();

                var flaw = plan.Flaws.Next();

                // Termination criteria
                if (flaw == null)
                {
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        return Solutions;
                    }
                    continue;
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
                var planClone = plan.Clone() as IPlan;
                var newStep = new PlanStep(cndt.Clone() as IOperator);
                planClone.Insert(newStep);
                planClone.Repair(oc.step, oc.precondition, newStep);
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
                planClone.Repair(oc.step, oc.precondition, planClone.InitialStep);
                Insert(planClone);
            }

            foreach (var step in plan.Steps)
            {
                if (CacheMaps.IsCndt(oc.precondition, step)){
                    var planClone = plan.Clone() as IPlan;
                    planClone.Repair(oc.step, oc.precondition, step);
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


    }
}
