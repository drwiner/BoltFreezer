using System;
using System.Collections.Generic;
using BoltFreezer.CacheTools;
using BoltFreezer.DecompTools;
using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;

namespace TestFreezer
{
    class TestAll
    {

        public static void RunPlanner(IPlan initPi, ISearch SearchMethod, ISelection SelectMethod, int k, float cutoff, string directoryToSaveTo, int problem)
        {
            var POP = new PlanSpacePlanner(initPi, SelectMethod, SearchMethod, true)
            {
                directory = directoryToSaveTo,
                problemNumber = problem,
            };
            var Solutions = POP.Solve(k, cutoff);
            if (Solutions != null)
            {
                Console.WriteLine(Solutions[0].ToStringOrdered());
            }
            
        }

        public static void RunJsonTravelExperiment()
        {
            Console.Write("hello world\n");
            var directory = @"D:\Documents\Frostbow\TravelExperiment\";
            var cutoff = 6000f;
            var k = 1;
            for (int i = 1; i < 9; i++)
            {
                var initPlan = JsonProblemDeserializer.DeserializeJsonTravelDomain(i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E0(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E1(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E2(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E4(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E5(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E6(new AddReuseHeuristic()), k, cutoff, directory, i);

                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E0(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E1(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E2(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E4(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E5(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E6(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);

                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E0(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E1(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E2(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E4(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E5(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E6(new ZeroHeuristic()), k, cutoff, directory, i);


                RunPlanner(initPlan.Clone() as IPlan, new DFS(), new Nada(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new BFS(), new Nada(new ZeroHeuristic()), k, cutoff, directory, i);
            }
        }

        public static void TestBenchmarks()
        {
            var domainNames = new List<string>() { "arth", "batman" };
            foreach (var dn in domainNames)
            {
                var testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + dn + @"\domain.pddl";
                var testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + dn + @"\domain.pddl", PlanType.PlanSpace);
                var testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + dn + @"\prob01.pddl");

                ProblemFreezer PF = new ProblemFreezer(dn, testDomainDirectory, testDomain, testProblem);
                PF.Serialize();

                var directory = @"D:\Documents\Frostbow\Benchmarks\Results\";
                var cutoff = 6000f;
                var k = 1;

                var initPlan = PlanSpacePlanner.CreateInitialPlan(PF);

                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E0(new AddReuseHeuristic()), k, cutoff, directory, 0);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E1(new AddReuseHeuristic()), k, cutoff, directory, 0);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E2(new AddReuseHeuristic()), k, cutoff, directory, 0);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(new AddReuseHeuristic()), k, cutoff, directory, 0);
                //RunPlanner(initPlan.Clone() as IPlan, new DFS(), new Nada(new ZeroHeuristic()), k, cutoff, directory, 0);
                RunPlanner(initPlan.Clone() as IPlan, new BFS(), new Nada(new ZeroHeuristic()), k, cutoff, directory, 0);

            }
        }

        static void Main(string[] args)
        {
            var cutoff = 6000f;
            var k = 1;
            var initPlan = TravelTest.ReadAndCompile(true);
            var directory = @"D:\Documents\Frostbow\Benchmarks\travel-test\Results\";
            System.IO.Directory.CreateDirectory(directory);
            RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E0(new AddReuseHeuristic()), k, cutoff, directory, 0);
        }

    }
}
