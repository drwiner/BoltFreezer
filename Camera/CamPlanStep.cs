using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using System.Collections;
using System.Collections.Generic;
using System;
using BoltFreezer.Camera.CameraEnums;

namespace BoltFreezer.Camera {

    [Serializable]
    public class CamPlanStep : PlanStep, IPlanStep {

        // Initially used to reference prerequisite criteria.
        public CamSchema CamDetails = null;

        // Only assigned once it is grounded plan step.
        public string CamObject = null;

        // A totally and temporally ordered list of action segments
        public CamTargetSchema TargetDetails = null;

        //public CamDirective directive = CamDirective.None;

        public CamPlanStep() : base()
        {

        }

        public CamPlanStep(IOperator groundAction) : base(groundAction)
        {
        }

        public CamPlanStep(PlanStep planStep) : base(planStep)
        {
        }

        public CamPlanStep(PlanStep planStep, int _id) : base(planStep, _id)
        {
        }

        public CamPlanStep(CamPlanStep cps) : base(cps.Action)
        {
            if (cps.CamDetails != null)
                CamDetails = cps.CamDetails.Clone();
            if (cps.TargetDetails != null)
                TargetDetails = cps.TargetDetails.Clone();
            if (cps.CamObject != null)
                CamObject = cps.CamObject;
            //directive = cps.directive;
            
        }

        public void UpdateActionSegs(Dictionary<int, IPlanStep> updateList)
        {
            TargetDetails.SetActionSegTargets(updateList);
        }

        public new System.Object Clone()
        {
            var newstep = new CamPlanStep(base.Clone() as PlanStep, ID);
            if (CamDetails != null)
                newstep.CamDetails = CamDetails.Clone();
            if (TargetDetails != null)
                newstep.TargetDetails = TargetDetails.Clone();

            //newstep.directive = directive;

            return newstep;
        }
    }
}
