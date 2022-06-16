using System;
using OpenXesNet.util;

namespace OpenXesNet.io
{
    /// <summary>
    /// System-wide registry for XES parser implementations. Applications can use 
    /// this registry as a convenience to provide an overview about parseable 
    /// formats, e.g., in the user interface.
    /// Any custom parser implementation can be registered with this registry, so 
    /// that it transparently becomes available also to any other using application.
    /// </summary>
    public class XParserRegistry : XRegistry<XParser>
    {
        /// <summary>
        /// Singleton registry instance.
        /// </summary>
        static XParserRegistry singleton = new XParserRegistry();

        /// <summary>
        ///  Retrieves the singleton registry instance.
        /// </summary>
        /// <value>The instance.</value>
        public static XParserRegistry Instance
        {
            get { return singleton; }
        }

        /// <summary>
        /// Creates the singleton.
        /// </summary>
        XParserRegistry() 
        {
            //Register(new XMxmlParser());
            //register(new XMxmlGZIPParser());
            Register(new XesXmlParser());
            CurrentDefault = new XesXmlGzipParser();
        }

        protected override bool AreEqual(XParser instance1, XParser instance2)
        {
            return instance1.GetType().Equals(instance2.GetType());
        }
    }
}
