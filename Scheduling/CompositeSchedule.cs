using BoltFreezer.Camera;
using BoltFreezer.DecompTools;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BoltFreezer.Scheduling
{
    [Serializable]
    public class CompositeSchedule : Composite, IComposite, ICompositeSchedule
    {
        public List<Tuple<IPlanStep, IPlanStep>> Cntgs;

        public int NumberSegments
        {
            get
            {
                int count = 0;
                foreach (var substep in subSteps)
                {
                    if (substep is CamPlanStep cps)
                    {
                        count = count + cps.TargetDetails.ActionSegs.Count;
                    }
                }
                return count;
            }
        }

        public int NumberCamSteps
        {
            get
            {
                int count = 0;
                foreach (var substep in subSteps)
                {
                    if (substep is CamPlanStep cps)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        //public List<ContextPredicate> ContextPrecons { get; set; }
        //public List<ContextPredicate> ContextEffects { get; set; }

        public ActionSeg InitialActionSeg { get; set; }
        public ActionSeg FinalActionSeg { get; set; }
        public CamPlanStep InitialCamAction { get; set; }
        public CamPlanStep FinalCamAction { get; set; }
        public IPlanStep InitialAction { get; set; }
        public IPlanStep FinalAction { get; set; }

        // used to create root 
        public CompositeSchedule(IOperator op) : base(op)
        {
            Cntgs = new List<Tuple<IPlanStep, IPlanStep>>();
        }

        public CompositeSchedule(Composite comp) : base(comp, comp.InitialStep, comp.GoalStep, comp.SubSteps, comp.SubOrderings, comp.SubLinks)
        {
            Cntgs = new List<Tuple<IPlanStep, IPlanStep>>();
        }

        public CompositeSchedule(Composite comp, List<Tuple<IPlanStep, IPlanStep>> cntgs) : base(comp, comp.InitialStep, comp.GoalStep, comp.SubSteps, comp.SubOrderings, comp.SubLinks)
        {
            Cntgs = cntgs;
        }

        /// <summary>
        /// The compositeschedule has terms, preconditions, and effects. 
        /// All preconditions and effects are expected to be ground because they are created based on the ground decomposition
        /// Thus, unlike the parent class, there is no need to propagate bindings to terms, preconditions, and effects.
        /// </summary>
        /// <param name="td"></param>
        public void ApplyDecomposition(TimelineDecomposition td)
        {
            subSteps = td.SubSteps;
            subOrderings = td.SubOrderings;
            subLinks = td.SubLinks;

            foreach (var substep in subSteps)
            {
                foreach (var term in substep.Terms)
                {
                    if (!td.Terms.Contains(term))
                    {
                        //var termAsPredicate = term as Predicate;
                        //if (termAsPredicate != null)
                        //{

                        //}
                        Terms.Add(term);
                    }
                }
            }

            Cntgs = td.fabCntgs;

            // The way things are done round here is just to group in discourse stuff with fabula stuff. We have two plans... but they can go in one plan.
            foreach (var camplanstep in td.discourseSubSteps)
            {
                SubSteps.Add(camplanstep as IPlanStep);
            }
            foreach (var dordering in td.discOrderings)
            {
                SubOrderings.Add(new Tuple<IPlanStep, IPlanStep>(dordering.First, dordering.Second));
            }
            foreach (var discCntg in td.discCntgs)
            {
                Cntgs.Add(new Tuple<IPlanStep, IPlanStep>(discCntg.First, discCntg.Second));
            }
            foreach (var dlink in td.discLinks)
            {
                SubLinks.Add(new CausalLink<IPlanStep>(dlink.Predicate, dlink.Head, dlink.Tail));
            }

            // these should already be ground.
            InitialActionSeg = td.InitialActionSeg.Clone();
            FinalActionSeg = td.FinalActionSeg.Clone();
            InitialAction = td.InitialAction.Clone() as IPlanStep;
            FinalAction = td.FinalAction.Clone() as IPlanStep;
            InitialCamAction = td.InitialCamAction.Clone() as CamPlanStep;
            FinalCamAction = td.FinalCamAction.Clone() as CamPlanStep;

        }

        public new System.Object Clone()
        {
            var CompositeBase = base.Clone() as Composite;
            var newCntgs = new List<Tuple<IPlanStep, IPlanStep>>();
            foreach (var cntg in Cntgs)
            {
                newCntgs.Add(cntg);
            }
            var theClone = new CompositeSchedule(CompositeBase, newCntgs)
            {
                InitialActionSeg = InitialActionSeg.Clone(),
                FinalActionSeg = FinalActionSeg.Clone(),
                InitialAction = InitialAction.Clone() as IPlanStep,
                FinalAction = FinalAction.Clone() as IPlanStep,
                InitialCamAction = InitialCamAction.Clone() as CamPlanStep,
                FinalCamAction = FinalCamAction.Clone() as CamPlanStep
            };
            return theClone;
        }
    }
}