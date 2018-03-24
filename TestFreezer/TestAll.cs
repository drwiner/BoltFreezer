using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoltFreezer.CacheTools;
using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
//using SimpleJSON;

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
                //RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new AddReuseHeuristic(), 1, 20000, directory, i);
                //RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E1(), 1, 20000, directory, i);
                //RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E2(), 1, 20000, directory, i);
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
                //RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E4(), 1, 20000, directory, i);
                //RunBestFirstPOP(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i, 10000, new E2().Heuristic, HeuristicType.E2);

                //RunAddReusePop(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
                //RunAddReusePopE1(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
                //RunAddReusePopE2(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i, 10000f);
                //RunAddReusePopE3(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i, 10000f);
                //RunAddReusePopE4(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
                //RunNumOCsPOP(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
                //RunBestFirstZeroPOP(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
                //RunBFSPOP(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
                //RunDFSPop(initPlan.Clone() as IPlan, @"D:\Documents\workspace\travel_domain.travel\", i);
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
