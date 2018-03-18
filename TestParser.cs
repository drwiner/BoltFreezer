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
        

        Hashtable typeDict = testProblem.TypeList;

        // Step 1: Try grounding a single action
        IOperator move = testDomain.Operators[0];
        
        
        List<List <IObject>> permList = new List<List<IObject>>();
        foreach (Term variable in move.Terms) {
            //List<IObject> thisTermCandidates = typeDict[variable.Type] as List<IObject>;
            permList.Add(typeDict[variable.Type] as List<IObject>);
        }

        List<IOperator> GroundOperatorList = new List<IOperator>();
        foreach (var combination in EnumerableExtension.GenerateCombinations(permList))
        {
            
            // Add bindings
            var opClone = move.Clone() as Operator;
            IEnumerable<string> termStringList = from term in opClone.Terms select term.Variable;

            IEnumerable<string> constantStringList = from objConst in combination select objConst.Name;

            opClone.AddBindings(termStringList.ToList(), constantStringList.ToList());
            //pClone.Terms.ToList(term => term.Variable);
            //opClone.AddBinding(opClone.Terms[0].Variable, combination[0].Name);

            Debug.Log("operator: " + opClone.ToString());
        }
    }

    

    // Update is called once per frame
    void Update () {
		
	}
}
