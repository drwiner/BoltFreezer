using Mediation.Enums;
using Mediation.FileIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestParser : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var testDomainName = "batman";
        var testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
        var testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
        var testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
        Debug.Log("testing");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
