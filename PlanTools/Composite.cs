using BoltFreezer.DecompTools;
using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class Composite : Operator, IComposite
    {
        protected IPlanStep initialStep;
        protected IPlanStep goalStep;
        protected List<Tuple<IPlanStep, IPlanStep>> subOrderings;
        protected List<CausalLink<IPlanStep>> subLinks;
        protected List<IPlanStep> subSteps;
        protected List<IPredicate> primaryEffects;

        public List<IPredicate> PrimaryEffects
        {
            get { return primaryEffects; }
            set { primaryEffects = value; }
        }

        public IPlanStep InitialStep {
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
            get { return subSteps;  }
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

        public Composite() : base()
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = new PlanStep();
            goalStep = new PlanStep();
        }

        public Composite(string name, List<ITerm> terms, IPlanStep init, IPlanStep goal, List<IPredicate> Preconditions, List<IPredicate> Effects, int ID) 
            : base(name, terms, new Hashtable(), Preconditions, Effects, ID)
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = init;
            goalStep = goal;
        }

        public Composite(IOperator core, IPlanStep init, IPlanStep goal, List<IPlanStep> substeps, List<Tuple<IPlanStep, IPlanStep>> suborderings, List<CausalLink<IPlanStep>> sublinks)
            : base(core.Name, core.Terms, new Hashtable(), core.Preconditions, core.Effects, core.ID)
        {
            subOrderings = suborderings;
            subLinks = sublinks;
            subSteps = substeps;
            initialStep = init;
            goalStep = goal;
            Height = core.Height;
        }

        public Composite(IOperator core) : base(core.Name, core.Terms, new Hashtable(), core.Preconditions, core.Effects, core.ID)
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = new PlanStep(new Operator("DummyInit", new List<IPredicate>(), core.Preconditions));
            goalStep = new PlanStep(new Operator("DummyGoal", core.Effects, new List<IPredicate>()));
            Height = core.Height;
            NonEqualities = core.NonEqualities;
        }

        public int ApplyDecomposition(Decomposition decomp)
        {
            var numUnBoundArgs = 0;

            subSteps = decomp.SubSteps;
            subOrderings = decomp.SubOrderings;
            subLinks = decomp.SubLinks;

            // For each variable term, find and substitute decomp term
            foreach (var term in Terms)
            {
                var decompTerm = decomp.Terms.FirstOrDefault(dterm => term.Variable.Equals(dterm.Variable));
                if (decompTerm == null)
                {
                    numUnBoundArgs++;
                    // need to pick some object to substitute. Any will do.
                    continue;

                }
                AddBinding(term.Variable, decompTerm.Constant);
            }

            // Check Equality Constraints
            if (!NonEqualTermsAreNonequal()){
                return -1;
            }

            // Also add bindings to Initial and Goal step.
            foreach (var effect in InitialStep.Effects)
            {
                foreach (var term in effect.Terms)
                {
                    var compTerm = Terms.FirstOrDefault(cterm => term.Variable.Equals(cterm.Variable));
                    if (compTerm == null)
                    {
                        throw new System.Exception();
                    }
                    term.Constant = compTerm.Constant;
                }
            }
            foreach (var precon in GoalStep.Preconditions)
            {
                foreach (var term in precon.Terms)
                {
                    var compTerm = Terms.FirstOrDefault(cterm => term.Variable.Equals(cterm.Variable));
                    if (compTerm == null)
                    {
                        throw new System.Exception();
                    }
                    term.Constant = compTerm.Constant;
                }
            }
            var unlistedDecompTerms = decomp.Terms.Where(dt => !Terms.Any(t => dt.Equals(t)));
            foreach (var udt in unlistedDecompTerms)
            {
                Terms.Add(udt);
            }

            //foreach(var substep in SubSteps)
            //{
            //    foreach(var precon in substep.Preconditions)
            //    {
            //        if (substep.OpenConditions.Contains(precon))
            //        {
            //            if (InitialStep.Effects.Contains(precon))
            //            {
            //                SubLinks.Add(new CausalLink<IPlanStep>(precon, InitialStep, substep));
            //            }
            //        }
            //    }
            //}

            return numUnBoundArgs;
        }

        public void RemoveRemainingArgs()
        {
            var newTerms = Terms.Where(term => term.Bound);
            var newPrecons = Preconditions.Where(precon => !precon.Terms.Any(preconTerm => !preconTerm.Bound));
            var newEffects = Effects.Where(eff => !eff.Terms.Any(effTerm => !effTerm.Bound));
            Terms = newTerms.ToList();
            Preconditions = newPrecons.ToList();
            Effects = newEffects.ToList();

        }

        public List<Composite> GroundRemainingArgs(int numUnBound)
        {
            
            var compList = new List<Composite>();

            foreach (var term in Terms)
            {
                if (term.Bound)
                {
                    continue;
                }


                // This strategy is to assign to arbitrary but consistent object
                if (numUnBound == 1)
                {

                    var legalSubstitutions = GroundActionFactory.TypeDict[term.Type] as List<string>;
                    foreach (var legalSub in legalSubstitutions)
                    {
                        var compClone = Clone() as Composite;
                        compClone.AddBinding(term.Variable, legalSub);

                        if (!compClone.NonEqualTermsAreNonequal())
                            continue;

                        // find this unbound arg in Initial and Goal steps
                        foreach (var effect in compClone.InitialStep.Effects)
                        {
                            var unboundTerms = effect.Terms.Where(t => !t.Bound);
                            foreach (var unboundTerm in unboundTerms)
                            {
                                var compTerm = compClone.Terms.FirstOrDefault(cterm => unboundTerm.Variable.Equals(cterm.Variable));
                                unboundTerm.Constant = compTerm.Constant;
                            }
                        }
                        foreach (var precon in compClone.GoalStep.Preconditions)
                        {
                            var unboundTerms = precon.Terms.Where(t => !t.Bound);
                            foreach (var unboundTerm in unboundTerms)
                            {
                                var compTerm = compClone.Terms.FirstOrDefault(cterm => unboundTerm.Variable.Equals(cterm.Variable));
                                unboundTerm.Constant = compTerm.Constant;
                            }
                        }

                        compList.Add(compClone);
                    }
                }
                else
                {
                    if (compList.Count == 0)
                    {
                        compList.Add(this);
                    }
                    var legalSubstitutions = GroundActionFactory.TypeDict[term.Type] as List<string>;
                    var newComps = new List<Composite>();
                    foreach (var existingUnBoundComp in compList)
                    {
                        foreach (var legalSub in legalSubstitutions)
                        {
                            var compClone = existingUnBoundComp.Clone() as Composite;
                            compClone.AddBinding(term.Variable, legalSub);

                            if (!compClone.NonEqualTermsAreNonequal())
                                continue;

                            // find this unbound arg in Initial and Goal steps
                            foreach (var effect in compClone.InitialStep.Effects)
                            {
                                var unboundTerms = effect.Terms.Where(t => !t.Bound);
                                foreach (var unboundTerm in unboundTerms)
                                {
                                    var compTerm = compClone.Terms.FirstOrDefault(cterm => unboundTerm.Variable.Equals(cterm.Variable));
                                    unboundTerm.Constant = compTerm.Constant;
                                }
                            }
                            foreach (var precon in compClone.GoalStep.Preconditions)
                            {
                                var unboundTerms = precon.Terms.Where(t => !t.Bound);
                                foreach (var unboundTerm in unboundTerms)
                                {
                                    var compTerm = compClone.Terms.FirstOrDefault(cterm => unboundTerm.Variable.Equals(cterm.Variable));
                                    unboundTerm.Constant = compTerm.Constant;
                                }
                            }

                            newComps.Add(compClone);
                        }
                    }
                    // overwrite with members that have one more bound item
                    compList = newComps;

                }
            }

            return compList;
        }

        public static void ComposeHTNs(int hMax, Dictionary<Composite, List<Decomposition>> Methods)
        {
            // for each height to ground a composite operator
            for (int h = 0; h < hMax; h++)
            {
                // For each (composite operator, method) pair
                foreach (var compositepair in Methods)
                {
                    // Create a new list of composites that will be assembled in this iteration
                    var compList = new List<Composite>();

                    // for each legal decomposition of this composite operator
                    foreach (var decomp in compositepair.Value)
                    {
                        // Create a list of ground decompositions
                        var groundDecomps = decomp.Compose(h);

                        // for each ground decomposition
                        foreach (var gdecomp in groundDecomps)
                        {
                            // clone composite task
                            var comp = compositepair.Key.PseudoClone();

                            // Set height of composite step
                            comp.Height = h + 1;

                            // Assign method to composite step
                            var numUnBound = comp.ApplyDecomposition(gdecomp);

                            // If all terms are bound, then add as is.
                            if (numUnBound == 0)
                            {
                                compList.Add(comp);
                            }
                            // Otherwise, bind the remaining unbound terms
                            else if (numUnBound > 0)
                            {
                                // Remove unbound args
                                comp.RemoveRemainingArgs();
                                compList.Add(comp);
                            }
                        }
                    }
                    // For each newly created composite step, add to the library.
                    foreach (var comp in compList)
                    {
                        GroundActionFactory.InsertOperator(comp as IOperator);
                    }
                }

            }

        }

        public Composite PseudoClone()
        {
            var newPreconds = new List<IPredicate>();
            foreach(var precon in base.Preconditions)
            {
                newPreconds.Add(precon.Clone() as IPredicate);
            }
            var newEffects = new List<IPredicate>();
            foreach(var eff in base.Effects)
            {
                newEffects.Add(eff.Clone() as IPredicate);
            }

            var newBase = new Operator(base.Predicate.Clone() as Predicate, newPreconds, newEffects);
            return new Composite(newBase, InitialStep.Clone() as IPlanStep, GoalStep.Clone() as IPlanStep, SubSteps, SubOrderings, SubLinks)
            {
                Height = this.Height,
                NonEqualities = this.NonEqualities
            };
        }

        public new Object Clone()
        {
            var op = base.Clone() as IOperator;
            var init = InitialStep.Clone() as IPlanStep;
            var goal = GoalStep.Clone() as IPlanStep;

            return new Composite(op, init, goal, SubSteps, SubOrderings, SubLinks)
            {
                Height = this.Height,
                NonEqualities = this.NonEqualities
            };
        }
    }
}
