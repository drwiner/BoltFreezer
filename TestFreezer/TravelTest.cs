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
    public static class TravelTest
    {

        public static ProblemFreezer ReadDomainAndProblem(bool serializeIt, int whichProblem)
        {
            var domainName = "travel-test";
            var domainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl";
            var domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.PlanSpace);
            var problem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\travel-" + whichProblem.ToString() + @".pddl");

            var PF = new ProblemFreezer(domainName, domainDirectory, domain, problem);
            if (serializeIt)
                PF.Serialize();
            else
                PF.Deserialize();
            return PF;
        }


        public static Decomposition TravelByCar()
        {

            //      (: action travel - by - car
            //      :parameters(?person - person ? from ? to - place)
            //:precondition(and(at ? person ? from)(not(= ? from ? to)))
            //      :effect(and(at ? person ? to)
            //                  (not(at ? person ? from)))
            //:decomp(
            // :sub -params (? car - car ? s1 ? s2 ? s3 - step)
            // :requirements(and
            //              (= ? s1(get -in -car ? person ? car ? from))
            //              (= ? s2(drive ? person ? car ? from ? to))
            //              (= ? s3(get -out -of - car ? person ? car ? to))
            //              (linked - by ? s1 ? s2(in ? person ? car))
            //              (linked - by ? s1 ? s3(in ? person ? car))
            //              (linked - by ? s2 ? s3(at ? car ? to)))))

            // Params
            var objTerms = new List<ITerm>() {
                new Term("?person")     { Type = "person"},
                new Term("?from")       { Type = "place"},
                new Term("?to")         { Type = "place"},
                new Term("?car")        { Type = "car"}
            };

            var litTerms = new List<IPredicate>();

            var s1terms = new List<ITerm>() { objTerms[0], objTerms[3], objTerms[1] };
            var s2terms = new List<ITerm>() { objTerms[0], objTerms[3], objTerms[1], objTerms[2] };
            var s3terms = new List<ITerm>() { objTerms[0], objTerms[3], objTerms[2] };

            var getInCar = new PlanStep(new Operator(new Predicate("get-in-car", s1terms, true)));
            var drive = new PlanStep(new Operator(new Predicate("drive", s2terms, true)));
            var getOutOfCar = new PlanStep(new Operator(new Predicate("get-out-of-car", s3terms, true)));

            var inPersonCar = new Predicate("in", new List<ITerm>() { objTerms[0], objTerms[3] }, true);
            var atCarTo = new Predicate("at", new List<ITerm>() { objTerms[3], objTerms[2] }, true);

            //new Operator()
            var substeps = new List<IPlanStep>() { getInCar, drive, getOutOfCar };
            var sublinks = new List<CausalLink<IPlanStep>>()
            {
                new CausalLink<IPlanStep>(inPersonCar, getInCar, drive),
                new CausalLink<IPlanStep>(inPersonCar, getInCar, getOutOfCar),
                new CausalLink<IPlanStep>(atCarTo, drive, getOutOfCar)
            };
            var suborderings = new List<BoltFreezer.Utilities.Tuple<IPlanStep, IPlanStep>>()
            {
                new BoltFreezer.Utilities.Tuple<IPlanStep,IPlanStep>(getInCar, drive),
                new BoltFreezer.Utilities.Tuple<IPlanStep,IPlanStep>(drive, getOutOfCar)
            };

            var root = new Operator(new Predicate("travel-by-car", objTerms, true));
            var decomp = new Decomposition(root, litTerms, substeps, suborderings, sublinks);
            return decomp;
        }

        public static Decomposition TravelByPlane()
        {

            //  (: action travel - by - plane
            //      :parameters(?person - person ? from ? to - place)
            //:precondition(and(at ? person ? from)(not(= ? from ? to)))
            //      :effect(and(at ? person ? to)
            //                  (not(at ? person ? from)))
            //:decomp(
            // :sub -params (? plane - plane
            //              ? s1 ? s2 ? s3 ? s4 - step)
            // :requirements(and
            //              (= ? s1(buy - tickets ? person))
            //              (= ? s2(board - plane ? person ? plane ? from))
            //              (= ? s3(fly ? person ? plane ? from ? to))
            //              (= ? s4(deplane ? person ? plane ? to))
            //              (linked - by ? s1 ? s2(has - ticket ? person))
            //              (linked - by ? s2 ? s3(in ? person ? plane))
            //              (linked - by ? s2 ? s4(in ? person ? plane))
            //              (linked - by ? s3 ? s4(at ? plane ? to)))))

            // Params
            var objTerms = new List<ITerm>() {
                new Term("?person")     { Type = "person"},
                new Term("?from")       { Type = "place"},
                new Term("?to")         { Type = "place"},
                new Term("?plane")        { Type = "plane"}
            };

            var litTerms = new List<IPredicate>();

            var buyTerms = new List<ITerm>() { objTerms[0] };
            var boardTerms = new List<ITerm>() { objTerms[0], objTerms[3], objTerms[1] };
            var flyTerms = new List<ITerm>() { objTerms[0], objTerms[3], objTerms[1], objTerms[2] };
            var deplaneTerms = new List<ITerm>() { objTerms[0], objTerms[3], objTerms[2] };


            var buy = new PlanStep(new Operator(new Predicate("buy", buyTerms, true)));
            var board = new PlanStep(new Operator(new Predicate("board", boardTerms, true)));
            var fly = new PlanStep(new Operator(new Predicate("fly", flyTerms, true)));
            var deplane = new PlanStep(new Operator(new Predicate("deplane", deplaneTerms, true)));

            var hasTicketPerson = new Predicate("has-ticket", new List<ITerm>() {objTerms[0]}, true);
            var inPersonPlane = new Predicate("in", new List<ITerm>() { objTerms[0], objTerms[3] }, true);
            var atPlaneTo = new Predicate("at", new List<ITerm>() { objTerms[3], objTerms[2] }, true);

            //new Operator()
            var substeps = new List<IPlanStep>() { buy, board, fly, deplane };
            var sublinks = new List<CausalLink<IPlanStep>>()
            {
                new CausalLink<IPlanStep>(hasTicketPerson, buy, board),
                new CausalLink<IPlanStep>(inPersonPlane, board, fly),
                new CausalLink<IPlanStep>(inPersonPlane, board, deplane),
                new CausalLink<IPlanStep>(atPlaneTo, fly, deplane)
            };

            var suborderings = new List<BoltFreezer.Utilities.Tuple<IPlanStep, IPlanStep>>()
            {
                new BoltFreezer.Utilities.Tuple<IPlanStep,IPlanStep>(buy, board),
                new BoltFreezer.Utilities.Tuple<IPlanStep,IPlanStep>(board, fly),
                new BoltFreezer.Utilities.Tuple<IPlanStep,IPlanStep>(fly, deplane)
            };

            var root = new Operator(new Predicate("travel-by-plane", objTerms, true));
            var decomp = new Decomposition(root, litTerms, substeps, suborderings, sublinks);
            return decomp;
        }

        public static Decomposition GenericSubStepExample()
        {
            //(: action travel
            //    : parameters(?person - person ? to - place)
            // :precondition()
            // :effect(and(at ? person ? to))
            // :decomp(
            //     :sub -params(? travel - step - step)
            //     :requirements(and
            //            (effect ? travel - step(at ? person ? to)))))

            var objTerms = new List<ITerm>() {
                new Term("?person")     { Type = "person"},
                new Term("?to")         { Type = "place"},
            };

            var litTerms = new List<IPredicate>();

            var atPersonTo = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[1]}, true);
            var travelOp = new Operator("", new List<IPredicate>(), new List<IPredicate>(){ atPersonTo});
            // check that travelOp.Terms are updated based on Effect? No this won't happen...
            var travelSubStep = new PlanStep(travelOp);


            var root = new Operator(new Predicate("generic-travel", objTerms, true));
            var decomp = new Decomposition(root, litTerms, new List<IPlanStep>() { travelSubStep }, new List<BoltFreezer.Utilities.Tuple<IPlanStep, IPlanStep>>(), new List<CausalLink<IPlanStep>>());
            return decomp;
        }

        public static Composite AddCompositeOperator()
        {
            var objTerms = new List<ITerm>() {
                new Term("?person")     { Type = "person"},
                new Term("?from")         { Type = "place"},
                new Term("?to")         { Type = "place"},
            };

            var atPersonFrom = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[1] }, true);
            var notAtPersonFrom = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[1] }, false);
            var atPersonTo = new Predicate("at", new List<ITerm>() { objTerms[0], objTerms[2] }, true);

            var op = 
                new Operator(new Predicate("Travel", objTerms, true),
                    new List<IPredicate>() { atPersonFrom },
                    new List<IPredicate>() { notAtPersonFrom, atPersonTo }
                )
            {
                NonEqualities = new List<List<ITerm>>() { new List<ITerm>() { objTerms[1], objTerms[2] } }
            };

            return new Composite(op);
        }

        public static List<Decomposition> ReadDecompositions()
        {
            var decomps = new List<Decomposition>();

            var travelByCar = TravelByCar();
            var travelByPlane = TravelByPlane();
            var genericTravel = GenericSubStepExample();

            decomps.Add(travelByCar);
            decomps.Add(travelByPlane);
            decomps.Add(genericTravel);
            return decomps;
        }

        public static IPlan ReadAndCompile(bool serializeIt, int whichProblem)
        {
            Parser.path = @"D:\documents\frostbow\boltfreezer\";

            GroundActionFactory.Reset();
            CacheMaps.Reset();

            var pfreeze = ReadDomainAndProblem(serializeIt, whichProblem);

            var decomps = ReadDecompositions();
            var composite = AddCompositeOperator();

            var CompositeMethods = new Dictionary<Composite, List<Decomposition>>();
            CompositeMethods[composite] = decomps;
            Composite.ComposeHTNs(2, CompositeMethods);

            CacheMaps.CacheLinks(GroundActionFactory.GroundActions);

            var initPlan = PlanSpacePlanner.CreateInitialPlan(pfreeze);

            CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, initPlan.Goal.Predicates);

            GroundActionFactory.DetectStatics();

            return initPlan;
        }

    }
}
