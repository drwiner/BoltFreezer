using BoltFreezer.Enums;
using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    public class ADstar : ISearch
    {
        private IFrontier frontier;
        private bool onlyStopOnDepth;

        public IFrontier Frontier
        {
            get { return frontier; }
        }

        public SearchType SType {
                get { return SearchType.BestFirst;}
        }


        public ADstar()
        {
            onlyStopOnDepth = false;
            frontier = new PriorityQueue();
        }

        public ADstar(bool onlyPassOnDepth)
        {
            onlyStopOnDepth = onlyPassOnDepth;
            frontier = new PriorityQueue();
        }

        public List<IPlan> Search(IPlanner IP)
        {
            return Search(IP, 1, 6000);
        }

        public List<IPlan> Search(IPlanner IP, int k, float cutoff)
        {

            var Solutions = new List<IPlan>();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            while (Frontier.Count > 0)
            {
                var plan = Frontier.Dequeue();
                IP.Expanded++;
                var flaw = plan.Flaws.Next();
                Console.WriteLine(plan.Decomps);
                if (IP.Console_log)
                {
                    //Console.WriteLine(plan.ToStringOrdered());
                    //Console.WriteLine(plan.ToStringOrdered());
                    //Console.WriteLine(flaw);
                }

                // Termination criteria
                if (flaw == null)
                {
                    if (onlyStopOnDepth)
                    {
                        if (plan.Hdepth == 0)
                            continue;
                    }

                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        IP.WriteToFile(elapsedMs, plan as Plan);
                        IP.WriteTimesToFile();
                        return Solutions;
                    }
                    continue;
                }

                if (watch.ElapsedMilliseconds > cutoff)
                {
                    watch.Stop();
                    IP.WriteToFile(watch.ElapsedMilliseconds, plan as Plan);
                    IP.WriteTimesToFile();
                    return null;
                }

                var frontierCount = Frontier.Count;

                if (flaw.Ftype == Enums.FlawType.Link)
                {
                    IP.RepairThreat(plan, flaw as ThreatenedLinkFlaw);
                }

                else if (flaw.Ftype == Enums.FlawType.Condition)
                {
                    var beforeAddStep = watch.ElapsedMilliseconds;
                    IP.AddStep(plan, flaw as OpenCondition);
                    IP.LogTime("addStep", watch.ElapsedMilliseconds - beforeAddStep);

                    var beforeReuseStep = watch.ElapsedMilliseconds;
                    IP.Reuse(plan, flaw as OpenCondition);
                    IP.LogTime("reuseStep", watch.ElapsedMilliseconds - beforeReuseStep);
                }

            }

            return null;
        }

        public new string ToString()
        {
            return SType.ToString();
        }
    }

    public class DFS : ISearch
    {
        private IFrontier frontier;

        public IFrontier Frontier
        {
            get { return frontier; }
        }

        public SearchType SType
        {
            get { return SearchType.DFS; }
        }

        public DFS()
        {
            frontier = new DFSFrontier();
        }

        public List<IPlan> Search(IPlanner IP)
        {
            return Search(IP, 1, 6000f);
        }

        public new string ToString()
        {
            return SType.ToString();
        }

        public List<IPlan> Search(IPlanner IP, int k, float cutoff)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var Solutions = new List<IPlan>();

            while (Frontier.Count > 0)
            {
                var plan = Frontier.Dequeue();
                IP.Expanded++;
                var flaw = plan.Flaws.Next();

                if (IP.Console_log)
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
                        IP.WriteToFile(elapsedMs, plan as Plan);
                        return Solutions;
                    }
                    continue;
                }

                if (watch.ElapsedMilliseconds > cutoff)
                {
                    watch.Stop();
                    IP.WriteToFile(watch.ElapsedMilliseconds, plan as Plan);
                    return null;
                }

                if (flaw.Ftype == Enums.FlawType.Link)
                {
                    IP.RepairThreat(plan, flaw as ThreatenedLinkFlaw);
                }

                else if (flaw.Ftype == Enums.FlawType.Condition)
                {
                    IP.AddStep(plan, flaw as OpenCondition);
                    IP.Reuse(plan, flaw as OpenCondition);
                }

            }

            return null;
        }
    }

    public class BFS : ISearch
    {
        private bool onlyStopOnDepth;
        private IFrontier frontier;

        public IFrontier Frontier
        {
            get { return frontier; }
        }

        public SearchType SType
        {
            get { return SearchType.BFS; }
        }

        public new string ToString()
        {
            return SType.ToString();
        }

        public BFS()
        {
            onlyStopOnDepth = false;
            frontier = new BFSFrontier();
        }

        public BFS(bool onlystopOnDepth)
        {
            onlyStopOnDepth = onlystopOnDepth;
            frontier = new BFSFrontier();
        }

        public List<IPlan> Search(IPlanner IP)
        {
            return Search(IP, 1, 6000f);
        }

        public List<IPlan> Search(IPlanner IP, int k, float cutoff)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var Solutions = new List<IPlan>();

            while (Frontier.Count > 0)
            {
                var plan = Frontier.Dequeue();

                IP.Expanded++;
                var flaw = plan.Flaws.Next();

                if (IP.Console_log)
                {
                    //Console.WriteLine(plan);
                    //Console.WriteLine(flaw);
                }

                // Termination criteria
                if (flaw == null)
                {
                    if (onlyStopOnDepth)
                    {
                        if (plan.Hdepth == 0)
                            continue;
                    }

                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        IP.WriteToFile(elapsedMs, plan as Plan);
                        return Solutions;
                    }
                    continue;
                }

                if (watch.ElapsedMilliseconds > cutoff)
                {
                    watch.Stop();
                    IP.WriteToFile(watch.ElapsedMilliseconds, plan as Plan);
                    return null;
                }

                if (flaw.Ftype == Enums.FlawType.Link)
                {
                    IP.RepairThreat(plan, flaw as ThreatenedLinkFlaw);
                }

                else if (flaw.Ftype == Enums.FlawType.Condition)
                {
                    IP.AddStep(plan, flaw as OpenCondition);
                    IP.Reuse(plan, flaw as OpenCondition);
                }

            }

            return null;
        }
    }
}
