﻿using BoltFreezer.Interfaces;
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
            // List<Composite> fully ground decomp operators, return when ready to be cached.
            var compList = new List<Composite>();

            // List<Decomposition> decompList = For each grounding of operator terms with constants possessing consistent types.
            var decompList = Compose(decomp);

            // Create "Planets" (i.e. sub-plans) by aggregating legal/consistent ground steps replacing sub-steps 
           
            foreach (var newDecomp in decompList)
            {
                // get legal effects
            }
            

            // For each substep, find consistent ground step, check if it is "arg consistent". Each sub-step arg has unique ID. predicate-based args cannot be unique instances. 
            // Also check if non equality constraints are observed. But, it may be that this isn't needed. it is in cinepydpocl not in pydpocl
            // Then, filter and add orderings. When we add orderings, we also add orderings for all sub-steps
            // Then add links and check if links are possible. If the linked condition is null, then any link condition will do.
            // Finally, Create a Composite step out of this by propagating preconditions and effects to the top-level. 

            return compList;
        }

        // Finds consistent decomp
        public static void FilterDecompCandidates(List<Decomposition> decompList, Decomposition decomp)
        {
            // find and replace sub-steps 
            //var substepDict = new Dictionary<int, List<Operator>>();
            var comboList = new List<List<Operator>>();
            var ID_List = new List<int>();
            foreach (var substep in decomp.SubSteps)
            {
                ID_List.Add(substep.ID);
                // each substep has ground terms that are already consistent
                var cndts = ConsistentSteps(substep.Action as Operator);
                //substepDict[substep.ID] = cndts;
                comboList.Add(cndts);
            }
            
            foreach (var combination in EnumerableExtension.GenerateCombinations(comboList))
            {
                var decompClone = decomp.Clone() as Decomposition;
                var newSubsteps = new List<IPlanStep>();
                var substepDict = new Dictionary<int, IPlanStep>();
                var order = 0;
                foreach (var item in combination)
                {
                    var originalID = ID_List[order++];
                    var newPlanStep = new PlanStep(item);
                    substepDict[originalID] = newPlanStep;
                    newSubsteps.Add(newPlanStep);
                }

                var newSuborderings = new List<Tuple<IPlanStep, IPlanStep>>();
                foreach (var subordering in decomp.SubOrderings)
                {
                    var first = substepDict[subordering.First.ID];
                    var second = substepDict[subordering.Second.ID];
                    newSuborderings.Add(new Tuple<IPlanStep, IPlanStep>(first, second));
                }

                var linkWorlds = new List<List<CausalLink<IPlanStep>>>();
                var newSublinks = new List<CausalLink<IPlanStep>>();
                foreach (var sublink in decomp.SubLinks)
                {

                    var cndts = sublink.Head.Effects.Where(eff => eff.IsConsistent(sublink.Predicate) && sublink.Tail.Preconditions.Any(pre=> pre.Equals(eff)));
                    foreach (var cndt in cndts)
                    {
                        var dependency = cndt.Clone() as Predicate;
                        var head = substepDict[sublink.Head.ID].Clone() as IPlanStep;
                        var tail = substepDict[sublink.Tail.ID].Clone() as IPlanStep;
                        var newLink = new CausalLink<IPlanStep>(dependency, head, tail);
                        newLink.Tail.Fulfill(cndt);

                        var clonedLinks = EnumerableExtension.CloneList(newSublinks);
                        linkWorlds.Add(clonedLinks);
                        foreach(var linkworld in linkWorlds)
                        {
                            linkworld.Add(newLink);
                        }   
                    }
                }

                foreach (var linkworld in linkWorlds)
                {
                    var newDecomp = decomp.Clone() as Decomposition;
                    newDecomp.SubSteps = newSubsteps;
                    newDecomp.SubOrderings = newSuborderings;
                    newDecomp.SubLinks = linkworld;

                    decompList.Add(newDecomp);
                }

            }

        }  

        public static List<Decomposition> Compose(Decomposition decomp)
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

                // zip to dict
                var varDict = EnumerableExtension.Zip(termStringList, constantStringList).ToDictionary(x=> x.Key, x=> x.Value);

                // Need to propagate bindings to sub-steps
                foreach (var substep in decompClone.SubSteps)
                {
                    var op = substep.Action as Operator;
                    foreach (var term in substep.Terms)
                    {
                        op.AddBinding(term.Variable, varDict[term.Variable]);
                    }
                }

                // should add to decompList in place.
                FilterDecompCandidates(decompList, decompClone);
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
            var newSubsteps = new List<IPlanStep>();
            foreach (var substep in SubSteps)
            {
                newSubsteps.Add(substep.Clone() as IPlanStep);
            }
            // do same for literals
            return new Decomposition(this as IOperator, Literals, newSubsteps, SubOrderings, SubLinks);
        }

    }
}
