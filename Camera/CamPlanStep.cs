using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BoltFreezer.Camera {

    [Serializable]
    public class CamPlanStep : PlanStep, IPlanStep {

        // Initially used to reference prerequisite criteria.
        public CamSchema CamDetails = null;

        // Only assigned once it is grounded plan step.
        public string CamObject = null;

        // A totally and temporally ordered list of action segments
        public CamTargetSchema TargetDetails = null;

        public CamPlanStep(IOperator groundAction) : base(groundAction)
        {
        }

        public CamPlanStep(PlanStep planStep) : base(planStep)
        {
        }

        public new CamPlanStep Clone()
        {
            var newstep = new CamPlanStep(base.Clone() as PlanStep);
            if (CamDetails != null)
                newstep.CamDetails = CamDetails.Clone();
            if (TargetDetails != null)
                newstep.TargetDetails = TargetDetails.Clone();

            return newstep;
        }
    }
}
