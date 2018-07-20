using BoltFreezer.CacheTools;
using BoltFreezer.Camera;
using BoltFreezer.DecompTools;
using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;
using BoltFreezer.Scheduling;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestFreezer
{
    public class CinemICEtest
    {
        public CinemICEtest(string problemName)
        {
            problemname = problemName;
        }

        public string problemname;
        public List<IPredicate> initial;
        public List<IPredicate> goal;

        public static List<IPredicate> AddObservedNegativeConditions(List<IPredicate> initialPredicates)
        {
            foreach (var ga in GroundActionFactory.GroundActions)
            {
                foreach (var precon in ga.Preconditions)
                {
                    // if the precon is signed positive, ignore
                    if (precon.Sign)
                    {
                        continue;
                    }
                    // if initially the precondition reveresed is true, ignore
                    if (initialPredicates.Contains(precon.GetReversed()))
                    {
                        continue;
                    }

                    // then this precondition is negative and its positive correlate isn't in the initial state
                    var obsPred = new Predicate("obs", new List<ITerm>() { precon as ITerm }, true);

                    if (initialPredicates.Contains(obsPred as IPredicate))
                    {
                        continue;
                    }

                    initialPredicates.Add(obsPred as IPredicate);
                }
            }
            return initialPredicates;
        }

        public void CachePlan(PlanSchedule plan)
        {
            Parser.path = @"D:\documents\frostbow\";
            var savePath = Parser.GetTopDirectory() + @"Results\" + problemname + @"\Solutions\";
            Directory.CreateDirectory(savePath);
            var planStepList = new List<IPlanStep>();
            var mergeMap = plan.MM.ToRootMap();
            foreach(var step in plan.Orderings.TopoSort(plan.InitialStep))
            {
                if (step is CamPlanStep cps)
                {
                    foreach (var seg in cps.TargetDetails.ActionSegs)
                    {
                        if (mergeMap.ContainsKey(seg.ActionID))
                        {
                            seg.ActionID = mergeMap[seg.ActionID];
                        }
                    }
                }
                planStepList.Add(step);
            }
            BinarySerializer.SerializeObject(savePath + "PlanSteps", planStepList);

            //BinarySerializer.SerializeObject(savePath + "Merges")
        }

        public void DecacheSteps()
        {
            Parser.path = @"D:\documents\frostbow\";
            var FileName = GetFileName();
            GroundActionFactory.GroundActions = new List<IOperator>();
            GroundActionFactory.GroundLibrary = new Dictionary<int, IOperator>();
            int maxSeen = 0;
            int maxStepSeen = 0;
            foreach (var file in Directory.GetFiles(Parser.GetTopDirectory() + @"Cached\CachedOperators\UnityBlocksWorld\", problemname + "*.CachedOperator"))
            {
                var op = BinarySerializer.DeSerializeObject<IOperator>(file);
                GroundActionFactory.GroundActions.Add(op);
                GroundActionFactory.GroundLibrary[op.ID] = op;
                if (op.ID > maxSeen)
                {
                    maxSeen = op.ID;
                }
                if (op is IComposite comp)
                {
                    foreach (var sub in comp.SubSteps)
                    {
                        if (sub.ID > maxStepSeen)
                        {
                            maxStepSeen = sub.ID;
                        }
                    }
                    if (comp.GoalStep.ID > maxStepSeen)
                    {
                        maxStepSeen = comp.GoalStep.ID;
                    }
                }
            }
            // THIS is so that initial and goal steps created don't get matched with these
            Operator.SetCounterExternally(maxSeen + 1);
            PlanStep.SetCounterExternally(maxStepSeen + 1);
        }



        public List<IPredicate> CreateInitialStateTest1()
        {
            var newList = new List<IPredicate>();
            var terms = new List<ITerm>();
            terms.Add(new Term("L1", true) as ITerm); //O, L1
            terms.Add(new Term("L3", true) as ITerm); //1, L3
            terms.Add(new Term("L4", true) as ITerm); //2, L4
            terms.Add(new Term("L5", true) as ITerm); //3, L5

            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[0], terms[1] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[1], terms[0] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[1], terms[2] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[2], terms[1] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[1], terms[3] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[3], terms[1] }, true));

            var boid = new Term("Boid", true) as ITerm;
            var block1 = new Term("Block1", true) as ITerm;
            var block2 = new Term("Block2", true) as ITerm;
            newList.Add(new Predicate("at", new List<ITerm>() { block1, terms[3] }, true));
            newList.Add(new Predicate("at", new List<ITerm>() { block2, terms[1] }, true));
            newList.Add(new Predicate("at", new List<ITerm>() { boid, terms[0] }, true));
            newList.Add(new Predicate("occupied", new List<ITerm>() { terms[3] }, true));
            newList.Add(new Predicate("occupied", new List<ITerm>() { terms[1] }, true));
            newList.Add(new Predicate("occupied", new List<ITerm>() { terms[0] }, true));
            newList.Add(new Predicate("freehands", new List<ITerm>() { boid }, true));

            return newList;
        }

        public List<IPredicate> CreateInitialStateRace()
        {
            var newList = new List<IPredicate>();
            var terms = new List<ITerm>();
            terms.Add(new Term("L0", true) as ITerm); //O, L0
            terms.Add(new Term("L1", true) as ITerm); //1, L1
            terms.Add(new Term("L2", true) as ITerm); //2, L2
            terms.Add(new Term("L3", true) as ITerm); //3, L3

            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[0], terms[1] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[1], terms[0] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[1], terms[2] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[2], terms[1] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[1], terms[3] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[3], terms[1] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[2], terms[3] }, true));
            newList.Add(new Predicate("adjacent", new List<ITerm>() { terms[3], terms[2] }, true));

            var ob = new Term("OrangeBoid", true) as ITerm;
            var bb = new Term("BlueBoid", true) as ITerm;
            newList.Add(new Predicate("at", new List<ITerm>() { ob, terms[3] }, true));
            newList.Add(new Predicate("at", new List<ITerm>() { bb, terms[0] }, true));
            newList.Add(new Predicate("occupied", new List<ITerm>() { terms[0] }, true));
            newList.Add(new Predicate("occupied", new List<ITerm>() { terms[3] }, true));

            return newList;
        }

        public List<IPredicate> CreateGoalState_Race1()
        {
            var bb = new Term("BlueBoid", true) as ITerm;
            var goalCondition = new Predicate("at", new List<ITerm>() { bb, new Term("L3", true) as ITerm }, true);
            //var goalCondition = new Predicate("at", new List<ITerm>() { boid, new Term("L3", true) as ITerm }, true);
            return new List<IPredicate>() { goalCondition };
        }

        public List<IPredicate> CreateGoalState_GetBlock1ToL1()
        {
            var boid = new Term("Boid", true) as ITerm;
            var block1 = new Term("Block1", true) as ITerm;
            var goalCondition = new Predicate("at", new List<ITerm>() { block1, new Term("L1", true) as ITerm }, true);
            //var goalCondition = new Predicate("at", new List<ITerm>() { boid, new Term("L3", true) as ITerm }, true);
            return new List<IPredicate>() { goalCondition };
        }

        public List<IPredicate> CreateGoalState_GetBlock2ToL1()
        {
            var boid = new Term("Boid", true) as ITerm;
            var block2 = new Term("Block2", true) as ITerm;
            var goalCondition = new Predicate("at", new List<ITerm>() { block2, new Term("L1", true) as ITerm }, true);
            //var goalCondition = new Predicate("at", new List<ITerm>() { boid, new Term("L3", true) as ITerm }, true);
            return new List<IPredicate>() { goalCondition };
        }

        public List<IPredicate> CreateGoalState_GetBlock2ToL4()
        {
            var boid = new Term("Boid", true) as ITerm;
            var block2 = new Term("Block2", true) as ITerm;
            var goalCondition = new Predicate("at", new List<ITerm>() { block2, new Term("L4", true) as ITerm }, true);
            //var goalCondition = new Predicate("at", new List<ITerm>() { boid, new Term("L3", true) as ITerm }, true);
            return new List<IPredicate>() { goalCondition };
        }

        public List<IPredicate> CreateGoalState_GetBlock1ToL4()
        {
            var boid = new Term("Boid", true) as ITerm;
            var block1 = new Term("Block1", true) as ITerm;
            var goalCondition = new Predicate("at", new List<ITerm>() { block1, new Term("L4", true) as ITerm }, true);
            //var goalCondition = new Predicate("at", new List<ITerm>() { boid, new Term("L3", true) as ITerm }, true);
            return new List<IPredicate>() { goalCondition };
        }

        public List<IPredicate> CreateGoalState_DuelState()
        {
            var bb = new Term("BlueBoid", true) as ITerm;
            var ob = new Term("OrangeBoid", true) as ITerm;
            var goalCondition = new Predicate("bel-about-to-duel", new List<ITerm>() { bb, ob }, true);
            return new List<IPredicate>() { goalCondition };
        }

        public List<IPredicate> MakeObservable(List<IPredicate> initialState)
        {
            var newState = new List<IPredicate>();
            foreach (var init in initialState)
            {
                newState.Add(init);
                var newObsTerm = new Predicate("obs", new List<ITerm>() { init as ITerm }, true) as IPredicate;
                newState.Add(newObsTerm);

            }
            return newState;
        }

        public void DeCacheIt()
        {
            Parser.path = @"D:\documents\frostbow\";
            DecacheSteps();

            // create initial State
            var initialState = CreateInitialStateRace();
           // initialState = MakeObservable(initialState);
           // initialState = AddObservedNegativeConditions(initialState);

            // create goal state
            //var goalState = CreateGoalState();
            //var goalState = CreateGoalState_GetBlock1ToL1();
            //var goalState = CreateGoalState_GetBlock2ToL1();
            //var goalState = CreateGoalState_GetBlock2ToL4();
            //var goalState = CreateGoalState_GetBlock1ToL4();
            //var goalState = CreateGoalState_Race1();
            var goalState = CreateGoalState_DuelState();
            //goalState = MakeObservable(goalState);

            var CausalMapFileName = GetCausalMapFileName();
            var ThreatMapFileName = GetThreatMapFileName();
            var EffortMapFileName = GetEffortDictFileName();

            CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
            CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, goalState);

            var iniTstate = new State(initialState) as IState;
            CacheMaps.CacheAddReuseHeuristic(iniTstate);
            CacheMaps.PrimaryEffectHack(iniTstate);


            initial = initialState;
            goal = goalState;

        }

        public string GetFileName()
        {
            return Parser.GetTopDirectory() + @"Cached\CachedOperators\UnityBlocksWorld\" + problemname;
        }

        public string GetCausalMapFileName()
        {
            return Parser.GetTopDirectory() + @"Cached\CausalMaps\UnityBlocksWorld\" + problemname;
        }

        public string GetThreatMapFileName()
        {
            return Parser.GetTopDirectory() + @"Cached\ThreatMaps\UnityBlocksWorld\" + problemname;
        }

        public string GetEffortDictFileName()
        {
            return Parser.GetTopDirectory() + @"Cached\EffortMaps\UnityBlocksWorld\" + problemname;
        }


        public IPlan Run(IPlan initPlan, ISearch SearchMethod, ISelection SelectMethod, float cutoff)
        {
            var directory = Parser.GetTopDirectory() + "/Results/";
            System.IO.Directory.CreateDirectory(directory);
            var POP = new PlannerScheduler(initPlan.Clone() as IPlan, SelectMethod, SearchMethod)
            {
                directory = directory,
                problemNumber = 0
            };
            //Debug.Log("Running plan-search");
            var Solutions = POP.Solve(1, cutoff);
            if (Solutions != null)
            {
                CachePlan(Solutions[0] as PlanSchedule);
                return Solutions[0];
            }
            //Debug.Log(string.Format("explored: {0}, expanded: {1}", POP.Open, POP.Expanded));
            return null;
        }

        public List<string> RunTest(float cutoffTime)
        {
            Parser.path = @"D:\documents\frostbow\";
            DeCacheIt();

            var initialPlan = PlannerScheduler.CreateInitialPlan(initial, goal);
            var PlanSteps = new List<string>();

            //Debug.Log("Planner and initial plan Prepared");

            // MW-Loc-Conf

            //var solution = Run(initialPlan, new ADstar(false), new E0(new ZeroHeuristic()), cutoffTime);
            //var solution = Run(initialPlan, new ADstar(false), new E0(new NumOpenConditionsHeuristic()), cutoffTime);
            //var solution = Run(initialPlan, new ADstar(false), new E0(new AddReuseHeuristic(), true), cutoffTime);
            // var solution = Run(initialPlan, new ADstar(false), new E3(new AddReuseHeuristic()), cutoffTime);


            var solution = Run(initialPlan, new ADstar(false), new E3Star(new AddReuseHeuristic()), cutoffTime);
            //var solution = Run(initialPlan, new ADstar(false), new E3(new AddReuseHeuristic()), cutoffTime);


            //var solution = Run(initialPlan, new ADstar(false), new E3(new AddReuseHeuristic()), cutoffTime);
            //var solution = Run(initPlan, new BFS(), new Nada(new ZeroHeuristic()), 20000);
            if (solution != null)
            {
                //Debug.Log(solution.ToStringOrdered());

                foreach (var step in solution.Orderings.TopoSort(solution.InitialStep))
                {
                    Console.WriteLine(step);
                    PlanSteps.Add(step.ToString());
                }
            }
            else
            {

                Console.WriteLine("No good");
            }
            return PlanSteps;
        }
    }
}
