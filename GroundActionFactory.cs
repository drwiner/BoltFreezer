using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BoltFreezer.PlanTools
{
    public class GroundActionFactory
    {
        public List<IOperator> GroundActions;
        private Problem prob;
        private Hashtable typeDict;

        public GroundActionFactory(List<IOperator> ops, Problem _prob)
        {
            GroundActions = new List<IOperator>();
            prob = _prob;
            typeDict = _prob.TypeList;
            FromOperators(ops);
        }

        public void FromOperator(IOperator op)
        {

            var permList = new List<List<IObject>>();
            foreach (Term variable in op.Terms)
            {
                permList.Add(typeDict[variable.Type] as List<IObject>);
            }

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
}