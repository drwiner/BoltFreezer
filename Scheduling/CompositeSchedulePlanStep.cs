using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System.Collections;
using System.Collections.Generic;

using System;
using BoltFreezer.Camera;
using System.Linq;
using BoltFreezer.DecompTools;

namespace BoltFreezer.Scheduling {

    // An instantiation of CompositeSchedule
	[Serializable]
    public class CompositeSchedulePlanStep : CompositePlanStep, ICompositePlanStep, ICompositeSchedule
    {


        public List<CamPlanStep> CamScheduleSubSteps
        {
            get
            {
                return SubSteps.OfType<CamPlanStep>().ToList();
            }
        }

        // has cntgs in addition to sub orderings
        public List<Tuple<IPlanStep, IPlanStep>> Cntgs
        {
            get
            {
                return cntgs;
            }
            set
            {
                cntgs = value;
            }
        }
        protected List<Tuple<IPlanStep, IPlanStep>> cntgs;

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

        public ActionSeg InitialActionSeg
        {
            get; set;
        }
        public ActionSeg FinalActionSeg
        {
            get; set;
        }
        public CamPlanStep InitialCamAction { get; set; }
        public CamPlanStep FinalCamAction { get; set; }
        public IPlanStep InitialAction { get; set; }
        public IPlanStep FinalAction { get; set; }

        List<Tuple<IPlanStep, IPlanStep>> IComposite.SubOrderings
        {
            get => SubOrderings;
            set
            {
                subOrderings = value;
            }
        }

        List<CausalLink<IPlanStep>> IComposite.SubLinks {
            get => SubLinks;
            set
            {
                subLinks = value;
            }
        }


        public CompositeSchedulePlanStep(CompositeSchedule comp) : base(comp as Composite)
        {
            Cntgs = comp.Cntgs;

            InitialActionSeg = comp.InitialActionSeg;
            FinalActionSeg = comp.FinalActionSeg;

            InitialAction = comp.InitialAction;
            FinalAction = comp.FinalAction;

            InitialCamAction = comp.InitialCamAction;
            FinalCamAction = comp.FinalCamAction;
            //ContextPrecons = comp.ContextPrecons;
            //ContextEffects = comp.ContextEffects;
        }

        public CompositeSchedulePlanStep(CompositeSchedulePlanStep comp) : base(comp as CompositePlanStep)
        {
            Cntgs = comp.Cntgs;
            InitialActionSeg = comp.InitialActionSeg;
            FinalActionSeg = comp.FinalActionSeg;

            InitialAction = comp.InitialAction;
            FinalAction = comp.FinalAction;

            InitialCamAction = comp.InitialCamAction;
            FinalCamAction = comp.FinalCamAction;
        }

        public CompositeSchedulePlanStep(IPlanStep ps) : base(ps)
        {
            if (ps is CompositeSchedule cs)
            {
                Cntgs = cs.Cntgs;
            }
            else
            {
                Cntgs = new List<Tuple<IPlanStep, IPlanStep>>();
            }
        }

        public CompositeSchedulePlanStep(Operator op) : base(new Composite(op))
        {

            Cntgs = new List<Tuple<IPlanStep, IPlanStep>>();
        }

        //public CompositeSchedulePlanStep(CompositePlanStep cps) : base(cps.CompositeAction, cps.OpenConditions, cps.InitialStep, cps.GoalStep, cps.SubSteps, cps.SubOrderings, cps.SubLinks)
        //{
        //    Cntgs = new List<Tuple<IPlanStep, IPlanStep>>();
        //}

        public CompositeSchedulePlanStep(CompositePlanStep cps, int id) : base(cps.CompositeAction, cps.OpenConditions, cps.InitialStep, cps.GoalStep, cps.SubSteps, cps.SubOrderings, cps.SubLinks, id)
        {
            Cntgs = new List<Tuple<IPlanStep, IPlanStep>>();
        }

        public CompositeSchedulePlanStep(CompositePlanStep cps, List<Tuple<IPlanStep, IPlanStep>> cntgs, int id) : base(cps.CompositeAction, cps.OpenConditions, cps.InitialStep, cps.GoalStep, cps.SubSteps, cps.SubOrderings, cps.SubLinks, id)
        {
            Cntgs = cntgs;
        }

        public CompositeSchedulePlanStep()
        {
        }

        public new System.Object Clone()
        {
            var cps = base.Clone() as CompositePlanStep;
            var newCntgs = new List<Tuple<IPlanStep, IPlanStep>>();
            foreach (var cntg in Cntgs)
            {
                // due dilligence
                newCntgs.Add(new Tuple<IPlanStep, IPlanStep>(cntg.First.Clone() as IPlanStep, cntg.Second.Clone() as IPlanStep));
            }
            return new CompositeSchedulePlanStep(cps, newCntgs, cps.ID)
            {
                
                InitialActionSeg = InitialActionSeg.Clone(),
                FinalActionSeg = FinalActionSeg.Clone(),
                InitialAction = InitialAction.Clone() as IPlanStep,
                FinalAction = FinalAction.Clone() as IPlanStep,
                InitialCamAction = InitialCamAction.Clone() as CamPlanStep,
                FinalCamAction = FinalCamAction.Clone() as CamPlanStep
            };
        }

    }
}