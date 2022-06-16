using System;
using System.Collections.Generic;

namespace OpenXesNet.util
{
    /// <summary>
    /// Template implementation for a generic registry.
    /// </summary>
    public abstract class XRegistry<T>
    {
        /// <summary>
        /// Registry set, holding all instances.
        /// </summary>
        readonly HashSet<T> registry;
        /// <summary>
        /// Holds the current default instance.
        /// </summary>
        T current;

        /// <summary>
        /// Instantiates a new registry.
        /// </summary>
        protected XRegistry()
        {
            this.registry = new HashSet<T>();
        }

        /// <summary>
        /// Retrieves a set of all available instances.
        /// </summary>
        /// <returns>The available.</returns>
        public HashSet<T> GetAvailable()
        {
            return this.registry;
        }

        /// <summary>
        /// Retrieves the current default instance.
        /// </summary>
        /// <returns>The default.</returns>
        public T CurrentDefault
        {
            get { return this.current; }
            set
            {
                this.registry.Add(value);
                this.current = value;
            }
        }

        /// <summary>
        /// Registers a new instance with this registry.
        /// </summary>
        /// <returns>The register.</returns>
        /// <param name="instance">Instance to be registered.</param>
        public void Register(T instance)
        {
            if (!(IsContained(instance)))
            {
                this.registry.Add(instance);
#pragma warning disable RECS0017 // Posible comparación de tipo de valor con 'null'
                if (this.current == null)
#pragma warning restore RECS0017 // Posible comparación de tipo de valor con 'null'
                {
                    this.current = instance;
                }
            }
        }

        /// <summary>
        /// Subclasses must implement this method. It is used by the registry to ensure 
        /// that no duplicates are inserted.
        /// </summary>
        /// <returns><c>true</c>, if equal was ared, <c>false</c> otherwise.</returns>
        /// <param name="instance1">Parameter 1.</param>
        /// <param name="instance2">Parameter 2.</param>
        protected abstract bool AreEqual(T instance1, T instance2);

        /// <summary>
        /// Checks whether the given instance is already contained in the registry.
        /// </summary>
        /// <returns>Whether the given instance is already registered.</returns>
        /// <param name="instance">Instance to check against registry.</param>
        protected bool IsContained(T instance)
        {
            foreach (T element in this.registry)
            {
                if (AreEqual(instance, element))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
