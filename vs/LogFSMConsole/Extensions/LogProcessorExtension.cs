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
    public static class LogProcessorExtension
    {
        public static void Process(this ILogFSM fsm, List<EventData> Data)
        {
            Console.Write(" (Process " + Data.Count + " events: ");
            for (int i=0; i< Data.Count; i++)
            {
                fsm.UpdateVariables(Data, i);
                fsm.ProcessEvent(Data, i);
                if (i % 100 == 0)
                    Console.Write(".");
            }
            Console.Write(")");


        }
    }
}
