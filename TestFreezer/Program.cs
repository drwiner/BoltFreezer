using System;
using System.Collections.Generic;
using System.IO;
using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;

namespace TestFreezer
{
    class Program
    {

        public static void FreezeProblem(bool RELOAD)
        {
            var testDomainName = "batman";
            var testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            var testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
            var testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");

            //*.CachedCausalMap
            //*.CachedThreatMap
            //*.CachedOperator

            string FileName = Parser.GetTopDirectory() + @"Cached\CachedOperators\" + testDomainName + "_" + testProblem.Name;
            string CausalMapFileName = Parser.GetTopDirectory() + @"Cached\CausalMaps\" + testDomainName + "_" + testProblem.Name;
            string ThreatMapFileName = Parser.GetTopDirectory() + @"Cached\ThreatMaps\" + testDomainName + "_" + testProblem.Name;

            if (RELOAD)
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
            else
            {
                List<IOperator> Operators = new List<IOperator>();
                foreach (var file in Directory.GetFiles(Parser.GetTopDirectory() + @"Cached\CachedOperators\", testDomainName + "_" + testProblem.Name + "*.CachedOperator"))
                {
                    var op = BinarySerializer.DeSerializeObject<IOperator>(file);
                    Operators.Add(op);
                }
                GroundActionFactory.GroundActions = Operators;
                foreach (var ga in GroundActionFactory.GroundActions)
                {
                    Console.WriteLine(ga);
                }
            }
        }

        static void Main(string[] args)
        {
            Console.Write("hello world\n");
            FreezeProblem(true);
            Console.WriteLine("\nFinished!");
            Console.Read();
            //Console.Write("hello world");
        }
    }
}
