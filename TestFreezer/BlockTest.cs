using BoltFreezer.CacheTools;
using BoltFreezer.DecompTools;
using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestFreezer
{
    public static class BlockTest
    {

        public static ProblemFreezer ReadDomainAndProblem(bool serializeIt, int whichProblem)
        {
            var domainName = "blocks";
            var domainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl";
            var domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.PlanSpace);
            var problem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\prob0" + whichProblem.ToString() + @".pddl");

            var PF = new ProblemFreezer(domainName, domainDirectory, domain, problem);
            if (serializeIt)
                PF.Serialize();
            else
                PF.Deserialize();
            return PF;
        }

        public static List<Problem> RandomProblemGenerator(int numProblems, BoltFreezer.Utilities.Tuple<int, int> blockRange, int agentMax, int maxGoals, string domainName)
        {
            List<Problem> generatedProblems = new List<Problem>();

            Random r = new Random();

            for (int i =0; i < numProblems; i++)
            {

                var blocks = r.Next(blockRange.First, blockRange.Second+1);
                var agents = r.Next(1, agentMax + 1);
                var locs = r.Next(blocks + agents, blocks+agents + blockRange.Second+1);
                var goals = r.Next(1, Math.Min(blocks+1, maxGoals + 1));

                // Create a problem with this specification. 
                
                List<IObject> problemObjects = new List<IObject>();
                List<IPredicate> initialPreds = new List<IPredicate>();
                List<IPredicate> goalPreds = new List<IPredicate>();

                // First, create locations. Each new location is randomly adjacent to between 1 and 4
                var assignableLocations = new List<Obj>();
                for (int j = 0; j < locs; j++)
                {
                    var newLoc = new Obj(string.Format("location_{0}", j), "location");
                    assignableLocations.Add(newLoc);
                    if (j > 0)
                    {
                        // pick a value that's between 1 and 3 (or however many locations have been created
                        var numAdjacentsToCreate = Math.Min(r.Next(1, 4), problemObjects.Count());
                        var result = problemObjects.PickRandom(numAdjacentsToCreate);
                        foreach (var loc in result)
                        {
                            var newAdjacent = new Predicate("adjacent", new List<ITerm>() { new Term(loc.Name, true) as ITerm, new Term(newLoc.Name, true) as ITerm }, true) as IPredicate;
                            initialPreds.Add(newAdjacent);
                            var newAdjacent2 = new Predicate("adjacent", new List<ITerm>() { new Term(newLoc.Name, true) as ITerm, new Term(loc.Name, true) as ITerm }, true) as IPredicate;
                            initialPreds.Add(newAdjacent2);
                        }
                    }

                    problemObjects.Add(newLoc as IObject);

                }

                // pick location of blocks and agents
                var blockLocations = problemObjects.PickRandom(blocks).ToList();
                var agentLocations = problemObjects.Where(loc => !blockLocations.Contains(loc)).PickRandom(agents).ToList();

                // Create Blocks
                var assignableBlocks = new List<Obj>();
                for (int j =0; j< blocks; j++)
                {
                    // blockLocation
                    var bl = blockLocations[j];

                    // create a new block
                    var newBlock = new Obj(string.Format("block_{0}", j), "block");

                    initialPreds.Add(new Predicate("at", new List<ITerm>() { new Term(newBlock.Name, true) as ITerm, new Term(bl.Name, true) as ITerm }, true) as IPredicate);
                    initialPreds.Add(new Predicate("occupied", new List<ITerm>() { new Term(bl.Name, true) }, true) as IPredicate);

                    problemObjects.Add(newBlock as IObject);
                    assignableBlocks.Add(newBlock);
                }

                // Create Agents
                for(int j = 0; j < agents; j++)
                {
                    // agentLocation
                    var al = agentLocations[j];

                    // create new agent
                    var newAgent = new Obj(string.Format("agent_{0}", j), "steeringagent");

                    initialPreds.Add(new Predicate("at", new List<ITerm>() { new Term(newAgent.Name, true) as ITerm, new Term(al.Name, true) as ITerm }, true) as IPredicate);
                    initialPreds.Add(new Predicate("freehands", new List<ITerm>() { new Term(newAgent.Name, true) }, true) as IPredicate);
                    initialPreds.Add(new Predicate("occupied", new List<ITerm>() { new Term(al.Name, true) }, true) as IPredicate);

                    problemObjects.Add(newAgent as IObject);
                }

                // Assign Goals
                var alreadyAssignedLocations = new List<Obj>();
                for (int j = 0; j < goals; j++)
                {
                    // pick a block, just pick the j-th block
                    var blockToAssign = assignableBlocks[j];
                    var goalBlockPosition = assignableLocations.First(loc => !loc.Name.Equals(blockLocations[j].Name) && !alreadyAssignedLocations.Contains(loc));
                    goalPreds.Add(new Predicate("at", new List<ITerm>() { new Term(blockToAssign.Name, true) as ITerm, new Term(goalBlockPosition.Name, true) as ITerm }, true) as IPredicate);
                    alreadyAssignedLocations.Add(goalBlockPosition);
                }

                // instantiate problem
                var generatedProblem = new Problem(i.ToString(), i.ToString(), domainName, "", problemObjects, initialPreds, goalPreds);
                generatedProblems.Add(generatedProblem);

            }

            return generatedProblems;
        }


        public static Decomposition MultiMove()
        {
            // Params
            var objTerms = new List<ITerm>() {
                new Term("?agent")     { Type = "steeringagent"}, //0
                new Term("?from")       { Type = "location"}, //1
                new Term("?to")         { Type = "location"}, //2
                new Term("?intermediate") {Type = "location"} //3
            };

            var litTerms = new List<IPredicate>();

            var atAgentOrigin = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[1] }, true);
            var atAgentInt = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[3] }, true);
            var atAgentDest = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[2] }, true);

            var move1 = new PlanStep(new Operator("",
               new List<IPredicate>() { atAgentOrigin },
               new List<IPredicate> { atAgentInt }));

            var move2 = new PlanStep(new Operator("",
               new List<IPredicate>() { atAgentInt },
               new List<IPredicate> { atAgentDest }));

            var substeps = new List<IPlanStep>() { move1, move2 };

            var sublinks = new List<CausalLink<IPlanStep>>()
            {
                new CausalLink<IPlanStep>(atAgentInt, move1, move2)
            };

            var suborderings = new List<BoltFreezer.Utilities.Tuple<IPlanStep, IPlanStep>>()
            {
                new BoltFreezer.Utilities.Tuple<IPlanStep,IPlanStep>(move1, move2)
            };

            var root = new Operator(new Predicate("multimove", objTerms, true));
            var decomp = new Decomposition(root, litTerms, substeps, suborderings, sublinks)
            {
                NonEqualities = new List<List<ITerm>>() {
                        new List<ITerm>() { objTerms[1], objTerms[2] },
                        new List<ITerm>() { objTerms[2], objTerms[3] },
                        new List<ITerm>() { objTerms[1], objTerms[3] }
                }
            };

            return decomp;
        }

        public static Decomposition Transport()
        {

            // Params
            var objTerms = new List<ITerm>() {
                new Term("?agent")     { Type = "steeringagent"}, //0
                new Term("?item")        { Type = "block"}, //1
                new Term("?from")       { Type = "location"}, //2
                new Term("?to")         { Type = "location"}, //3
                new Term("?adjacentfrom") {Type = "location"}, //4
                new Term("?adjacentto") {Type = "location"} //5
            };

            var litTerms = new List<IPredicate>();
            // pickup (?taker - agent ?block - block ?location - location ?takerLocation - location)
            var pickupterms = new List<ITerm>() { objTerms[0], objTerms[1], objTerms[2], objTerms[4] };

            // This guaranteed move
            //var moveterms = new List<ITerm>() { objTerms[0], objTerms[4], objTerms[5] };

            //(?putter - agent ?thing - block ?agentlocation - location ?newlocation - location)
            var putdownterms = new List<ITerm>() { objTerms[0], objTerms[1], objTerms[5], objTerms[3] };

            var pickup = new PlanStep(new Operator(new Predicate("pickup", pickupterms, true)));
            // var travelOp = new Operator("", new List<IPredicate>(), new List<IPredicate>(){ atPersonTo});
           
            var putdown = new PlanStep(new Operator(new Predicate("putdown", putdownterms, true)));

            var atAgentOrigin = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[4] }, true);
            var hasAgentThing = new Predicate("has", new List<ITerm>() { objTerms[0], objTerms[1] }, true);
            var atAgentDest = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[5] }, true);

            var move = new PlanStep(new Operator("",
               new List<IPredicate>() { atAgentOrigin},
               new List<IPredicate> { atAgentDest}));

            //new Operator()
            var substeps = new List<IPlanStep>() { pickup, move, putdown };
            var sublinks = new List<CausalLink<IPlanStep>>()
            {
               // new CausalLink<IPlanStep>(atAgentOrigin, pickup, move),
                new CausalLink<IPlanStep>(hasAgentThing, pickup, putdown),
                new CausalLink<IPlanStep>(atAgentDest, move, putdown),
            };
            var suborderings = new List<BoltFreezer.Utilities.Tuple<IPlanStep, IPlanStep>>()
            {
                new BoltFreezer.Utilities.Tuple<IPlanStep,IPlanStep>(pickup, move),
                new BoltFreezer.Utilities.Tuple<IPlanStep,IPlanStep>(move, putdown)
            };

            var root = new Operator(new Predicate("transport", objTerms, true));
            var decomp = new Decomposition(root, litTerms, substeps, suborderings, sublinks)
            {
                NonEqualities = new List<List<ITerm>>() {
                        new List<ITerm>() { objTerms[2], objTerms[3] },
                        new List<ITerm>() { objTerms[2], objTerms[4] },
                        new List<ITerm>() { objTerms[3], objTerms[5] }
                }
            };

            return decomp;
        }

        public static Composite ReadMultimoveCompositeOperator()
        {
            var objTerms = new List<ITerm>() {
                new Term("?agent")     { Type = "steeringagent"},
                new Term("?from")         { Type = "location"},
                new Term("?to")         { Type = "location"},
            };

            var atAgentFrom = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[1] }, true);
            var atAgentTo = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[2] }, true);

            var op =
                new Operator(new Predicate("Multimove", objTerms, true),
                    new List<IPredicate>() { atAgentFrom },
                    new List<IPredicate>() { atAgentTo, atAgentFrom.GetReversed() }
                )
                {
                    NonEqualities = new List<List<ITerm>>() {
                        new List<ITerm>() { objTerms[1], objTerms[2] }
                    }
                };

            return new Composite(op);
        }

        public static Composite ReadTransportCompositeOperator()
        {
            var objTerms = new List<ITerm>() {
                new Term("?agent")     { Type = "steeringagent"}, //0
                new Term("?item")         { Type = "block"}, //1
                new Term("?adjacentfrom") {Type="location"}, //2
                new Term("?from")         { Type = "location"}, //3
                new Term("?to")         { Type = "location"} //4
            };

            var atItemFrom = new Predicate("at", new List<ITerm>() { objTerms[1], objTerms[3] }, true);
            var atAgentFrom = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[2] }, true);
            var atItemTo = new Predicate("at", new List<ITerm>() { objTerms[1], objTerms[4] }, true);
            var freeHands = new Predicate("freehands", new List<ITerm>() { objTerms[0] }, true);
            var occupied = new Predicate("occupied", new List<ITerm>() { objTerms[4] }, true);
            var adjacent = new Predicate("adjacent", new List<ITerm>() { objTerms[2], objTerms[3] }, true);

            var op =
                new Operator(new Predicate("Transport", objTerms, true),
                    new List<IPredicate>() { atItemFrom, atAgentFrom, adjacent, freeHands, occupied.GetReversed()},
                    new List<IPredicate>() { atItemTo }
                        //freeHands, occupied, atItemFrom.GetReversed()}
                )
                {
                    NonEqualities = new List<List<ITerm>>() {
                        new List<ITerm>() { objTerms[3], objTerms[4] },
                        new List<ITerm>(){objTerms[2], objTerms[3]},
                        new List<ITerm>(){objTerms[3], objTerms[4]}
                    }
                };

            return new Composite(op);
        }

        public static List<Decomposition> ReadTransportDecompositions()
        {
            var decomps = new List<Decomposition>();

            var transport = Transport();
            decomps.Add(transport);
            return decomps;
        }

        public static List<Decomposition> ReadMultimoveDecompositions()
        {
            var decomps = new List<Decomposition>();
            var multimove = MultiMove();
            decomps.Add(multimove);
            return decomps;
        }

        public static BoltFreezer.Utilities.Tuple<Composite, List<Decomposition>> TransportComposites()
        {
            var decomps =  ReadTransportDecompositions();
            var composite = ReadTransportCompositeOperator();
            var CompositeMethods = new BoltFreezer.Utilities.Tuple<Composite, List<Decomposition>>(composite, decomps);
            return CompositeMethods;
        }

        public static BoltFreezer.Utilities.Tuple<Composite, List<Decomposition>> MultimoveComposites()
        {
            var decomps = ReadMultimoveDecompositions();
            var composite = ReadMultimoveCompositeOperator();
            var CompositeMethods = new BoltFreezer.Utilities.Tuple<Composite, List<Decomposition>>(composite, decomps);
            return CompositeMethods;
        }

        public static Dictionary<Composite, List<Decomposition>> ReadCompositeOperators()
        {
            var compositeDecompList = new Dictionary<Composite, List<Decomposition>>();
            var transport = TransportComposites();
            compositeDecompList[transport.First] = transport.Second;//.Add(transport);
            var multimove = MultimoveComposites();
            compositeDecompList[multimove.First] = multimove.Second;
            return compositeDecompList;
        }

        public static void WriteProblemToFile(Problem problem, string directory)
        {
            var file = directory + "_" + problem.Name + ".txt";
            
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.Write(problem.ToString());
            }
        }

        public static IPredicate ParenthesisStringToPredicate(string stringItem)
        {
            var splitInput = stringItem.Split(' ');
            var predName = splitInput[0].TrimStart('(');
            var terms = new List<ITerm>();
            foreach (string item in splitInput.Skip(1))
            {
                var cleanedItem = item.TrimEnd(')');
                var newTerm = new Term(cleanedItem, true) as ITerm;
                terms.Add(newTerm);
            }
            var predic = new Predicate(predName, terms, true) as IPredicate;
            return predic;
        }

        public static Problem ReadStringGeneratedProblem(string file, int problemNumber)
        {
            string[] input = System.IO.File.ReadAllLines(file);

            List<IObject> problemObjects = new List<IObject>();
            List<IPredicate> initialPreds = new List<IPredicate>();
            List<IPredicate> goalPreds = new List<IPredicate>();

            // objects, then initial state, then goal state
            int i = 3;
            bool onObjects = true;
            bool onInit = false;
            while (true)
            {
                if (onObjects)
                {

                    var objType = input[i].Split('_').First();
                    if (objType.Equals("agent"))
                    {
                        objType = "steeringagent";
                    }
                    var newObject = new Obj(input[i], objType) as IObject;
                    problemObjects.Add(newObject);
                    i++;
                    if (input[i].Equals("") || input[i].Equals("\n"))
                    {
                        onObjects = false;
                        onInit = true;
                        i = i + 2;
                    }
                }

                if (onInit)
                {

                    var newInit = ParenthesisStringToPredicate(input[i]);
                    initialPreds.Add(newInit);
                    i++;

                    if (input[i].Equals("") || input[i].Equals("\n"))
                    {
                        onInit = false;
                        i = i + 2;
                    }
                }

                if (!onInit && !onObjects)
                {
                    var pred = ParenthesisStringToPredicate(input[i]);
                    goalPreds.Add(pred);
                    i++;
                }

                if (i >= input.Count())
                {
                    break;
                }
                
            }

            // create new Problem
            var prob =  new Problem(problemNumber.ToString(), problemNumber.ToString(), "blocks", "", problemObjects, initialPreds, goalPreds);
            return prob;
        }


        public static void RunProblem(string directory, string domainName, string domainDirectory, Domain domain, Problem problem, float cutoff, int HTN_level, Dictionary<Composite, List<Decomposition>> CompositeMethods)
        {
            // Reset Cached Items
            GroundActionFactory.Reset();
            CacheMaps.Reset();

            var PF = new ProblemFreezer(domainName, domainDirectory, domain, problem);
            PF.Serialize();

            Console.WriteLine("Detecting Statics");
            GroundActionFactory.DetectStatics();

            var initPlan = PlanSpacePlanner.CreateInitialPlan(PF);

            // Removing irrelevant actions
            Console.WriteLine("Removing Irrelevant Actions");
            var staticInitial = initPlan.Initial.Predicates.Where(state => GroundActionFactory.Statics.Contains(state));

            // Every action that has No preconditions which are both static and not in staticInitial
            var possibleActions = GroundActionFactory.GroundActions.Where(action => !action.Preconditions.Any(pre => GroundActionFactory.Statics.Contains(pre) && !staticInitial.Contains(pre)));
            GroundActionFactory.GroundActions = possibleActions.ToList();
            GroundActionFactory.GroundLibrary = possibleActions.ToDictionary(item => item.ID, item => item);

            // Composing HTNs
            Console.WriteLine("Composing HTNs");
            Composite.ComposeHTNs(HTN_level, CompositeMethods);

            // Caching Causal Maps
            Console.WriteLine("Caching Causal Maps");
            CacheMaps.Reset();
            CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
            CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, initPlan.Goal.Predicates);

            // Cache Heuristic Costs (dynamic programming)
            Console.WriteLine("Caching Heuristic Costs");
            CacheMaps.CacheAddReuseHeuristic(initPlan.Initial);

            // Redo to gaurantee accuracy (needs refactoring)
            initPlan = PlanSpacePlanner.CreateInitialPlan(PF);

            var probNum = Int32.Parse(problem.Name);
           

            Console.WriteLine(String.Format("Running Problem {0}", probNum));

            RunPlanner(initPlan.Clone() as IPlan, new ADstar(false), new E0(new AddReuseHeuristic(), true), cutoff, directory, probNum);

            //RunPlanner(initPlan.Clone() as IPlan, new ADstar(true), new E0(new AddReuseHeuristic()), cutoff, directory, probNum);
            //RunPlanner(initPlan.Clone() as IPlan, new ADstar(true), new E1(new AddReuseHeuristic()), cutoff, directory, probNum);
            //RunPlanner(initPlan.Clone() as IPlan, new ADstar(true), new E2(new AddReuseHeuristic()), cutoff, directory, probNum);
            //RunPlanner(initPlan.Clone() as IPlan, new ADstar(true), new E3(new AddReuseHeuristic()), cutoff, directory, probNum);
            //RunPlanner(initPlan.Clone() as IPlan, new BFS(false), new Nada(new ZeroHeuristic()), cutoff, directory, probNum);

            // RunPlanner(initPlan.Clone() as IPlan, new BFS(true), new Nada(new ZeroHeuristic()), cutoff, directory, probNum);
        }

        public static void ReadGeneratedAndTest(int numProblems, string directory, float cutoff, int HTN_level)
        {
            System.IO.Directory.CreateDirectory(directory);
            System.IO.Directory.CreateDirectory(directory + @"\Problems\");
            Parser.path = @"D:\documents\frostbow\boltfreezer\";
            List<IPlan> initialPlans = new List<IPlan>();

            var domainName = "blocks";
            var domainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl";
            var domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.PlanSpace);
            var CompositeMethods = ReadCompositeOperators();

            for (int i =0; i < numProblems; i++)
            {
                var prob = ReadStringGeneratedProblem(directory + string.Format(@"\Problems\_{0}.txt", i), i);

                RunProblem(directory, domainName, domainDirectory, domain, prob, cutoff, HTN_level, CompositeMethods);
            }
        }

        public static void GenerateAndTest(int numProblems, string directory, float cutoff, int HTN_level)
        {
            System.IO.Directory.CreateDirectory(directory);
            System.IO.Directory.CreateDirectory(directory + @"\Problems\");
            Parser.path = @"D:\documents\frostbow\boltfreezer\";
            List<IPlan> initialPlans = new List<IPlan>();

            var domainName = "blocks";
            var domainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl";
            var domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.PlanSpace);
            //var CompositeMethods = ReadCompositeOperators();

            List<Problem> problems = RandomProblemGenerator(numProblems, new BoltFreezer.Utilities.Tuple<int, int>(1, 4), 3, 2, domainName);

            foreach(var problem in problems)
            {
                WriteProblemToFile(problem, directory + @"\Problems\");
                // Reset Cached Items
                // GroundActionFactory.Reset();
                // CacheMaps.Reset();

                // var PF = new ProblemFreezer(domainName, domainDirectory, domain, problem);
                // PF.Serialize();

                // Console.WriteLine("Detecting Statics");
                // GroundActionFactory.DetectStatics(CacheMaps.CausalMap, CacheMaps.ThreatMap);

                // var initPlan = PlanSpacePlanner.CreateInitialPlan(PF);

                // // Removing irrelevant actions
                // Console.WriteLine("Removing Irrelevant Actions");
                // var staticInitial = initPlan.Initial.Predicates.Where(state => GroundActionFactory.Statics.Contains(state));

                // // Every action that has No preconditions which are both static and not in staticInitial
                // var possibleActions = GroundActionFactory.GroundActions.Where(action => !action.Preconditions.Any(pre => GroundActionFactory.Statics.Contains(pre) && !staticInitial.Contains(pre)));
                // GroundActionFactory.GroundActions = possibleActions.ToList();
                // GroundActionFactory.GroundLibrary = possibleActions.ToDictionary(item => item.ID, item => item);

                // // Composing HTNs
                // Console.WriteLine("Composing HTNs");
                // Composite.ComposeHTNs(HTN_level, CompositeMethods);

                // // Caching Causal Maps
                // Console.WriteLine("Caching Causal Maps");
                // CacheMaps.Reset();
                // CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
                // CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, initPlan.Goal.Predicates);

                // // Cache Heuristic Costs (dynamic programming)
                // CacheMaps.CacheAddReuseHeuristic(initPlan.Initial);

                // // Redo to gaurantee accuracy (needs refactoring)
                // initPlan = PlanSpacePlanner.CreateInitialPlan(PF);

                // var probNum = Int32.Parse(problem.Name);
                //// RunPlanner(initPlan.Clone() as IPlan, new ADstar(false), new E0(new AddReuseHeuristic(), true), cutoff, directory, probNum);

                // RunPlanner(initPlan.Clone() as IPlan, new ADstar(true), new E0(new AddReuseHeuristic()), cutoff, directory, probNum);
                // RunPlanner(initPlan.Clone() as IPlan, new ADstar(true), new E1(new AddReuseHeuristic()), cutoff, directory, probNum);
                // RunPlanner(initPlan.Clone() as IPlan, new ADstar(true), new E2(new AddReuseHeuristic()), cutoff, directory, probNum);
                // RunPlanner(initPlan.Clone() as IPlan, new ADstar(true), new E3(new AddReuseHeuristic()), cutoff, directory, probNum);
                // RunPlanner(initPlan.Clone() as IPlan, new BFS(false), new Nada(new ZeroHeuristic()), cutoff, directory, probNum);

                //// RunPlanner(initPlan.Clone() as IPlan, new BFS(true), new Nada(new ZeroHeuristic()), cutoff, directory, probNum);


                // Add to List
                //  initialPlans.Add(initPlan);//
            }

            //  return initialPlans;
        }

        public static void RunPlanner(IPlan initPi, ISearch SearchMethod, ISelection SelectMethod, float cutoff, string directoryToSaveTo, int problem)
        {
            var POP = new PlanSpacePlanner(initPi, SelectMethod, SearchMethod, true)
            {
                directory = directoryToSaveTo,
                problemNumber = problem,
            };
            var Solutions = POP.Solve(1, cutoff);
            if (Solutions != null)
            {
                Console.WriteLine(Solutions[0].ToStringOrdered());
            }

        }


        public static IPlan ReadAndCompile(bool serializeIt, int whichProblem)
        {
            Parser.path = @"D:\documents\frostbow\boltfreezer\";

            GroundActionFactory.Reset();
            CacheMaps.Reset();

            // Reads Domain and problem, also populates ground actions and caches causal maps
            var pfreeze = ReadDomainAndProblem(serializeIt, whichProblem);

            // Detecting static conditions
            Console.WriteLine("Detecting Statics");
            GroundActionFactory.DetectStatics();

            var initPlan = PlanSpacePlanner.CreateInitialPlan(pfreeze);

            // Removing irrelevant actions
            Console.WriteLine("Removing Irrelevant Actions");
            var staticInitial = initPlan.Initial.Predicates.Where(state => GroundActionFactory.Statics.Contains(state));

            // Every action that has No preconditions which are both static and not in staticInitial
            var possibleActions = GroundActionFactory.GroundActions.Where(action => !action.Preconditions.Any(pre => GroundActionFactory.Statics.Contains(pre) && !staticInitial.Contains(pre)));
            GroundActionFactory.GroundActions = possibleActions.ToList();
            GroundActionFactory.GroundLibrary = possibleActions.ToDictionary(item => item.ID, item => item);

            var CompositeMethods = ReadCompositeOperators();

            // Composing HTNs
            Console.WriteLine("Composing HTNs");
            Composite.ComposeHTNs(2, CompositeMethods);

            // Caching Causal Maps
            Console.WriteLine("Caching Causal Maps");
            CacheMaps.Reset();
            CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
            CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, initPlan.Goal.Predicates);
            CacheMaps.CacheAddReuseHeuristic(initPlan.Initial);
            initPlan = PlanSpacePlanner.CreateInitialPlan(pfreeze);


            return initPlan;
        }

    }
}
