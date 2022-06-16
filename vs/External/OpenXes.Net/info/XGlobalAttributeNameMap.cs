using System.Collections.Generic;
using System.Text;
using OpenXesNet.model;

namespace OpenXesNet.info
{
    /// <summary>
    /// This singleton class implements a global attribute name mapping facility 
    /// and can manage a number of attribute name mappings. 
    /// Further, this class also acts as a proxy to the standard 
    /// mapping, i.e.it can be used directly as a attribute name mapping instance.
    /// </summary>
    public class XGlobalAttributeNameMap : Dictionary<string, XAttributeNameMap>, IXAttributeNameMap
    {
        /// <summary>
        /// The standard attribute name mapping, to the English language (EN).
        /// </summary>
        public static readonly string MAPPING_STANDARD = "EN";
        /// <summary>
        /// The attribute name mapping to the English language.
        /// </summary>
        public static readonly string MAPPING_ENGLISH = "EN";
        /// <summary>
        /// The attribute name mapping to the German language.
        /// </summary>
        public static readonly string MAPPING_GERMAN = "DE";
        /// <summary>
        /// The attribute name mapping to the Dutch language.
        /// </summary>
        public static readonly string MAPPING_DUTCH = "NL";
        /// <summary>
        /// The attribute name mapping to the French language.
        /// </summary>
        public static readonly string MAPPING_FRENCH = "FR";
        /// <summary>
        /// The attribute name mapping to the Italian language.
        /// </summary>
        public static readonly string MAPPING_ITALIAN = "IT";
        /// <summary>
        /// The attribute name mapping to the Spanish language.
        /// </summary>
        public static readonly string MAPPING_SPANISH = "ES";
        /// <summary>
        /// The attribute name mapping to the Portuguese language.
        /// </summary>
        public static readonly string MAPPING_PORTUGUESE = "PT";

        /// <summary>
        /// Singleton instance.
        /// </summary>
        static readonly XGlobalAttributeNameMap singleton = new XGlobalAttributeNameMap();

        /// <summary>
        /// Standard mapping (EN).
        /// </summary>
        XAttributeNameMap standardMapping;

        /// <summary>
        /// Accesses the singleton instance.
        /// </summary>
        /// <value>The instance.</value>
        public static XGlobalAttributeNameMap Instance
        {
            get { return singleton; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.info.XGlobalAttributeNameMap"/> class.
        /// </summary>
        XGlobalAttributeNameMap()
        {
            this.standardMapping = new XAttributeNameMap("EN");
            this.Add("EN", this.standardMapping);
        }

        /// <summary>
        /// Returns the names of all available mappings. Note that referenced mappings may be empty.
        /// </summary>
        /// <returns>A collection of names of all available mappings.</returns>
        public IList<string> GetAvailableMappingNames()
        {
            return new List<string>(this.Keys);
        }

        /// <summary>
        /// Returns all available mappings. Note that returned mappings may be empty.
        /// </summary>
        /// <returns> A collection of all available mappings.</returns>
        public IList<XAttributeNameMap> GetAvailableMappings()
        {
            return new List<XAttributeNameMap>(this.Values);
        }

        /// <summary>
        /// Provides access to a specific attribute name mapping by its name.
        /// If the requested mapping does not exist yet, a new mapping will be created, 
        /// added to the set of managed mappings, and returned. This means, this method 
        /// will always return a mapping,  but this could be empty.
        /// </summary>
        /// <returns>The requested mapping, as stored in this facility (or newly created).</returns>
        /// <param name="name">Name of the requested mapping.</param>
        public IXAttributeNameMap GetMapping(string name)
        {
            if (!this.ContainsKey(name))
            {
                this.Add(name, new XAttributeNameMap(name));
            }

            return this[name];
        }

        /// <summary>
        /// Retrieves the standard attribute name mapping, i.e.the EN english language mapping.
        /// </summary>
        /// <returns>The standard mapping.</returns>
        public IXAttributeNameMap GetStandardMapping()
        {
            return this.standardMapping;
        }

        /// <summary>
        /// Maps an attribute safely, using the given attribute mapping.  Safe mapping 
        /// attempts to map the attribute using the given mapping first. If this does 
        /// not succeed, the standard mapping (EN) will be used for mapping. If no 
        /// mapping is available in the standard mapping, the original attribute key 
        /// is returned unchanged. This way, it is always ensured that this method 
        /// returns a valid string for naming attributes.
        /// </summary>
        /// <returns>The safe mapping for the given attribute.</returns>
        /// <param name="attribute">Attribute to map.</param>
        /// <param name="mapping">Mapping to be used preferably.</param>
        public string MapSafely(XAttribute attribute, IXAttributeNameMap mapping)
        {
            return this.MapSafely(attribute.Key, mapping);
        }

        /// <summary>
        /// Maps an attribute safely, using the given attribute mapping. Safe mapping 
        /// attempts to map the attribute using the given mapping first. If this does 
        /// not succeed, the standard mapping (EN) will be used for mapping.If no mapping 
        /// is available in the standard mapping, the original attribute key is 
        /// returned unchanged. This way, it is always ensured that this method 
        /// returns a valid string for naming attributes.
        /// </summary>
        /// <returns>The safe mapping for the given attribute key.</returns>
        /// <param name="attributeKey">Key of the attribute to map.</param>
        /// <param name="mapping">Mapping to be used preferably.</param>
        public string MapSafely(string attributeKey, IXAttributeNameMap mapping)
        {
            string alias = null;
            if (mapping != null)
            {
                alias = mapping.Get(attributeKey);
            }
            if (alias == null)
            {
                alias = this.standardMapping.Get(attributeKey);
            }
            if (alias == null)
            {
                alias = attributeKey;
            }
            return alias;
        }

        /// <summary>
        /// Maps an attribute safely, using the given attribute mapping. Safe mapping 
        /// attempts to map the attribute using the given mapping first. If this does 
        /// not succeed, the standard mapping (EN) will be used for mapping.If no mapping 
        /// is available in the standard mapping, the original attribute key is 
        /// returned unchanged. This way, it is always ensured that this method 
        /// returns a valid string for naming attributes.
        /// </summary>
        /// <returns>The safe mapping for the given attribute.</returns>
        /// <param name="attribute">Attribute to map.</param>
        /// <param name="mappingName">Name of the mapping to be used preferably.</param>
        public string MapSafely(XAttribute attribute, string mappingName)
        {
            return MapSafely(attribute, this[mappingName]);
        }

        /// <summary>
        /// Maps an attribute safely, using the given attribute mapping. Safe mapping 
        /// attempts to map the attribute using the given mapping first. If this does 
        /// not succeed, the standard mapping (EN) will be used for mapping.If no mapping 
        /// is available in the standard mapping, the original attribute key is 
        /// returned unchanged. This way, it is always ensured that this method 
        /// returns a valid string for naming attributes.
        /// </summary>
        /// <returns>The safe mapping for the given attribute.</returns>
        /// <param name="attributeKey">Key of the attribute to map.</param>
        /// <param name="mappingName">Name of the mapping to be used preferably.</param>
        public string MapSafely(string attributeKey, string mappingName)
        {
            return MapSafely(attributeKey, this[mappingName]);
        }

        /// <summary>
        /// Registers a known attribute for mapping in a given attribute name map. 
        /// </summary>
        /// <remarks>
        /// <b>IMPORTANT:</b> This method should only be called when one intends to 
        /// create, or add to, the global attribute name mapping.
        /// </remarks>
        /// <param name="mappingName">Name of the mapping to register with.</param>
        /// <param name="attributeKey">Attribute key to be mapped.</param>
        /// <param name="alias">Alias to map the given attribute to.</param>
        public void RegisterMapping(string mappingName, string attributeKey, string alias)
        {
            lock (this)
            {
                XAttributeNameMap mapping = (XAttributeNameMap)GetMapping(mappingName);
                mapping.RegisterMapping(attributeKey, alias);
            }
        }

        public string MappingName
        {
            get { return MAPPING_STANDARD; }
        }

        public string Get(XAttribute attribute)
        {
            return this.standardMapping.Get(attribute);
        }

        public string Get(string key)
        {
            return this.standardMapping.Get(key);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Global attribute name map.\n\nContained maps:\n\n");
            foreach (XAttributeNameMap map in this.Values)
            {
                sb.Append(map.ToString());
                sb.Append("\n\n");
            }
            return sb.ToString();
        }
    }
}
