using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using OpenXesNet.model;
using OpenXesNet.id;
using OpenXesNet.factory;
using System.Collections.Generic;
using OpenXesNet.logging;
using OpenXesNet.info;


namespace OpenXesNet.extension
{
    public class XExtensionParser
    {
        static XExtensionParser singleton;
        static readonly List<string> ATTRIBUTE_TYPES = new List<string>(new string[] { "string", "date", "int", "float", "boolean", "id" });

        readonly XmlSchema schema;

        public static XExtensionParser Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new XExtensionParser();
                }
                return singleton;
            }
        }

        public XExtensionParser()
        {
            // Load the xsd file for validations
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream schemaStream = assembly.GetManifestResourceStream("xesext-ieee-1849-2016"))
            {
                schema = XmlSchema.Read(schemaStream, null);
            }
        }

        /// <summary>
        /// Validates an XML file againts the XesExt schema
        /// </summary>
        /// <returns>The validate.</returns>
        /// <param name="location">A file name or a URI.</param>
        protected bool Validate(string location)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(location);
            doc.Schemas.Add(schema);
            try
            {
                doc.Validate((sender, evt) =>
                {
                    if (evt.Severity == XmlSeverityType.Error)
                    {
                        XLogging.Log(String.Format("XML validation error: {0}", evt.Message), XLogging.Importance.ERROR);
                    }
                    else
                    {
                        XLogging.Log(String.Format("XML validation warning: {0}", evt.Message), XLogging.Importance.WARNING);
                    }
                });
                return true;
            }
            catch (Exception e)
            {
                XLogging.Log(e.Message, XLogging.Importance.ERROR);
                return false;
            }
        }

        public XExtension Parse(FileInfo file)
        {
            if (file.Exists)
            {
                if (!Validate(file.FullName))
                {
                    throw new XmlSchemaValidationException();
                }
            }
            XmlReader reader = XmlReader.Create(file.OpenRead());

            return this.Parse(reader);
        }

        public XExtension Parse(Uri uri)
        {
            if (uri.IsWellFormedOriginalString())
            {
                if (!Validate(uri.ToString()))
                {
                    throw new XmlSchemaValidationException();
                }
            }
            WebRequest request = WebRequest.Create(uri);
            using (WebResponse response = request.GetResponse())
            {

                using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                {
                    return this.Parse(reader);
                }
            }
        }

        protected XExtension Parse(XmlReader reader)
        {
            XExtension extension = null;
            XAttribute currentAttribute = null;
            Dictionary<string, XAttribute> xAttributes = null;
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;

            while (reader.Read())
            {
                // When a start tag is found
                if (reader.IsStartElement())
                {
                    string tagName = reader.LocalName;
                    if (tagName.Equals("", StringComparison.CurrentCultureIgnoreCase))
                    {
                        tagName = reader.Name;
                    }

                    if (tagName.Equals("xesextension", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string xName = reader.GetAttribute("name");
                        string xPrefix = reader.GetAttribute("prefix");
                        Uri xUri = null;
                        try
                        {
                            xUri = new Uri(reader.GetAttribute("uri"));
                        }
                        catch (UriFormatException e)
                        {
                            XLogging.Log(e.Message, XLogging.Importance.ERROR);
                            throw e;
                        }
                        extension = new XExtension(xName, xPrefix, xUri);
                    }
                    else if (tagName.Equals("log", StringComparison.CurrentCultureIgnoreCase))
                    {
                        xAttributes = extension.LogAttributes;
                    }
                    else if (tagName.Equals("trace", StringComparison.CurrentCultureIgnoreCase))
                    {
                        xAttributes = extension.TraceAttributes;
                    }
                    else if (tagName.Equals("event", StringComparison.CurrentCultureIgnoreCase))
                    {
                        xAttributes = extension.EventAttributes;
                    }
                    else if (tagName.Equals("meta", StringComparison.CurrentCultureIgnoreCase))
                    {
                        xAttributes = extension.MetaAttributes;
                    }
                    else if (tagName.Equals("string", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string key = reader.GetAttribute("key");
                        currentAttribute = factory.CreateAttributeLiteral(key, "DEFAULT", extension);
                        xAttributes.Add(key, currentAttribute);
                    }
                    else if (tagName.Equals("date", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string key = reader.GetAttribute("key");
                        currentAttribute = factory.CreateAttributeTimestamp(key, 0L, extension);
                        xAttributes.Add(key, currentAttribute);
                    }
                    else if (tagName.Equals("int", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string key = reader.GetAttribute("key");
                        currentAttribute = factory.CreateAttributeDiscrete(key, 0L, extension);
                        xAttributes.Add(key, currentAttribute);
                    }
                    else if (tagName.Equals("float", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string key = reader.GetAttribute("key");
                        currentAttribute = factory.CreateAttributeContinuous(key, 0.0D, extension);
                        xAttributes.Add(key, currentAttribute);
                    }
                    else if (tagName.Equals("boolean", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string key = reader.GetAttribute("key");
                        currentAttribute = factory.CreateAttributeBoolean(key, false, extension);
                        xAttributes.Add(key, currentAttribute);
                    }
                    else if (tagName.Equals("id", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string key = reader.GetAttribute("key");
                        currentAttribute = factory.CreateAttributeID(key, XIDFactory.Instance.CreateId(),
                               extension);
                        xAttributes.Add(key, currentAttribute);
                    }
                    else if (tagName.Equals("alias", StringComparison.CurrentCultureIgnoreCase) && currentAttribute != null)
                    {
                        string mapping = reader.GetAttribute("mapping");
                        string name = reader.GetAttribute("name");
                        XGlobalAttributeNameMap.Instance.RegisterMapping(mapping, currentAttribute.Key, name);
                    }
                    else
                    {
                        // non supported tag
                        XLogging.Log(String.Format("Non-supported tag '{0}' found. Ignoring it.", tagName), XLogging.Importance.TRACE);
                    }
                }
                // When a close tag is found (including empty elements)
                if (reader.IsEmptyElement || !reader.IsStartElement())
                {
                    string tagName = reader.LocalName;
                    if (tagName.Equals("", StringComparison.InvariantCultureIgnoreCase))
                    {
                        tagName = reader.Name;
                    }
                    if (ATTRIBUTE_TYPES.Contains(tagName.Trim().ToLower()))
                    {
                        currentAttribute = null;
                    }
                }
            }
            return extension;
        }
    }
}
