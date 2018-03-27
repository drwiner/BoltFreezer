using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.DecompTools
{
    public class Decomposition : IDecomposition
    {
        public List<ITask> SubTasks { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<IConstraint> Constraints { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
