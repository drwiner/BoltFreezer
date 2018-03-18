using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GroundActionFactory {

    public List<Operator> GroundActions;
    private Problem prob;
    private Hashtable typeDict;

    public GroundActionFactory(List<IOperator> ops, Problem _prob)
    {
        prob = _prob;
        typeDict = _prob.TypeList;
        FromOperators(ops);
    }

    public void FromOperator(IOperator op)
    {
        var permList = (from variable in op.Terms select typeDict[variable.Type] as List<IObject>) as List<List<IObject>>;

        //List < IOperator > GroundOperatorList = new List<IOperator>();
        foreach (var combination in EnumerableExtension.GenerateCombinations(permList))
        {
            // Add bindings
            var opClone = op.Clone() as Operator;
            var termStringList = from term in opClone.Terms select term.Variable;
            var constantStringList = from objConst in combination select objConst.Name;

            opClone.AddBindings(termStringList.ToList(), constantStringList.ToList());
            Debug.Log("operator: " + opClone.ToString());
            GroundActions.Add(opClone);
        }
    }

    public void FromOperators(List<IOperator> operators)
    {
        foreach (var op in operators)
        {
            FromOperator(op);
        } 
    }
}
