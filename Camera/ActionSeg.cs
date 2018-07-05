using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Camera
{
    [Serializable]
    public class ActionSeg
    {
        public string actionVarName = "";
        public string targetVarName = "";
        protected int actionID;
        public int actiontypeID;
        public double startPercent = 0;
        public double endPercent = 1;

        public ActionSeg(string actionvar, double start, double end)
        {
            actionVarName = actionvar;
            startPercent = start;
            endPercent = end;
        }

        public int ActionID
        {
            get { return actionID; }
            set { actionID = value; }
        }

        public ActionSeg()
        {
            // No target specified. This just represents free space. 
            // These should be inserted for each pair of targets that are consecutive but not contiguous.
        }

        public ActionSeg(string actionvar, int _actionID, int _actiontypeID, string tarvar, double start, double end)
        {
            actionVarName = actionvar;
            actionID = _actionID;
            actiontypeID = _actiontypeID;
            targetVarName = tarvar;
            startPercent = start;
            endPercent = end;
        }


        public bool CanReplace(ActionSeg other)
        {
            
            if (actiontypeID != other.actiontypeID)
            {
                return false;
            }

            if (other.startPercent < startPercent)
            {
                return false;
            }

            if (other.endPercent > endPercent)
            {
                return false;
            }

            return true;
        }

        public ActionSeg Clone()
        {
            return new ActionSeg(actionVarName, actionID, actiontypeID, targetVarName, startPercent, endPercent);
        }

    }
}
