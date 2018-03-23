using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;
//using SimpleJSON;

namespace TestFreezer
{
    class TestAll
    {


        //public static string testDomainName = "batman";
        //public static string testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
        //public static Domain testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
        //public static Problem testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");


        //public static void FreezeProblem(bool serialize, bool deserialize)
        //{
        //    string FileName = Parser.GetTopDirectory() + @"Cached\CachedOperators\" + testDomainName + "_" + testProblem.Name;
        //    string CausalMapFileName = Parser.GetTopDirectory() + @"Cached\CausalMaps\" + testDomainName + "_" + testProblem.Name;
        //    string ThreatMapFileName = Parser.GetTopDirectory() + @"Cached\ThreatMaps\" + testDomainName + "_" + testProblem.Name;

        //    if (serialize)
        //    {
        //        Console.Write("Creating Ground Operators");
        //        GroundActionFactory.PopulateGroundActions(testDomain.Operators, testProblem);
        //        //BinarySerializer.SerializeObject(FileName, GroundActionFactory.GroundActions);
        //        foreach (var op in GroundActionFactory.GroundActions)
        //        {
        //            BinarySerializer.SerializeObject(FileName + op.GetHashCode().ToString() + ".CachedOperator", op);
        //        }

        //        CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
        //        CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, testProblem.Goal);
        //        BinarySerializer.SerializeObject(CausalMapFileName + ".CachedCausalMap", CacheMaps.CausalMap);
        //        BinarySerializer.SerializeObject(ThreatMapFileName + ".CachedThreatMap", CacheMaps.ThreatMap);
        //    }
        //    else if(deserialize)
        //    {
        //        GroundActionFactory.GroundActions = new List<IOperator>();
        //        GroundActionFactory.GroundLibrary = new Dictionary<int, IOperator>();
        //        foreach (var file in Directory.GetFiles(Parser.GetTopDirectory() + @"Cached\CachedOperators\", testDomainName + "_" + testProblem.Name + "*.CachedOperator"))
        //        {
        //            var op = BinarySerializer.DeSerializeObject<IOperator>(file);
        //            GroundActionFactory.GroundActions.Add(op);
        //            GroundActionFactory.GroundLibrary[op.ID] = op;
        //        }
        //        // THIS is so that initial and goal steps created don't get matched with these
        //        Operator.SetCounterExternally(GroundActionFactory.GroundActions.Count + 1);

        //        Console.WriteLine("\nCmap\n");

        //        var cmap = BinarySerializer.DeSerializeObject<Dictionary<IPredicate, List<int>>>(CausalMapFileName + ".CachedCausalMap");
        //        CacheMaps.CausalMap = cmap;

        //        Console.WriteLine("\nTmap\n");
        //        var tcmap = BinarySerializer.DeSerializeObject<Dictionary<IPredicate, List<int>>>(ThreatMapFileName + ".CachedThreatMap");
        //        CacheMaps.ThreatMap = tcmap;

        //        Console.WriteLine("Finding Statics");
        //        GroundActionFactory.DetectStatics(CacheMaps.CausalMap, CacheMaps.ThreatMap);
        //        foreach (var stat in GroundActionFactory.Statics)
        //        {
        //            Console.WriteLine(stat);
        //        }
        //    }
        //}

        public static List<ITerm> JsonTermListtoTerms(JsonArray jsontermlist)
        {
            var Terms = new List<ITerm>();

            for (int i = 0; i < jsontermlist.Count; i++)
            {
                var jsonterm = jsontermlist[i] as JsonObject;
                var name = jsonterm["Name"].ToString();
                var _type = jsonterm["Types"].ToString();
                var term = new Term(i.ToString(), name, _type);
                Terms.Add(term);
            }

            return Terms;
        }
        
        public static Predicate JsonPreconToPrecon(JsonObject jsonPrecon)
        {
            var Name = jsonPrecon["name"].ToString();
            var Terms = JsonTermListtoTerms(jsonPrecon["Terms"] as JsonArray);
            var sign = jsonPrecon["Sign"].ToString();
            bool Sign = false;
            if (sign.Equals("True"))
            {
                Sign = true;
            }

            var newPredicate = new Predicate(Name, Terms, Sign);
            if (jsonPrecon["Static"].ToString().Equals("True"))
            {
                GroundActionFactory.Statics.Add(newPredicate);
            }

            return newPredicate;
        }

        public static List<IPredicate> JsonPreconditionsToPreconditions(JsonArray jsonPreconditions)
        {
            var Preconditions = new List<IPredicate>();
            foreach (JsonObject p in jsonPreconditions)
            {
                var precondition = JsonPreconToPrecon(p);
                Preconditions.Add(precondition);
            }
            return Preconditions;
        }

        public static Predicate StringToPredicate(string formattedString)
        {
            bool Sign = true;
            string predName = formattedString.Split('[')[0];
            var possibleSign = formattedString.Split('-')[0];
            var afterBracket = formattedString.Split('[')[1];
            if (possibleSign.Equals("not"))
            {
                var afterHyphen = formattedString.Split('-')[1];
                Sign = false;
                predName = afterHyphen.Split('[')[0];
                afterBracket = afterHyphen.Split('[')[1];
            }
            var betweenBrackets = afterBracket.Split(']')[0];
            var indArgs = betweenBrackets.Split(',') as string [];

            var termList = new List<ITerm>();
            foreach(var arg in indArgs)
            {
                termList.Add(new Term(arg.Trim(' ').Trim('\''), true));
            }

            return new Predicate(predName, termList, Sign);
        }

        public static Plan DeserializeJsonTravelDomain(int whichOne)
        {
            //var problemFile = @"D:\Unity projects\BoltFreezer\travel_domain.travel\travel_domain.travel\1\1.json";
            var problemFile = @"D:\Documents\workspace\travel_domain.travel\" + whichOne.ToString() + @"\" + whichOne.ToString() + @".json";
            //var problemFile = @"D:\Unity projects\BoltFreezer\travel_domain.travel\travel_domain.travel\" + whichOne.ToString() + @"\" + whichOne.ToString() + @".json";
            //problemFile

            var travelProblem = Parser.GetProblem(@"D:\Documents\workspace\travel_domain.travel\" + whichOne.ToString() + @"\travel-" + whichOne.ToString() + @".pddl");

            var problemText = System.IO.File.ReadAllText(problemFile);

            var jsonArray = SimpleJson.DeserializeObject(problemText) as JsonArray;
            Console.WriteLine("CHERE");

            GroundActionFactory.GroundActions = new List<IOperator>();
            GroundActionFactory.GroundLibrary = new Dictionary<int, IOperator>();
            CacheMaps.CausalMap = new Dictionary<IPredicate, List<int>>();
            CacheMaps.ThreatMap = new Dictionary<IPredicate, List<int>>();

            Operator initialOp = new Operator();
            Operator goalOp = new Operator();
            // for each operator in the list
            foreach (JsonObject jsonObject in jsonArray)
            {
                // ID, Name, Terms, Preconditions, Effects
                var ID = jsonObject["ID"];
                var Name = jsonObject["Name"].ToString();
                var Terms = JsonTermListtoTerms(jsonObject["Terms"] as JsonArray);
                var Preconditions = JsonPreconditionsToPreconditions(jsonObject["Preconditions"] as JsonArray) as List<IPredicate>;
                var Effects = new List<IPredicate>();
                var Height = int.Parse(jsonObject["height"].ToString());
                if (Name.Equals("dummy_goal"))
                {
                    Name = "goal";
                }
                if (Name.Equals("dummy_init"))
                {
                    Name = "initial";
                    Effects = travelProblem.Initial;
                    Preconditions = new List<IPredicate>();
                }
                var Action = new Operator(Name, Terms, new Hashtable(), Preconditions, Effects, int.Parse(ID.ToString()))
                {
                    Height = Height
                };

                if (Name.Equals("goal"))
                    goalOp = Action;
                else if (Name.Equals("initial"))
                    initialOp = Action;
                else {
                    GroundActionFactory.GroundActions.Add(Action);
                    GroundActionFactory.GroundLibrary[Action.ID] = Action;
                }

                if (jsonObject.ContainsKey("CausalMap"))
                {
                    var CausalMap = jsonObject["CausalMap"] as JsonObject;
                    foreach(var keyvalue in CausalMap)
                    {
                        var predKey = StringToPredicate(keyvalue.Key);
                        if (!CacheMaps.CausalMap.ContainsKey(predKey))
                        {
                            var intList = new List<object>();
                            var jsonList = keyvalue.Value as JsonArray;
                            var enumItems = from item in jsonList select int.Parse(item.ToString());
                            CacheMaps.CausalMap[predKey] = enumItems.ToList() as List<int>;
                        }
                        
                    }
                }
                if (jsonObject.ContainsKey("ThreatMap"))
                {
                    var ThreatMap = jsonObject["ThreatMap"] as JsonObject;
                    foreach (var keyvalue in ThreatMap)
                    {
                        var predKey = StringToPredicate(keyvalue.Key);
                        if (!CacheMaps.ThreatMap.ContainsKey(predKey))
                        {
                            var intList = new List<object>();
                            var jsonList = keyvalue.Value as JsonArray;
                            var enumItems = from item in jsonList select int.Parse(item.ToString());
                        }
                    }
                }
                                
            }

            GroundActionFactory.GroundLibrary[initialOp.ID] = null;
            GroundActionFactory.GroundLibrary[goalOp.ID] = null;

            Operator.SetCounterExternally(GroundActionFactory.GroundActions.Count + 1);

            var plan = new Plan(initialOp, goalOp);
            foreach (var goal in plan.Goal.Predicates)
            {
                plan.Flaws.Insert(plan, new OpenCondition(goal, plan.GoalStep as IPlanStep));
            }

            Console.WriteLine("Insert First Ordering");
            plan.Orderings.Insert(plan.InitialStep, plan.GoalStep);
            return plan;
        }


        public static void RunAddReusePop(IPlan initialPlan, string directoryToSaveTo, int problem)
        {
            Console.WriteLine("First POP");
            var AStarPOP = new PlanSpacePlanner(initialPlan, SearchType.BestFirst, new AddReuseHeuristic().Heuristic, true)
            {
                directory = directoryToSaveTo,
                problemNumber = problem,
                heuristicType = HeuristicType.AddReuseHeuristic
            };
            var bestFirstSolutions = AStarPOP.Solve(1, 14400f);
            Console.WriteLine(bestFirstSolutions[0].ToStringOrdered());
        }

        public static void RunNumOCsPOP(IPlan initialPlan, string directoryToSaveTo, int problem)
        {
            Console.WriteLine("First POP");
            var AStarPOP = new PlanSpacePlanner(initialPlan, SearchType.BestFirst, new NumOpenConditionsHeuristic().Heuristic, true)
            {
                directory = directoryToSaveTo,
                problemNumber = problem,
                heuristicType = HeuristicType.NumOCsHeuristic
            };
            var bestFirstSolutions = AStarPOP.Solve(1, 14400f);
            Console.WriteLine(bestFirstSolutions[0].ToStringOrdered());
        }

        public static void RunBestFirstZeroPOP(IPlan initialPlan, string directoryToSaveTo, int problem)
        {
            Console.WriteLine("First POP");
            var AStarPOP = new PlanSpacePlanner(initialPlan, SearchType.BestFirst, new ZeroHeuristic().Heuristic, true)
            {
                directory = directoryToSaveTo,
                problemNumber = problem,
                heuristicType = HeuristicType.ZeroHeuristic
            };
            var bestFirstSolutions = AStarPOP.Solve(1, 14400f);
            Console.WriteLine(bestFirstSolutions[0].ToStringOrdered());
        }

        public static void RunBFSPOP(IPlan initialPlan, string directoryToSaveTo, int problem)
        {
            var BFSPOP = new PlanSpacePlanner(initialPlan, SearchType.BFS, new ZeroHeuristic().Heuristic, true)
            {
                directory = directoryToSaveTo,
                problemNumber = problem,
                heuristicType = HeuristicType.ZeroHeuristic
            };
            var BFSSolutions = BFSPOP.Solve(1, 14400f);
            Console.WriteLine(BFSSolutions[0].ToStringOrdered());
        }

        public static void RunDFSPop(IPlan initialPlan, string directoryToSaveTo, int problem)
        {
            var DFSPOP = new PlanSpacePlanner(initialPlan, SearchType.DFS, new ZeroHeuristic().Heuristic, true)
            {
                directory = directoryToSaveTo,
                problemNumber = problem,
                heuristicType = HeuristicType.ZeroHeuristic
            };
            var DFSSolutions = DFSPOP.Solve(1, 14400f);
            Console.WriteLine(DFSSolutions[0].ToStringOrdered());
        }

        //public static IPlan CreateInitialPlan()
        //{
        //    Console.WriteLine("Creating initial Plan");

        //    // Create Initial Plan
        //    var initialPlan = new Plan(new State(testProblem.Initial) as IState, new State(testProblem.Goal) as IState);
        //    foreach (var goal in testProblem.Goal)
        //    {
        //        initialPlan.Flaws.Insert(initialPlan, new OpenCondition(goal, initialPlan.GoalStep as IPlanStep));
        //    }
        //    Console.WriteLine(initialPlan);

        //    Console.WriteLine("Insert First Ordering");
        //    initialPlan.Orderings.Insert(initialPlan.InitialStep, initialPlan.GoalStep);
        //    return initialPlan;
        //}

        static void Main(string[] args)
        {

            Console.Write("hello world\n");

            for (int i = 1; i < 9; i++)
            {
                var initPlan = DeserializeJsonTravelDomain(i);
                RunAddReusePop(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
                RunNumOCsPOP(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
                RunBestFirstZeroPOP(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
                RunBFSPOP(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
                RunDFSPop(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
            }

            //FreezeProblem(false, true);
            //Console.WriteLine("\nFinishedUNLoading!\n");
            //var initPlan = CreateInitialPlan();

            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //RunAddReusePop(initPlan);
            //watch.Stop();
            //var elapsedMs = watch.ElapsedMilliseconds;
            //Console.Write(elapsedMs);

            //Console.WriteLine(Console.Read());

        }
    }
}
