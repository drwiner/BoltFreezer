using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.DecompTools
{
    public class Decomposition : Operator, IComposite
    {
        private IOperator initialStep;
        private IOperator goalStep;
        private List<Tuple<IPlanStep, IPlanStep>> subOrderings;
        private List<CausalLink<IPlanStep>> subLinks;
        private List<IPlanStep> subSteps;
        private List<IPredicate> literals;

        public List<IPredicate> Literals
        {
            get { return literals; }
            set { literals = value; }
        }

        public IOperator InitialStep
        {
            get { return initialStep; }
            set { initialStep = value; }
        }

        public IOperator GoalStep
        {
            get { return goalStep; }
            set { goalStep = value; }
        }

        public List<IPlanStep> SubSteps
        {
            get { return subSteps; }
            set { subSteps = value; }
        }

        public List<Tuple<IPlanStep, IPlanStep>> SubOrderings
        {
            get { return subOrderings; }
            set { subOrderings = value; }
        }

        public List<CausalLink<IPlanStep>> SubLinks
        {
            get { return subLinks; }
            set { subLinks = value; }
        }

        public Decomposition() : base()
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = new Operator();
            goalStep = new Operator();
            literals = new List<IPredicate>();
        }

        public Decomposition(string name, List<ITerm> terms, IOperator init, IOperator dummy, List<IPredicate> Preconditions, List<IPredicate> Effects, int ID)
            : base(name, terms, new Hashtable(), Preconditions, Effects, ID)
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = new Operator();
            goalStep = new Operator();
        }

        public Decomposition(IOperator core, List<IPredicate> literals, List<IPlanStep> substeps, List<Tuple<IPlanStep, IPlanStep>> suborderings, List<CausalLink<IPlanStep>> sublinks)
            : base(core.Name, core.Terms, new Hashtable(), core.Preconditions, core.Effects, core.ID)
        {
            this.literals = literals;
            subOrderings = suborderings;
            subLinks = sublinks;
            subSteps = substeps;
            initialStep = new Operator();
            goalStep = new Operator();
        }

        public static List<Composite> Plannify(Decomposition decomp)
        {
            var compList = new List<Composite>();

            // First, for each grounding of operator terms

            var decompList = GroundDecompArgs(decomp);

            var substepDict = new Dictionary<int, List<Operator>>();
            var comboList = new List<List<Operator>>();
            foreach(var substep in decomp.SubSteps)
            {
                var cndts = ConsistentSteps(substep.Action as Operator);
                substepDict[substep.ID] = cndts;
                comboList.Add(cndts);
            }

            foreach (var combination in EnumerableExtension.GenerateCombinations(comboList))
            {
                foreach (var step in combination)
                {
                    
                }
            }

            // For each substep, find consistent ground step, check if it is "arg consistent". Each sub-step arg has unique ID. predicate-based args cannot be unique instances. 
            // Also check if non equality constraints are observed. But, it may be that this isn't needed. it is in cinepydpocl not in pydpocl
            // Then, filter and add orderings. When we add orderings, we also add orderings for all sub-steps
            // Then add links and check if links are possible. If the linked condition is null, then any link condition will do.
            // Finally, Create a Composite step out of this by propagating preconditions and effects to the top-level. 

            return compList;
        }

        public static List<Decomposition> GroundDecompArgs(Decomposition decomp)
        {
            var permList = new List<List<IObject>>();
            foreach (Term variable in decomp.Terms)
            {
                permList.Add(GroundActionFactory.TypeDict[variable.Type] as List<IObject>);
            }

            var decompList = new List<Decomposition>();
            foreach (var combination in EnumerableExtension.GenerateCombinations(permList))
            {
                // Add bindings
                var decompClone = decomp.Clone() as Decomposition;
                var termStringList = from term in decompClone.Terms select term.Variable;
                var constantStringList = from objConst in combination select objConst.Name;

                decompClone.AddBindings(termStringList.ToList(), constantStringList.ToList());
                decompList.Add(decompClone);
            }
            return decompList;
        }

        public static bool IsParamConsistent()
        {
            return true;
        }

        /// <summary>
        /// Finds a set of ground Operators that are consistent with this substep
        /// </summary>
        /// <param name="substep"></param>
        /// <returns></returns>
        public static List<Operator> ConsistentSteps(Operator substep)
        {
            
            var Cndts = GroundActionFactory.GroundActions as IEnumerable<Operator>;

            Cndts = FilterOperatorsByPredicate(substep, Cndts);
            Cndts = FilterOperatorsByPreconditionsAndEffects(substep, Cndts);

            return Cndts as List<Operator>;
        }

        public static IEnumerable<Operator> FilterOperatorsByPredicate(Operator substep, IEnumerable<Operator> Cndts)
        {
            return Cndts.Where(op => op.Predicate.IsConsistent(substep.Predicate));
        }

        public static IEnumerable<Operator> FilterOperatorsByPreconditionsAndEffects(Operator substep, IEnumerable<Operator> Cndts)
        {

            foreach (var eff in substep.Effects)
            {
                Cndts = FilterOperatorsByEffect(eff as Predicate, Cndts);
            }
            foreach(var pre in substep.Preconditions)
            {
                Cndts = FilterOperatorsByPrecondition(pre as Predicate, Cndts);
            }
            return Cndts;
        }

        public static IEnumerable<Operator> FilterOperatorsByPrecondition(Predicate precon, IEnumerable<Operator> Cndts)
        {
            return Cndts.Where(op => op.Preconditions.Any(pre => pre.IsConsistent(precon)));
        }

        public static IEnumerable<Operator> FilterOperatorsByEffect(Predicate effect, IEnumerable<Operator> Cndts)
        {
            return Cndts.Where(op => op.Effects.Any(pre => pre.IsConsistent(effect)));
        }


        public new Object Clone()
        {
            return new Decomposition(this as IOperator, Literals, SubSteps, SubOrderings, SubLinks);
        }
    }
}
