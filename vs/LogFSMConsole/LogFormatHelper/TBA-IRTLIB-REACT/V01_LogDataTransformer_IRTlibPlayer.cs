namespace LogDataTransformer_IRTlibPlayer_V01
{
    #region usings
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    public enum KeyStrokeDirection
    {
        Up,
        Down,
    }

    public class KeyStroke
    {
        public string Key { get; set; }
        public DateTime Timestamp { get; set; }
        public KeyStrokeDirection Direction { get; set; }
    }

    public enum MouseEventType
    {
        Move,
        Down,
        Scroll,
        ScrollStart,
    }

    public class MouseEvent
    {
        public int X { get; set; }
        public int Y { get; set; }
        public DateTime Timestamp { get; set; }
        public MouseEventType Type { get; set; }
    }


    public class SessionNavigationState
    {
        /// <summary>
        /// Gets or sets the booklet.
        /// </summary>
        public string Booklet { get; set; }

        /// <summary>
        /// Gets or sets the test.
        /// </summary>
        public string Test { get; set; }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        public string Task { get; set; }

        /// <summary>
        /// Gets or sets the preview.
        /// </summary>
        public string Preview { get; set; }


        public override string ToString() => $"{Booklet} - {Test} - {Item} - {Task}";
    }

    public class EventBase
    {
        public DateTime Timestamp { get; set; }

        public string SessionId { get; set; }
 
        public int TraceID { get; set; }

        public SessionNavigationState Context { get; set; }

    }

    public class TraceEvent : EventBase
    {
        public string Trace { get; set; }
    }

    public class TraceLog : EventBase
    { 
        public string Sender { get; set; }
        public string Trigger { get; set; }
        public string Log { get; set; } 
         
    }

    public class json_IRTlib_V01__ItemScore
    {
        public DateTime Timestamp { get; set; }
        public string ItemScore { get; set; }
        public string SessionId { get; set; }

        public SessionNavigationState Context { get; set; }
    }

    public class json_IRTLib_V01__TokenLis
    { 
        public string sessionId { get;  set; }
        public DateTime lastUpdate { get; set; }
    }



}
