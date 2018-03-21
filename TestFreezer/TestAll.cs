using System;
using System.Collections.Generic;
using System.IO;
using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;

namespace TestFreezer
{
    class TestAll
    {


        public static string testDomainName = "batman";
        public static string testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
        public static Domain testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
        public static Problem testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");


        public static void FreezeProblem(bool serialize, bool deserialize)
        {
            string FileName = Parser.GetTopDirectory() + @"Cached\CachedOperators\" + testDomainName + "_" + testProblem.Name;
            string CausalMapFileName = Parser.GetTopDirectory() + @"Cached\CausalMaps\" + testDomainName + "_" + testProblem.Name;
            string ThreatMapFileName = Parser.GetTopDirectory() + @"Cached\ThreatMaps\" + testDomainName + "_" + testProblem.Name;

            if (serialize)
            {
                Console.Write("Creating Ground Operators");
                GroundActionFactory.PopulateGroundActions(testDomain.Operators, testProblem);
                //BinarySerializer.SerializeObject(FileName, GroundActionFactory.GroundActions);
                foreach (var op in GroundActionFactory.GroundActions)
                {
                    BinarySerializer.SerializeObject(FileName + op.GetHashCode().ToString() + ".CachedOperator", op);
                }

                CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
                CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, testProblem.Goal);
                BinarySerializer.SerializeObject(CausalMapFileName + ".CachedCausalMap", CacheMaps.CausalMap);
                BinarySerializer.SerializeObject(ThreatMapFileName + ".CachedThreatMap", CacheMaps.ThreatMap);
            }
            else if(deserialize)
            {
                GroundActionFactory.GroundActions = new List<IOperator>();
                GroundActionFactory.GroundLibrary = new Dictionary<int, IOperator>();
                foreach (var file in Directory.GetFiles(Parser.GetTopDirectory() + @"Cached\CachedOperators\", testDomainName + "_" + testProblem.Name + "*.CachedOperator"))
                {
                    var op = BinarySerializer.DeSerializeObject<IOperator>(file);
                    GroundActionFactory.GroundActions.Add(op);
                    GroundActionFactory.GroundLibrary[op.ID] = op;
                }
                // THIS is so that initial and goal steps created don't get matched with these
                Operator.SetCounterExternally(GroundActionFactory.GroundActions.Count + 1);

                Console.WriteLine("\nCmap\n");

                var cmap = BinarySerializer.DeSerializeObject<Dictionary<IPredicate, List<int>>>(CausalMapFileName + ".CachedCausalMap");
                CacheMaps.CausalMap = cmap;

                Console.WriteLine("\nTmap\n");
                var tcmap = BinarySerializer.DeSerializeObject<Dictionary<IPredicate, List<int>>>(ThreatMapFileName + ".CachedThreatMap");
                CacheMaps.ThreatMap = tcmap;

                Console.WriteLine("Finding Statics");
                GroundActionFactory.DetectStatics(CacheMaps.CausalMap, CacheMaps.ThreatMap);
                foreach (var stat in GroundActionFactory.Statics)
                {
                    Console.WriteLine(stat);
                }
            }
        }


        public static void RunAddReusePop(IPlan initialPlan)
        {
            Console.WriteLine("First POP");
            var AStarPOP = new PlanSpacePlanner(initialPlan, SearchType.BestFirst, new AddReuseHeuristic().Heuristic, true);
            var bestFirstSolutions = AStarPOP.Solve(1, 6000f);
            Console.WriteLine(bestFirstSolutions[0].ToStringOrdered());
        }

        public static void RunBFSPOP(IPlan initialPlan)
        {
            var BFSPOP = new PlanSpacePlanner(initialPlan, SearchType.BFS, new ZeroHeuristic().Heuristic, true);
            var BFSSolutions = BFSPOP.Solve(1, 6000f);
            Console.WriteLine(BFSSolutions[0].ToStringOrdered());
        }

        public static void RunDFSPop(IPlan initialPlan)
        {
            var DFSPOP = new PlanSpacePlanner(initialPlan, SearchType.DFS, new ZeroHeuristic().Heuristic, true);
            var DFSSolutions = DFSPOP.Solve(1, 6000f);
            Console.WriteLine(DFSSolutions[0].ToStringOrdered());
        }

        public static IPlan CreateInitialPlan()
        {
            Console.WriteLine("Creating initial Plan");

            // Create Initial Plan
            var initialPlan = new Plan(new State(testProblem.Initial) as IState, new State(testProblem.Goal) as IState);
            foreach (var goal in testProblem.Goal)
            {
                initialPlan.Flaws.Insert(initialPlan, new OpenCondition(goal, initialPlan.GoalStep as IPlanStep));
            }
            Console.WriteLine(initialPlan);

            Console.WriteLine("Insert First Ordering");
            initialPlan.Orderings.Insert(initialPlan.InitialStep, initialPlan.GoalStep);
            return initialPlan;
        }

        static void Main(string[] args)
        {
            Console.Write("hello world\n");
            FreezeProblem(false, true);
            Console.WriteLine("\nFinishedUNLoading!\n");
            var initPlan = CreateInitialPlan();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            RunAddReusePop(initPlan);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.Write(elapsedMs);

            Console.WriteLine(Console.Read());
            
        }
    }
}
