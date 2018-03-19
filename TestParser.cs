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

public class TestParser : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var testDomainName = "batman";
        var testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
        var testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
        var testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");

        Debug.Log("Creating Ground Operators");
        GroundActionFactory.PopulateGroundActions(testDomain.Operators, testProblem);
        Debug.Log("Caching Maps");
        CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
        CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, testProblem.Goal);
        Debug.Log("Finding static preconditions");
        GroundActionFactory.DetectStatics(CacheMaps.CausalMap, CacheMaps.ThreatMap);

        Debug.Log("Creating initial Plan");
        // Create Initial Plan
        // public Plan(List<IOperator> steps, IState initial, IState goal, Graph<IOperator> og, ICausalLinkGraph clg, Flawque flawQueue)
        // IState _initial, IState _goal, List<IOperator> _steps
        var initialPlan = new Plan(testProblem.Initial as IState, testProblem.Goal as IState);
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
