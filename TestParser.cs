using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BoltFreezer.Utilities;
using BoltFreezer.PlanSpace;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

public class TestParser : MonoBehaviour {
    /// <summary>
    /// Serializes an object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serializableObject"></param>
    /// <param name="fileName"></param>
    public void SerializeObject<T>(T serializableObject, string fileName)
    {
        if (serializableObject == null) { return; }

        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, serializableObject);
                stream.Position = 0;
                xmlDocument.Load(stream);
                xmlDocument.Save(fileName);
                stream.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.Log(serializableObject); 
            //Log exception here
        }
    }


    /// <summary>
    /// Deserializes an xml file into an object list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public T DeSerializeObject<T>(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) { return default(T); }

        T objectOut = default(T);

        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fileName);
            string xmlString = xmlDocument.OuterXml;

            using (StringReader read = new StringReader(xmlString))
            {
                Type outType = typeof(T);

                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    objectOut = (T)serializer.Deserialize(reader);
                    reader.Close();
                }

                read.Close();
            }
        }
        catch (Exception ex)
        {
            //Log exception here
        }

        return objectOut;
    }

    // Use this for initialization
    void Start () {
        var testDomainName = "batman";
        var testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
        var testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
        var testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");

        string FileName = Parser.GetTopDirectory() + @"Test\" + testDomainName + "_" + testProblem.Name;
        

        Debug.Log("Creating Ground Operators");
        GroundActionFactory.PopulateGroundActions(testDomain.Operators, testProblem);
        foreach(var op in GroundActionFactory.GroundActions)
            SerializeObject(op, FileName + "@_" + op.ID + ".XML");
        
        
        Debug.Log("Caching Maps");
        CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
        CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, testProblem.Goal);
        Debug.Log("Finding static preconditions");
        GroundActionFactory.DetectStatics(CacheMaps.CausalMap, CacheMaps.ThreatMap);


        Debug.Log("Creating initial Plan");
        // Create Initial Plan
        // public Plan(List<IOperator> steps, IState initial, IState goal, Graph<IOperator> og, ICausalLinkGraph clg, Flawque flawQueue)
        // IState _initial, IState _goal, List<IOperator> _steps
        var initialPlan = new Plan(new State(testProblem.Initial) as IState, new State(testProblem.Goal) as IState);
        foreach (var goal in testProblem.Goal)
        {
            initialPlan.Flaws.Insert(initialPlan, new OpenCondition(goal, initialPlan.GoalStep as IOperator));
        }
        Debug.Log("Insert First Ordering");
        initialPlan.Orderings.Insert(initialPlan.InitialStep, initialPlan.GoalStep);

        Debug.Log("First POP");
        var AStarPOP = new PlanSpacePlanner(initialPlan, SearchType.BestFirst, new AddReuseHeuristic().Heuristic);
        var bestFirstSolutions = AStarPOP.Solve(1, 6000f);
        Debug.Log(bestFirstSolutions[0]);

        var BFSPOP = new PlanSpacePlanner(initialPlan, SearchType.BFS, new ZeroHeuristic().Heuristic);
        var BFSSolutions = BFSPOP.Solve(1, 6000f);
        Debug.Log(BFSSolutions[0]);

        var DFSPOP = new PlanSpacePlanner(initialPlan, SearchType.DFS, new ZeroHeuristic().Heuristic);
        var DFSSolutions = DFSPOP.Solve(1, 6000f);
        Debug.Log(DFSSolutions[0]);
    }

    

    // Update is called once per frame
    void Update () {
		
	}
}
