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

public class TestParser : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var testDomainName = "batman";
        var testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
        var testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
        var testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
        
        var GAF = new GroundActionFactory(testDomain.Operators, testProblem);
    }

    

    // Update is called once per frame
    void Update () {
		
	}
}
