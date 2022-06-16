using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenXesNet.logging;


namespace OpenXesNet.model
{
    /// <summary>
    /// <para>Lazy implementation of the XAttributeMap interface.</para>
    /// <para>This implementation serves as a proxy for an XAttributeMapImpl instance, 
    /// which is initially not present.Once the attribute map is to be filled
    /// with values, the true backing XAttributeMapImpl instance will be created 
    /// on the fly, and used for storing and accessing data transparently.
    /// This lazy instantiation prevents lots of initializations of real
    /// attribute maps, since a large amount of attributes do not have
    /// any meta-attributes.</para>
    /// <para>This class is a generic, and can be parametrized with the actual
    /// implementation for the backing storage, which will then be instantiated
    /// on demand.</para>
    /// </summary>
    public class XAttributeMapLazy<T> : IXAttributeMap where T : XAttributeMap, new()
    {
        /// <summary>
        /// Backing store, initialized lazily, i.e. on the fly.
        /// </summary>
        T backingStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeMapLazy`1"/> class.
        /// </summary>
        public XAttributeMapLazy()
        {
            this.backingStore = (T)Activator.CreateInstance(typeof(T), new object[] { });
        }

        public List<XAttribute> AsList()
        {
            return this.backingStore.AsList();
        }

        public XAttribute this[string key]
        {
            get
            {
                return this.backingStore[key];
            }
            set
            {
                this.backingStore[key] = value;
            }
        }

        public void Clear()
        {
            this.backingStore.Clear();
        }

        public bool ContainsKey(string key)
        {
            return this.backingStore.ContainsKey(key);
        }

        public bool Contains(KeyValuePair<string, XAttribute> kvpair)
        {
            return this.backingStore.Contains(kvpair);
        }

        public bool ContainsValue(object value)
        {
            return this.backingStore.ContainsValue((XAttribute)value);
        }

        public bool TryGetValue(string key, out XAttribute attr)
        {
            attr = null;
            if (this.backingStore.ContainsKey(key))
            {
                attr = this.backingStore[key];
                return true;
            }
            return false;
        }

        public bool IsEmpty()
        {
            return this.backingStore.Count == 0;
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<String> Keys
        {
            get
            {
                return new HashSet<string>(this.backingStore.Keys);
            }
        }

        public void Add(string key, XAttribute value)
        {
            this.backingStore.Add(key, value);
        }

        public void Add(KeyValuePair<string, XAttribute> kvpair)
        {
            this.Add(kvpair.Key, kvpair.Value);
        }

        public void PutAll(Dictionary<string, XAttribute> t)
        {
            if (t.Count > 0)
            {
                this.backingStore.Concat(t);
            }
        }

        public bool Remove(string key)
        {
            return this.backingStore.Remove(key);
        }

        public bool Remove(KeyValuePair<string, XAttribute> kvpair)
        {
            return this.backingStore.Remove(kvpair.Key);
        }

        public void CopyTo(KeyValuePair<string, XAttribute>[] array, int arrayIndex)
        {
            // TODO: implement if necessary
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                return this.backingStore.Count;
            }
        }

        public ICollection<XAttribute> Values
        {
            get
            {
                return new List<XAttribute>(this.backingStore.Values);
            }
        }

        public object Clone()
        {
            try
            {
                XAttributeMapLazy<T> clone = (XAttributeMapLazy<T>)MemberwiseClone();
                if (this.backingStore != null)
                {
                    clone.backingStore = ((T)this.backingStore.Clone());
                }
                return clone;
            }
            catch (NotSupportedException e)
            {
                XLogging.Log(e.Message, XLogging.Importance.ERROR);
            }
            return null;
        }

        public IEnumerator<KeyValuePair<string, XAttribute>> GetEnumerator()
        {
            if (this.backingStore != null)
            {
                return this.backingStore.GetEnumerator();
            }
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.backingStore != null)
            {
                return this.backingStore.GetEnumerator();
            }
            return null;
        }
    }
}
