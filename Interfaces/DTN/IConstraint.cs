using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.DecompTools
{
    public interface IConstraint
    {
        // name of the constraint
        string Name { get; set; }

        // The Constraint should have a constructor and a field for the items to add

        // Send in a plan and return modified plan. This object's fields (arguments) are already in the planToBuildOn.
        IPlan Process(IPlan planToBuildOn);

        // Checks if the field's arguments can take on the constraint described. False if it cannot.
        bool Check();

        ///
        // Example
        ///
        // Linked (s1, s2, p)
        // planToBuildOn includes s1 and s2.
        // Check evaluates if s1 and s2 can take on causalLink(s1, s2, p)
        // Process creates a causal link in planToBuildOn and passes back the result.
        
    }
}
