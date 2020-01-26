namespace LogDataTransformer_NEPS_V01
{
    #region usings
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml.Serialization;
    #endregion

    public class TBAToolsLog { }
    public class TBAToolsTestStart : TBAToolsLog { }
    public class TBAToolsLotStart : TBAToolsLog { }
    public class TBAToolsLogin : TBAToolsLog { }
    public class TBAToolsLogout : TBAToolsLog { }
    public class TBAToolsNextItem : TBAToolsLog { }
    public class TBAToolsPreviousItem : TBAToolsLog { }
    public class TBAToolsNextItemWhileLocked : TBAToolsLog { }
    public class TBAToolsTestRestart : TBAToolsLog { }
    public class TBAToolsEndOfSequence : TBAToolsLog { }
    public class TBAToolsItemNotFinished : TBAToolsLog { }
    public class TBAToolsRealTime : TBAToolsLog {[XmlAttribute] public DateTime RealTime { get; set; } }
    public class TBAToolsLoading : TBAToolsLog {[XmlAttribute] public string Sender { get; set; } }
    public class TBAToolsLoaded : TBAToolsLog {[XmlAttribute] public string Sender { get; set; } }
    public class TBAToolsUnloading : TBAToolsLog {[XmlAttribute] public string Sender { get; set; } }
    public class TBAToolsUnloaded : TBAToolsLog {[XmlAttribute] public string Sender { get; set; } }
    public class TBAToolsIBStopTask : TBAToolsLog {[XmlAttribute] public string Sender { get; set; } }
    public class TTLogRestart : TBAToolsLog {[XmlAttribute] public string Sender { get; set; } }
    public class TBAToolsIBLoadedAgain : TBAToolsLog {[XmlAttribute] public string Sender { get; set; } }
    public class TBAToolsIBReceivedNextTask : TBAToolsLog {[XmlAttribute] public string Sender { get; set; } }
    public class TBAToolsIBReceivedStopTask : TBAToolsLog {[XmlAttribute] public string Sender { get; set; } }
    public class TBAToolsVariableChanged : TBAToolsLog {[XmlAttribute] public string Sender { get; set; }[XmlAttribute] public string Variable { get; set; }[XmlAttribute] public string Value { get; set; }[XmlAttribute] public string ValueLabel { get; set; } }
    public class TBAToolsClientInfo : TBAToolsLog {[XmlAttribute] public string Sender { get; set; }[XmlAttribute] public int ScreenWidth { get; set; }[XmlAttribute] public int ScreenHeight { get; set; }[XmlAttribute] public int WindowWidth { get; set; }[XmlAttribute] public int WindowHeight { get; set; } }

}
