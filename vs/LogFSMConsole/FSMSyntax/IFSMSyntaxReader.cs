#region usings
using LogFSMShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace LogFSM
{        
    public interface IFSMSyntaxReader
    {
        List<FSMIgnore> Ignores { get; }
        Dictionary<string, FSMOperator> Operators { get; }
        List<FSMTransition> Transitions { get; }
        Dictionary<int, List<string>> States { get; }
        //string[] States { get; }
        Dictionary<int, string> Start { get; }
        Dictionary<int, string[]> End { get; }
        FSMTrigger[] Triggers { get; }
        List<string> Errors { get; }
        int NumberOfMachines { get; }
    }
}
