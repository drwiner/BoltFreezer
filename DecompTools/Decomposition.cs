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
        protected IPlanStep initialStep;
        protected IPlanStep goalStep;
        protected List<Tuple<IPlanStep, IPlanStep>> subOrderings;
        protected List<CausalLink<IPlanStep>> subLinks;
        protected List<IPlanStep> subSteps;
        protected List<IPredicate> literals;

        public List<IPredicate> Literals
        {
            get { return literals; }
            set { literals = value; }
        }

        public IPlanStep InitialStep
        {
            get { return initialStep; }
            set { initialStep = value; }
        }

        public IPlanStep GoalStep
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
            initialStep = new PlanStep();
            goalStep = new PlanStep();
            literals = new List<IPredicate>();
        }

        public Decomposition(string name, List<ITerm> terms, IOperator init, IOperator dummy, List<IPredicate> Preconditions, List<IPredicate> Effects, int ID)
            : base(name, terms, new Hashtable(), Preconditions, Effects, ID)
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = new PlanStep(init);
            goalStep = new PlanStep(dummy);
        }

        public Decomposition(IOperator core, List<IPredicate> literals, List<IPlanStep> substeps, List<Tuple<IPlanStep, IPlanStep>> suborderings, List<CausalLink<IPlanStep>> sublinks)
            : base(core.Name, core.Terms, new Hashtable(), core.Preconditions, core.Effects, core.ID)
        {
            this.literals = literals;
            subOrderings = suborderings;
            subLinks = sublinks;
            subSteps = substeps;
            initialStep = new PlanStep();
            goalStep = new PlanStep();
        }

        public Decomposition(IOperator core, List<IPredicate> literals, IPlanStep init, IPlanStep goal, List<IPlanStep> substeps, List<Tuple<IPlanStep, IPlanStep>> suborderings, List<CausalLink<IPlanStep>> sublinks)
            : base(core.Name, core.Terms, new Hashtable(), core.Preconditions, core.Effects, core.ID)
        {
            this.literals = literals;
            subOrderings = suborderings;
            subLinks = sublinks;
            subSteps = substeps;
            initialStep = init;
            goalStep = goal;
        }


        /// <summary>
        /// The Decomposition is composed of a sub-plan with at least sub-step at height "height"
        /// </summary>
        /// <returns>A list of decompositions with ground terms and where each sub-step is ground. </returns>
        public List<Decomposition> Compose(int height)
        {
            ///////////////////////////////////////
            // START BY ADDING BINDINGS TO TERMS //
            ///////////////////////////////////////
            var permList = new List<List<string>>();
            foreach (Term variable in Terms)
            {
                permList.Add(GroundActionFactory.TypeDict[variable.Type] as List<string>);
            }

            var decompList = new List<Decomposition>();
            foreach (var combination in EnumerableExtension.GenerateCombinations(permList))
            {
                // Add bindings
                var decompClone = Clone() as Decomposition;
                var termStringList = from term in decompClone.Terms select term.Variable;
                var constantStringList = combination;

                decompClone.AddBindings(termStringList.ToList(), constantStringList.ToList());

                /////////////////////////////////////////////////////////
                // PROPAGATE BINDINGS TO NONEQUALITY CONSTRAINTS
                /////////////////////////////////////////////////////////
                var newNonEqualities = new List<List<ITerm>>();
                foreach (var nonequals in NonEqualities)
                {
                    var newNonEquals = new List<ITerm>();
                    newNonEquals.Add(decompClone.Terms.First(dterm => dterm.Variable.Equals(nonequals[0].Variable)));
                    newNonEquals.Add(decompClone.Terms.First(dterm => dterm.Variable.Equals(nonequals[1].Variable)));
                    newNonEqualities.Add(newNonEquals);
                }
                decompClone.NonEqualities = newNonEqualities;
                if (!decompClone.NonEqualTermsAreNonequal())
                {
                    continue;
                }

                // zip to dict
                var varDict = EnumerableExtension.Zip(termStringList, constantStringList).ToDictionary(x => x.Key, x => x.Value);


                /////////////////////////////////////////////////////////
                // BINDINGS ARE ADDED. NEED TO APPLY BINDINGS TO SUBSTEPS
                /////////////////////////////////////////////////////////

                // Need to propagate bindings to sub-steps
                foreach (var substep in decompClone.SubSteps)
                {
                    var op = substep.Action as Operator;
                    foreach (var term in substep.Terms)
                    {
                        op.AddBinding(term.Variable, varDict[term.Variable]);
                    }
                    foreach (var precon in substep.Preconditions)
                    {
                        foreach (var term in precon.Terms)
                        {
                            if (!term.Bound)
                            {
                                var decompTerm = decompClone.Terms.First(dterm => dterm.Variable.Equals(term.Variable));
                                op.Terms.Add(term);
                                op.AddBinding(term.Variable, decompTerm.Constant);
                            }
                        }
                    }
                    foreach (var eff in substep.Effects)
                    {
                        foreach (var term in eff.Terms)
                        {
                            if (!term.Bound)
                            {
                                var decompTerm = decompClone.Terms.First(dterm => dterm.Variable.Equals(term.Variable));
                                op.Terms.Add(term);
                                op.AddBinding(term.Variable, decompTerm.Constant);
                            }
                        }
                    }
                }

                

                ////////////////////////////////////////////////////////////////
                // FILTER CANDIDATES FOR SUBSTEPS AND PASS BACK GROUNDED DECOMPS
                ////////////////////////////////////////////////////////////////
                var newGroundDecomps = FilterDecompCandidates(decompClone, height);
                foreach (var gdecomp in newGroundDecomps)
                {
                    decompList.Add(gdecomp);
                }
                //Console.WriteLine("Check");
            }

            return decompList;
        }

        /// <summary>
        /// Filters candidates for substeps, at least one with height "height"
        /// </summary>
        /// <param name="decomp"></param>
        /// <returns> List of decompositions with ground sub-steps. </returns>
        public static List<Decomposition> FilterDecompCandidates(Decomposition decomp, int height)
        {
            // find and replace sub-steps 
            var comboList = new List<List<IOperator>>();
            var ID_List = new List<int>();
            foreach (var substep in decomp.SubSteps)
            {
                ID_List.Add(substep.ID);
                // each substep has ground terms that are already consistent. Composite IS-A Operator
                var cndts = ConsistentSteps(substep.Action as Operator);

                // If there's no cndts for this substep, then abandon this decomp.
                if (cndts.Count == 0)
                    return new List<Decomposition>();

                comboList.Add(cndts);
            }

            List<Decomposition> decompList = new List<Decomposition>();
            foreach (var combination in EnumerableExtension.GenerateCombinations(comboList))
            {
                var decompClone = decomp.Clone() as Decomposition;
                var newSubsteps = new List<IPlanStep>();
                var substepDict = new Dictionary<int, IPlanStep>();
                var order = 0;
                var hasPrerequisiteHeight = false;
                foreach (var item in combination)
                {
                    if (item.Height >= height)
                    {
                        // meets height requirement
                        hasPrerequisiteHeight = true;
                    }
                    var originalID = ID_List[order++];
                    if (item.Height > 0)
                    {
                        var newPlanStep = new CompositePlanStep(item as Composite);
                        substepDict[originalID] = newPlanStep;
                        newSubsteps.Add(newPlanStep);
                    }
                    else
                    {
                        var newPlanStep = new PlanStep(item);
                        substepDict[originalID] = newPlanStep;
                        newSubsteps.Add(newPlanStep);
                    }
                }

                // Did not meet requirements for height.
                if (!hasPrerequisiteHeight)
                {
                    continue;
                }

                var newSuborderings = new List<Tuple<IPlanStep, IPlanStep>>();
                foreach (var subordering in decomp.SubOrderings)
                {
                    var first = substepDict[subordering.First.ID];
                    var second = substepDict[subordering.Second.ID];
                    newSuborderings.Add(new Tuple<IPlanStep, IPlanStep>(first, second));
                }

                var linkWorlds = new List<List<CausalLink<IPlanStep>>>();
                linkWorlds.Add(new List<CausalLink<IPlanStep>>());
                var newSublinks = new List<CausalLink<IPlanStep>>();
                foreach (var sublink in decomp.SubLinks)
                {
                    var head = substepDict[sublink.Head.ID];
                    var tail = substepDict[sublink.Tail.ID];
                    var cndts = head.Effects.Where(eff => eff.IsConsistent(sublink.Predicate) && tail.Preconditions.Any(pre=> pre.Equals(eff)));

                    //// swap tall members
                    //if (head.Height > 0)
                    //{
                    //    var Chead = head as CompositePlanStep;
                    //    head = Chead.GoalStep;
                    //}
                    //if (tail.Height > 0)
                    //{
                    //    var Ctail = tail as CompositePlanStep;
                    //    tail = Ctail.InitialStep;
                    //}

                    if (cndts.Count() == 0)
                    {
                        // forfeit this entire subplan
                        linkWorlds = new List<List<CausalLink<IPlanStep>>>();
                        continue;
                    }
                    if (cndts.Count() == 1)
                    {
                        var cndt = cndts.First();
                        var dependency = cndt.Clone() as Predicate;
                        var newLink = new CausalLink<IPlanStep>(dependency, head, tail);
                        newLink.Tail.Fulfill(cndt);
                        foreach( var linkworld in linkWorlds)
                        {
                            linkworld.Add(newLink);
                        }
                    }
                    else
                    {
                        foreach (var cndt in cndts)
                        {
                            var dependency = cndt.Clone() as Predicate;

                            var newLink = new CausalLink<IPlanStep>(dependency, head, tail);
                            newLink.Tail.Fulfill(cndt);

                            var clonedLinks = EnumerableExtension.CloneList(newSublinks);

                            linkWorlds.Add(clonedLinks);
                            foreach (var linkworld in linkWorlds)
                            {
                                linkworld.Add(newLink);
                            }
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
            return decompList;

        }

        /// <summary>
        /// Finds a set of ground Operators that are consistent with this substep
        /// </summary>
        /// <param name="substep"></param>
        /// <returns></returns>
        public static List<IOperator> ConsistentSteps(Operator substep)
        {
            
            var Cndts = GroundActionFactory.GroundActions as IEnumerable<IOperator>;

            if (!substep.Name.Equals(""))
                Cndts = FilterOperatorsByPredicate(substep, Cndts);
            Cndts = FilterOperatorsByPreconditionsAndEffects(substep, Cndts);

            return Cndts.ToList();
        }

        public static IEnumerable<IOperator> FilterOperatorsByPredicate(Operator substep, IEnumerable<IOperator> Cndts)
        {
            return Cndts.Where(op => op.Predicate.IsConsistent(substep.Predicate));
        }

        public static IEnumerable<IOperator> FilterOperatorsByPreconditionsAndEffects(Operator substep, IEnumerable<IOperator> Cndts)
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

        public static IEnumerable<IOperator> FilterOperatorsByPrecondition(Predicate precon, IEnumerable<IOperator> Cndts)
        {
            return Cndts.Where(op => op.Preconditions.Any(pre => pre.IsConsistent(precon)));
        }

        public static IEnumerable<IOperator> FilterOperatorsByEffect(Predicate effect, IEnumerable<IOperator> Cndts)
        {
            return Cndts.Where(op => op.Effects.Any(pre => pre.IsConsistent(effect)));
        }

        public new Object Clone()
        {
            var op = base.Clone() as IOperator;
            var newSubsteps = new List<IPlanStep>();

            foreach (var substep in SubSteps)
            {
                var newsubstep = substep.Clone() as IPlanStep;
                newsubstep.Action = newsubstep.Action.Clone() as Operator;
                newSubsteps.Add(newsubstep);
            }

          //  var newinitial = InitialStep.Clone() as IPlanStep;
            //newinitial.Action = InitialStep.Action.Clone() as Operator;
            // do same for literals
            return new Decomposition(op, Literals, InitialStep.Clone() as IPlanStep, GoalStep.Clone() as IPlanStep, newSubsteps, SubOrderings.ToList(), SubLinks.ToList());
           // return new Decomposition(op, Literals, newSubsteps, SubOrderings, SubLinks);
        }

    }
}
