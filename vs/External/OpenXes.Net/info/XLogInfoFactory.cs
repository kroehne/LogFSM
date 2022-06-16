using OpenXesNet.model;
using OpenXesNet.classification;

namespace OpenXesNet.info
{
    /// <summary>
    /// Factory for deriving log info summaries from logs.
    /// </summary>
    public static class XLogInfoFactory
    {
        /// <summary>
        /// Creates a new log info with the standard event classifier.
        /// </summary>
        /// <returns>log info for this log.</returns>
        /// <param name="log">The event log to create an info summary for.</param>
        public static IXLogInfo CreateLogInfo(IXLog log)
        {
            return CreateLogInfo(log, XLogInfo.STANDARD_CLASSIFIER);
        }

        /// <summary>
        /// Creates a new log info summary with a custom event classifier.
        /// </summary>
        /// <returns>The log info summary for this log.</returns>
        /// <param name="log">The event log to create an info summary for.</param>
        /// <param name="classifier">The event classifier to be used.</param>
        public static IXLogInfo CreateLogInfo(IXLog log, IXEventClassifier classifier)
        {
            IXLogInfo info = log.GetInfo(classifier);
            if (info == null)
            {
                info = XLogInfo.Create((XLog)log, classifier);

                log.SetInfo(classifier, info);
            }
            return info;
        }
    }
}
