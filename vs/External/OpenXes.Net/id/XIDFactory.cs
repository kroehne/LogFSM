using System;
namespace OpenXesNet.id
{
    /// <summary>
    /// This class is a factory for unique identifiers, as they are used throughout 
    /// the XES model for element identification. Uses the singleton pattern.
    /// </summary>
    public class XIDFactory
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        static XIDFactory singleton = new XIDFactory();

        /// <summary>
        /// Accesses the singleton instance
        /// </summary>
        /// <value>Singleton ID factory.</value>
        public static XIDFactory Instance
        {
            get { return singleton; }
        }

        /// <summary>
        /// Creates a new, unique ID
        /// </summary>
        /// <returns>Unique ID.</returns>
        public XID CreateId()
        {
            return new XID();
        }
    }
}
