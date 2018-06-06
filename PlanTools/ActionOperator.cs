using BoltFreezer.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class ActionOperator : Operator, IAction, IOperator
    {
        public new List<Literal> preconditions;
        public new List<Literal> effects;

        public new List<Literal> Preconditions
        {
            get { return preconditions; }
            set { preconditions = value; }
        }
        public new List<Literal> Effects
        {
            get { return effects; }
            set { effects = value; }
        }

        public ActionOperator() : base()
        {
            preconditions = new List<Literal>();
            effects = new List<Literal>();
        }

        public ActionOperator(Operator op) : base(op.Name, op.Terms, op.Bindings, op.Preconditions, op.Effects, op.Conditionals, op.ID)
        {
            foreach (var precon in op.Preconditions)
            {
                preconditions.Add(new Literal(precon));
            }
            foreach (var effect in op.Effects)
            {
                effects.Add(new Literal(effect));
            }
        }

        public ActionOperator(string name, List<ITerm> terms, Hashtable binds, List<Literal> preconditions, List<Literal> effects, List<IAxiom> conditionals, int id)
        {
            this.Name = name;
            this.Terms = terms;
            this.Bindings = bindings;
            this.Preconditions = preconditions;
            this.Effects = effects;
            this.Conditionals = conditionals;
            this.id = id;
        }


        // Creates a clone of the operator
        public new Object Clone()
        {
            var opClone = base.Clone() as Operator;
            var listOfLiteralPreconditions = new List<Literal>();
            foreach (var precon in opClone.Preconditions)
            {
                listOfLiteralPreconditions.Add(new Literal(precon));
            }
            var listOfLiteralEffects = new List<Literal>();
            foreach (var eff in opClone.Effects)
            {
                listOfLiteralEffects.Add(new Literal(eff));
            }

            return new ActionOperator(opClone.Name, opClone.Terms, opClone.Bindings, listOfLiteralPreconditions, listOfLiteralEffects, opClone.Conditionals, ID)
            {
                Height = height,
                NonEqualities = nonequalities
            };
        }
    }
}
