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

            var suborderings = new List<Tuple<IPlanStep, IPlanStep>>()
            {
                new Tuple<IPlanStep,IPlanStep>(move1, move2)
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
            var suborderings = new List<Tuple<IPlanStep, IPlanStep>>()
            {
                new Tuple<IPlanStep,IPlanStep>(pickup, move),
                new Tuple<IPlanStep,IPlanStep>(move, putdown)
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
                new Term("?agent")     { Type = "steeringagent"},
                new Term("?item")         { Type = "block"},
                new Term("?adjacentfrom") {Type="location"},
                new Term("?from")         { Type = "location"},
                new Term("?to")         { Type = "location"}
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

        public static Tuple<Composite, List<Decomposition>> TransportComposites()
        {
            var decomps =  ReadTransportDecompositions();
            var composite = ReadTransportCompositeOperator();
            var CompositeMethods = new Tuple<Composite, List<Decomposition>>(composite, decomps);
            return CompositeMethods;
        }

        public static Tuple<Composite, List<Decomposition>> MultimoveComposites()
        {
            var decomps = ReadMultimoveDecompositions();
            var composite = ReadMultimoveCompositeOperator();
            var CompositeMethods = new Tuple<Composite, List<Decomposition>>(composite, decomps);
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
        

        public static IPlan ReadAndCompile(bool serializeIt, int whichProblem)
        {
            Parser.path = @"D:\documents\frostbow\boltfreezer\";

            GroundActionFactory.Reset();
            CacheMaps.Reset();

            // Reads Domain and problem, also populates ground actions and caches causal maps
            var pfreeze = ReadDomainAndProblem(serializeIt, whichProblem);

            // Detecting static conditions
            Console.WriteLine("Detecting Statics");
            GroundActionFactory.DetectStatics(CacheMaps.CausalMap, CacheMaps.ThreatMap);

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
            Composite.ComposeHTNs(1, CompositeMethods);

            // Caching Causal Maps
            Console.WriteLine("Caching Causal Maps");
            CacheMaps.Reset();
            CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
            CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, initPlan.Goal.Predicates);
            CacheMaps.CacheAddReuseHeuristic(initPlan.Initial, initPlan.Goal.Predicates);
            initPlan = PlanSpacePlanner.CreateInitialPlan(pfreeze);


            return initPlan;
        }

    }
}
