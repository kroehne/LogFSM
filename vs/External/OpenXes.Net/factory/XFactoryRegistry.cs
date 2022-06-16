using System;
using OpenXesNet.util;

namespace OpenXesNet.factory
{
    /// <summary>
    /// <para>XModelFactoryRegistry is the most important integration point for external 
    /// contributors, aside from the extension infrastructure.</para>
    /// <para>This singleton class serves as a system-wide registry for XES factory 
    /// implementations.It provides a current, i.e.standard, factory implementation, 
    /// which can be switched by applications.This factory will be used in any 
    /// internal places, e.g., for creating models from reading XES serializations.</para>
    /// <para>Other, e.g.proprietary or domain-specific, implementations of the XES 
    /// standard (and the OpenXES model hierarchy interface) are suggested to implement
    /// the XModelFactory interface, and to register their factory with this registry.
    /// This enables to transparently switch the storage implementation of the complete
    /// OpenXES system (wherever applicable), and every application making use of this
    /// registry to create new models.</para>
    /// </summary>
    public class XFactoryRegistry : XRegistry<IXFactory>
    {
        /// <summary>
        /// Singleton registry instance.
        /// </summary>
        static XFactoryRegistry singleton = new XFactoryRegistry();

        /// <summary>
        /// Retrieves the singleton registry instance.
        /// </summary>
        /// <value>The instance.</value>
        public static XFactoryRegistry Instance
        {
            get { return singleton; }
        }

        /// <summary>
        /// Creates the singleton.
        /// </summary>
        XFactoryRegistry()
        {
            this.CurrentDefault = new XFactoryNaive();
        }

        protected override bool AreEqual(IXFactory instance1, IXFactory instance2)
        {
            return instance1.GetType().Equals(instance2.GetType());
        }
    }
}
