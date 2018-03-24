using System;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;

namespace TestFreezer
{
    class TestAll
    {

        public static void RunPlanner(IPlan initPi, ISearch SearchMethod, IHeuristic HeuristicMethod, int k, float cutoff, string directoryToSaveTo, int problem)
        {
            var POP = new PlanSpacePlanner(initPi, SearchMethod, HeuristicMethod, true)
            {
                directory = directoryToSaveTo,
                problemNumber = problem,
            };

            var Solutions = POP.Solve(k, cutoff);
        }

        static void Main(string[] args)
        {

            Console.Write("hello world\n");
            var directory = @"D:\Documents\workspace\travel_domain.travel\";

            for (int i = 8; i < 9; i++)
            {
                var initPlan = JsonProblemDeserializer.DeserializeJsonTravelDomain(i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(2,2), 1, 50000, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(6,2), 1, 50000, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(2, 4), 1, 50000, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(6, 4), 1, 50000, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(2, 6), 1, 50000, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(6, 6), 1, 50000, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(2, 8), 1, 50000, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(6, 8), 1, 50000, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(2, 10), 1, 50000, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(6, 10), 1, 50000, directory, i);
            }
        }
    }
}
