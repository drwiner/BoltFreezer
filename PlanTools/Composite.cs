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
    public class Composite : Operator, IComposite
    {
        private IOperator initialStep;
        private IOperator goalStep;
        private List<Tuple<IPlanStep, IPlanStep>> subOrderings;
        private List<CausalLink<IPlanStep>> subLinks;
        private List<IPlanStep> subSteps;
        private List<IPredicate> primaryEffects;

        public List<IPredicate> PrimaryEffects
        {
            get { return primaryEffects; }
            set { primaryEffects = value; }
        }

        public IOperator InitialStep {
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
            initialStep = new Operator();
            goalStep = new Operator();
        }

        public Composite(string name, List<ITerm> terms, IOperator init, IOperator goal, List<IPredicate> Preconditions, List<IPredicate> Effects, int ID) 
            : base(name, terms, new Hashtable(), Preconditions, Effects, ID)
        {
            subOrderings = new List<Tuple<IPlanStep, IPlanStep>>();
            subLinks = new List<CausalLink<IPlanStep>>();
            subSteps = new List<IPlanStep>();
            initialStep = init;
            goalStep = goal;
        }

        public Composite(IOperator core, IOperator init, IOperator goal, List<IPlanStep> substeps, List<Tuple<IPlanStep, IPlanStep>> suborderings, List<CausalLink<IPlanStep>> sublinks)
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
            initialStep = new Operator("DummyInit", new List<IPredicate>(), core.Preconditions);
            goalStep = new Operator("DummyGoal", core.Effects, new List<IPredicate>());
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
            //var unlistedDecompTerms = decomp.Terms.Where(dt => !Terms.Any(t => dt.Equals(t)));
            //foreach (var udt in unlistedDecompTerms)
            //{
            //    Terms.Add(udt);
            //}

            return numUnBoundArgs;
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

                if (numUnBound == 1)
                {

                    var legalSubstitutions = GroundActionFactory.TypeDict[term.Type] as List<IObject>;
                    foreach (var legalSub in legalSubstitutions)
                    {
                        var compClone = Clone() as Composite;
                        compClone.AddBinding(term.Variable, legalSub.Name);
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
                    var legalSubstitutions = GroundActionFactory.TypeDict[term.Type] as List<IObject>;
                    var newComps = new List<Composite>();
                    foreach (var existingUnBoundComp in compList)
                    {
                        foreach (var legalSub in legalSubstitutions)
                        {
                            var compClone = existingUnBoundComp.Clone() as Composite;
                            compClone.AddBinding(term.Variable, legalSub.Name);
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

        public static void ComposeHTNs(int hMax, Tuple<Composite, List<Decomposition>> Methods)
        {
            for (int h = 0; h < hMax; h++)
            {
                var compList = new List<Composite>();
                foreach (var decomp in Methods.Second)
                {
                    var groundDecomps = decomp.Compose(h);

                    foreach (var gdecomp in groundDecomps)
                    {
                        // clone composite task
                        var comp = Methods.First.Clone() as Composite;

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
                        else
                        {
                            // There could be more than one way to bind remaining terms
                            var boundComps = comp.GroundRemainingArgs(numUnBound);
                            foreach (var bc in boundComps)
                            {
                                // Add each possible way to bind remaining terms
                                compList.Add(bc);
                            }
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

        public new Object Clone()
        {
            var op = base.Clone() as IOperator;
            var init = InitialStep.Clone() as IOperator;
            var goal = GoalStep.Clone() as IOperator;

            return new Composite(op, init, goal, SubSteps, SubOrderings, SubLinks)
            {
                Height = this.Height,
                NonEqualities = this.NonEqualities
            };
        }
    }
}
