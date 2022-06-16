using System;
using OpenXesNet.factory;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using OpenXesNet.model;
using OpenXesNet.extension;
using OpenXesNet.logging;
using OpenXesNet.classification;
using OpenXesNet.util;
using OpenXesNet.id;
using System.Globalization;

namespace OpenXesNet.io
{
    /// <summary>
    /// Parser for the XES XML serialization.
    /// </summary>
    public class XesXmlParser : XParser
    {
        /// <summary>
        /// Unique URI for the format definition.
        /// </summary>
        protected static readonly Uri XES_URI = new UriBuilder("http://www.xes-standard.org/").Uri;

        /// <summary>
        /// XES model factory used to build model.
        /// </summary>
        protected IXFactory factory;

        /// <summary>
        /// Creates a new parser instance.
        /// </summary>
        /// <param name="factory">The XES model factory instance used to build 
        /// the model from the serialization.</param>
        public XesXmlParser(IXFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Creates a new parser instance, using the currently-set standard factory 
        /// for building the model.
        /// </summary>
        public XesXmlParser() : this(XFactoryRegistry.Instance.CurrentDefault)
        {
        }

        public override String Author
        {
            get { return "Alvaro Martinez Romero"; }
        }

        public override bool CanParse(FileInfo fileInfo)
        {
            String filename = fileInfo.FullName;
            return EndsWith(filename, ".xes");
        }

        public override String Description
        {
            get { return "Reads XES models from plain XML serializations"; }
        }

        public override string Name
        {
            get
            {
                return "XES XML";
            }
        }

        public override IXLog Parse(string fileName)
        {
            FileInfo info = new FileInfo(fileName);
            if (!info.Exists)
            {
                throw new FileNotFoundException();
            }
            return Parse(new BufferedStream(info.OpenRead()));
        }

        public override IXLog Parse(Stream stream)
        {
            Stack<IXAttributable> attributableStack = new Stack<IXAttributable>();
            Stack<XAttribute> attributeStack = new Stack<XAttribute>();
            IXEvent evt = null;
            IXLog log = null;
            IXTrace trace = null;
            List<XAttribute> globals = null;

            using (XmlReader reader = XmlReader.Create(stream))
            {
                List<string> ATTR_TYPE_TAGS = new List<string>(new string[] { "string", "date", "int", "float", "boolean", "id", "list", "container" });
                ATTR_TYPE_TAGS.Sort();

                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        // start tag found
                        string tagName = reader.LocalName.Trim();
                        if (tagName.Length == 0)
                        {
                            tagName = reader.Name.Trim(); // <= qualified name
                        }


                        if (ATTR_TYPE_TAGS.Contains(tagName.ToLower()))
                        {
                            // The tag is an attribute
                            string key = reader.GetAttribute("key") ?? "";
                            string val = reader.GetAttribute("value") ?? "";
                            XExtension ext = null;
                            int colonindex = key.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
                            if (colonindex > 0)
                            {
                                string prefix = key.Substring(0, colonindex);
                                ext = XExtensionManager.Instance.GetByPrefix(prefix);
                            }

                            XAttribute attr = null;
                            switch (tagName)
                            {
                                case "string":
                                    attr = factory.CreateAttributeLiteral(key, val, ext);
                                    break;
                                case "int":
                                    attr = factory.CreateAttributeDiscrete(key, long.Parse(val), ext);
                                    break;
                                case "boolean":
                                    attr = factory.CreateAttributeBoolean(key, bool.Parse(val), ext);
                                    break;
                                case "date":
                                    DateTime d = XAttributeTimestamp.Parse(val);
                                    attr = factory.CreateAttributeTimestamp(key, d, ext);
                                    break;
                                case "float":
                                    attr = factory.CreateAttributeContinuous(key, double.Parse(val), ext);
                                    break;
                                case "id":
                                    attr = factory.CreateAttributeID(key, XID.Parse(val), ext);
                                    break;
                                case "list":
                                    attr = factory.CreateAttributeList(key, ext);
                                    break;
                                case "container":
                                    attr = factory.CreateAttributeContainer(key, ext);
                                    break;
                                default:
                                    XLogging.Log("Unknown tag '" + tagName + "'", XLogging.Importance.WARNING);
                                    break;
                            }
                            if (reader.IsEmptyElement)
                            {
                                // No child nodes, we can directly store it
                                if (globals != null)
                                {
                                    // attribute is global
                                    globals.Add(attr);
                                }
                                else
                                {
                                    attributableStack.Peek().GetAttributes().Add(attr.Key, attr);

                                    if ((!(attributeStack.Count == 0))
                                            && (attributeStack.Peek() is XAttributeCollection))
                                    {
                                        ((XAttributeCollection)attributeStack.Peek()).AddToCollection(attr);
                                    }
                                }

                            }
                            else if (attr != null)
                            {
                                attributeStack.Push(attr);
                                attributableStack.Push((IXAttributable)attr);
                            }
                        }
                        else if ("event".Equals(tagName.ToLower()))
                        {
                            // Parse an event
                            evt = factory.CreateEvent();
                            attributableStack.Push(evt);
                        }
                        else if ("trace".Equals(tagName.ToLower()))
                        {
                            trace = factory.CreateTrace();
                            attributableStack.Push(trace);
                        }
                        else if ("log".Equals(tagName.ToLower()))
                        {
                            log = factory.CreateLog();
                            ((XLog)log).Version = reader.GetAttribute("xes.version") ?? "2.0";
                            ((XLog)log).Features = reader.GetAttribute("xes.features") ?? "";
                            attributableStack.Push(log);
                        }
                        else if ("extension".Equals(tagName.ToLower()))
                        {
                            XExtension extension = null;
                            String uriString = reader.GetAttribute("uri");
                            if (uriString != null)
                            {
                                extension = XExtensionManager.Instance.GetByUri(new UriBuilder(uriString).Uri);
                            }
                            else
                            {
                                string prefixString = reader.GetAttribute("prefix");
                                if (prefixString != null)
                                {
                                    extension = XExtensionManager.Instance.GetByPrefix(prefixString);
                                }
                            }

                            if (extension != null)
                            {
                                log.Extensions.Add(extension);
                            }
                            else
                            {
                                XLogging.Log("Unknown extension: " + uriString, XLogging.Importance.ERROR);
                            }
                        }
                        else if ("global".Equals(tagName.ToLower()))
                        {
                            string scope = reader.GetAttribute("scope");
                            if (scope.Equals("trace"))
                            {
                                globals = log.GlobalTraceAttributes;
                            }
                            else if (scope.Equals("event"))
                            {
                                globals = log.GlobalEventAttributes;
                            }
                        }
                        else if ("classifier".Equals(tagName.ToLower()))
                        {

                            string name = reader.GetAttribute("name");
                            string keys = reader.GetAttribute("keys");
                            if ((name == null) || (keys == null) || (name.Length <= 0) || (keys.Length <= 0))
                            {
                                continue;
                            }
                            IList<string> keysList = FixKeys(log, XTokenHelper.ExtractTokens(keys));

                            string[] keysArray = new string[keysList.Count];
                            int i = 0;
                            foreach (string key in keysList)
                            {
                                keysArray[(i++)] = key;
                            }
                            IXEventClassifier classifier = new XEventAttributeClassifier(name, keysArray);

                            log.Classifiers.Add(classifier);
                        }
                    }
                    else
                    {
                        // end tag found
                        string tagName = reader.LocalName.Trim().ToLower();
                        if (tagName.Length == 0)
                        {
                            tagName = reader.Name.Trim().ToLower(); // <= qualified name
                        }

                        if ("global".Equals(tagName))
                        {
                            globals = null;
                        }
                        else if (ATTR_TYPE_TAGS.Contains(tagName))
                        {
                            XAttribute attribute = attributeStack.Pop();
                            attributableStack.Pop();
                            if (globals != null)
                            {
                                globals.Add(attribute);
                            }
                            else
                            {
                                attributableStack.Peek().GetAttributes().Add(attribute.Key, attribute);

                                if ((!(attributeStack.Count == 0))
                                        && (attributeStack.Peek() is XAttributeCollection))
                                {
                                    ((XAttributeCollection)attributeStack.Peek()).AddToCollection(attribute);
                                }
                            }
                        }
                        else if ("event".Equals(tagName))
                        {
                            trace.Add(evt);
                            evt = null;
                            attributableStack.Pop();
                        }
                        else if ("trace".Equals(tagName))
                        {
                            log.Add(trace);
                            trace = null;
                            attributableStack.Pop();
                        }
                        else if ("log".Equals(tagName))
                        {
                            attributableStack.Pop();
                        }
                    }
                }

            }
            return log;

        }

        IList<String> FixKeys(IXLog log, List<string> keys)
        {
            IList<string> fixedKeys = FixKeys(log, keys, 0);
            return (fixedKeys ?? keys);
        }

        IList<string> FixKeys(IXLog log, List<string> keys, int index)
        {
            if (index >= keys.Count)
            {
                return keys;
            }

            if (FindGlobalEventAttribute(log, keys[index]))
            {
                IList<string> fixedKeys = FixKeys(log, keys, index + 1);
                if (fixedKeys != null)
                {
                    return fixedKeys;
                }

            }

            if (index + 1 == keys.Count)
            {
                return null;
            }

            List<string> newKeys = new List<string>(keys.Count - 1);
            for (int i = 0; i < index; ++i)
            {
                newKeys.Add(keys[i]);
            }

            newKeys.Add(keys[index] + " " + keys[index + 1]);

            for (int i = index + 2; i < keys.Count; ++i)
            {
                newKeys.Add(keys[i]);
            }

            return FixKeys(log, newKeys, index);
        }

        bool FindGlobalEventAttribute(IXLog log, String key)
        {
            foreach (XAttribute attribute in log.GlobalEventAttributes)
            {
                if (attribute.Key.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }

        public override void Dispose()
        {
            // Nothing to dispose at this time
        }
    }
}
