using OpenXesNet.util;

namespace OpenXesNet.io
{
    ///<summary>
    /// System-wide registry for XES serializer implementations.
    /// Applications can use this registry as a convenience
    /// to provide an overview about serializeable formats, e.g.,
    /// in the user interface.
    /// Any custom serializer implementation can be registered
    /// with this registry, so that it transparently becomes
    /// available also to any other using application.
    /// </summary>
    public class XSerializerRegistry : XRegistry<IXSerializer>
    {

        /// <summary>
        /// Singleton registry instance.
        /// </summary>
        static XSerializerRegistry singleton = new XSerializerRegistry();

        /// <summary>
        /// Retrieves the singleton registry instance.
        /// </summary>
        /// <value>The instance.</value>
        public static XSerializerRegistry Instance
        {
            get { return singleton; }
        }

        /// <summary>
        /// Creates the singleton.
        /// </summary>
        XSerializerRegistry() 
        {
            Register(new XesXmlSerializer());
            CurrentDefault = new XesXmlGZIPSerializer();
        }

        protected override bool AreEqual(IXSerializer instance1, IXSerializer instance2)
        {
            return instance1.GetType().Equals(instance2.GetType());
        }
    }
}
