using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;

namespace BoltFreezer.DecompTools
{
    public class Linked : IConstraint
    {
        private string name;
        private List<ITerm> terms;

        public string Name {
            get => name;
            set { name = value; }
        }

        public List<ITerm> Terms
        {
            get { return terms; }
            set { terms = value; }
        }

        public bool Check()
        {
            throw new NotImplementedException();
        }

        public IPlan Process(IPlan planToBuildOn)
        {
            return planToBuildOn;
        }
    }
}
