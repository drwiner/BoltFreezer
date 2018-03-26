using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.DecompTools
{
    public interface IDecomposition
    {
        // Sub-tasks
        List<ITask> SubTasks { get; set; }

        // Constraints for sub-task
        List<IConstraint> Constraints { get; set; }
    }
}
