#region usings
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion


namespace LogFSM
{
    public class LogFSMResultJSON
    {
        public Dictionary<string, DataTable> Tables { get; set; }

        public LogFSMResultJSON()
        {
            Tables = new Dictionary<string, DataTable>();
        }

    }
}
