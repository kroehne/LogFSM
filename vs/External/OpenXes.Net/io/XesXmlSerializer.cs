using System;
using System.IO;
using System.Collections.Generic;
using OpenXesNet.model;
using OpenXesNet.logging;
using OpenXesNet.extension;
using OpenXesNet.classification;
using OpenXesNet.util;
using System.Xml;
using System.Reflection;

namespace OpenXesNet.io
{
    /// <summary>
    /// XES plain XML serialization for the XES format.
    /// </summary>
    public class XesXmlSerializer : IXSerializer
    {
        readonly Version openXesNetVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public string OutputTimeStampFormatString = "dd.MM.yyyy HH:mm:ss.fff";

        public string Description
        {
            get { return "XES XML Serialization"; }
        }

        public string Name
        {
            get { return "XES XML"; }
        }

        public string Author
        {
            get { return "Alvaro Martinez Romero"; }
        }

        public string[] Suffices
        {
            get { return new string[] { "xes" }; }
        }

        public void Serialize(IXLog log, Stream stream)
        {
            XLogging.Log("Start serializing log to XES.XML", XLogging.Importance.DEBUG);

            long start = DateTime.Now.Ticks;
            XmlDocument doc = new XmlDocument();

            doc.AppendChild(doc.CreateComment("This file has been generated with the OpenXES.Net library. It conforms"));
            doc.AppendChild(doc.CreateComment("to the XML serialization of the XES standard for log storage and management."));
            doc.AppendChild(doc.CreateComment("XES standard version: 2.0"));
            doc.AppendChild(doc.CreateComment(String.Format("OpenXES.Net library version: {0}", openXesNetVersion)));
            doc.AppendChild(doc.CreateComment("OpenXES is available from http://www.openxes.org/")); // TODO: update this string

            XmlNode logNode = doc.CreateElement("log");
            doc.AppendChild(logNode);

            // Log metadata
            XmlAttribute xesNetVersion = doc.CreateAttribute("openxesnet.version");
            xesNetVersion.Value = String.Format("{0}", openXesNetVersion);
            logNode.Attributes.Append(xesNetVersion);

            XmlAttribute xesVersion = doc.CreateAttribute("xes.version");
            xesVersion.Value = ((XLog)log).Version;
            logNode.Attributes.Append(xesVersion);

            XmlAttribute xesFeatures = doc.CreateAttribute("xes.features");
            xesFeatures.Value = ((XLog)log).Features;
            logNode.Attributes.Append(xesFeatures);

            // Log extensions
            foreach (XExtension extension in log.Extensions)
            {
                XmlNode extNode = doc.CreateElement("extension");

                XmlAttribute name = doc.CreateAttribute("name");
                name.Value = extension.Name;
                extNode.Attributes.Append(name);

                XmlAttribute prefix = doc.CreateAttribute("prefix");
                prefix.Value = extension.Prefix;
                extNode.Attributes.Append(prefix);

                XmlAttribute uri = doc.CreateAttribute("uri");
                uri.Value = extension.Uri.ToString();
                extNode.Attributes.Append(uri);

                logNode.AppendChild(extNode);
            }

            // Global attributes: we add this tag and subtags only if present
            /*
            if (log.GlobalEventAttributes.Count > 0 || log.GlobalTraceAttributes.Count > 0)
            {
                XmlNode globalNode = doc.CreateElement("global");
                logNode.AppendChild(globalNode);

                if (log.GlobalTraceAttributes.Count > 0)
                {
                    XmlNode globalTraceNode = doc.CreateElement("trace");
                    globalNode.AppendChild(globalTraceNode);
                    AddAttributes(doc, globalTraceNode, log.GlobalTraceAttributes);
                }

                if (log.GlobalEventAttributes.Count > 0)
                {
                    XmlNode globalEventNode = doc.CreateElement("event");
                    globalNode.AppendChild(globalEventNode);
                    AddAttributes(doc, globalEventNode, log.GlobalEventAttributes);
                }
            }*/

            if (log.GlobalEventAttributes.Count > 0)
            {
                XmlNode globalEventNode = doc.CreateElement("global");
                XmlAttribute scope = doc.CreateAttribute("scope");
                scope.Value = "event";
                globalEventNode.Attributes.Append(scope);

                logNode.AppendChild(globalEventNode);
                AddAttributes(doc, globalEventNode, log.GlobalEventAttributes);
            }

            if (log.GlobalTraceAttributes.Count > 0)
            {
                XmlNode globalTraceNode = doc.CreateElement("global");
                XmlAttribute scope = doc.CreateAttribute("scope");
                scope.Value = "trace";
                globalTraceNode.Attributes.Append(scope);

                logNode.AppendChild(globalTraceNode);
                AddAttributes(doc, globalTraceNode, log.GlobalTraceAttributes);
            }

            foreach (IXEventClassifier classifier in log.Classifiers)
            {
                if (classifier is XEventAttributeClassifier)
                {
                    XEventAttributeClassifier attrClass = (XEventAttributeClassifier)classifier;
                    XmlNode clsNode = doc.CreateElement("classifier");
                    XmlAttribute name = doc.CreateAttribute("name");
                    name.Value = ((XEventAttributeClassifier)classifier).Name;
                    clsNode.Attributes.Append(name);
                    XmlAttribute keys = doc.CreateAttribute("keys");
                    keys.Value = XTokenHelper.FormatTokenString(new List<string>(attrClass.DefiningAttributeKeys));
                    clsNode.Attributes.Append(keys);
                }
            }

            // Log attributes
            AddAttributes(doc, logNode, log.GetAttributes().AsList());

            // Log traces
            foreach (XTrace trace in log)
            {
                XmlNode traceNode = doc.CreateElement("trace");
                logNode.AppendChild(traceNode);

                // Trace attributes
                AddAttributes(doc, traceNode, trace.GetAttributes().AsList());

                // Trace events
                foreach (XEvent evt in trace)
                {
                    // Event attributes
                    XmlNode eventNode = doc.CreateElement("event");
                    traceNode.AppendChild(eventNode);
                    AddAttributes(doc, eventNode, evt.GetAttributes().AsList());
                }
            }

            // Save the doc
            doc.Save(stream);

            string duration = " (" + (new TimeSpan(DateTime.Now.Ticks - start)).TotalMilliseconds + " msec.)";

            XLogging.Log("Finished serializing log" + duration, XLogging.Importance.DEBUG);
        }

        /// <summary>
        /// Helper method, adds the given collection of attributes to the given Tag.
        /// </summary>
        /// <param name="doc">The document containing the nodes and attributes.</param>
        /// <param name="parent">Node to add attributes to.</param>
        /// <param name="attributes">The attributes to add.</param>
        protected void AddAttributes(XmlDocument doc, XmlNode parent, List<XAttribute> attributes)
        {
            foreach (XAttribute attribute in attributes)
            {
                XmlNode attrNode;
                XmlAttribute key = doc.CreateAttribute("key");
                XmlAttribute val = doc.CreateAttribute("value");

                if (attribute is XAttributeList)
                {
                    attrNode = doc.CreateElement("list");
                    key.Value = attribute.Key;
                    attrNode.Attributes.Append(key);
                }
                else if (attribute is XAttributeContainer)
                {
                    attrNode = doc.CreateElement("container");
                    key.Value = attribute.Key;
                    attrNode.Attributes.Append(key);
                }
                else if (attribute is XAttributeLiteral)
                {
                    attrNode = doc.CreateElement("string");
                    key.Value = attribute.Key;
                    val.Value = ((XAttributeLiteral)attribute).Value;
                    attrNode.Attributes.Append(key);
                    attrNode.Attributes.Append(val);
                }
                else if (attribute is XAttributeDiscrete)
                {
                    attrNode = doc.CreateElement("int");
                    key.Value = attribute.Key;
                    val.Value = ((XAttributeDiscrete)attribute).Value.ToString();
                    attrNode.Attributes.Append(key);
                    attrNode.Attributes.Append(val);
                }
                else if (attribute is XAttributeContinuous)
                {
                    attrNode = doc.CreateElement("float");
                    key.Value = attribute.Key;
                    val.Value = ((XAttributeContinuous)attribute).Value.ToString();
                    attrNode.Attributes.Append(key);
                    attrNode.Attributes.Append(val);
                }
                else if (attribute is XAttributeTimestamp)
                {
                    attrNode = doc.CreateElement("date");
                    key.Value = attribute.Key;
                    val.Value = ((XAttributeTimestamp)attribute).Value.ToString(OutputTimeStampFormatString); 
                    attrNode.Attributes.Append(key);
                    attrNode.Attributes.Append(val);
                }
                else if (attribute is XAttributeBoolean)
                {
                    attrNode = doc.CreateElement("boolean");
                    key.Value = attribute.Key;
                    val.Value = ((XAttributeBoolean)attribute).Value.ToString();
                    attrNode.Attributes.Append(key);
                    attrNode.Attributes.Append(val);
                }
                else if (attribute is XAttributeID)
                {
                    attrNode = doc.CreateElement("id");
                    key.Value = attribute.Key;
                    val.Value = ((XAttributeID)attribute).Value.ToString();
                    attrNode.Attributes.Append(key);
                    attrNode.Attributes.Append(val);
                }
                else
                {
                    throw new IOException("Unknown attribute type!");
                }
                parent.AppendChild(attrNode);


                // Check for nested attributes
                if (attribute is XAttributeCollection)
                {
                    List<XAttribute> childAttributes = ((XAttributeCollection)attribute).GetCollection();
                    AddAttributes(doc, attrNode, childAttributes);
                }
                else if (attribute.HasAttributes())
                {
                    AddAttributes(doc, attrNode, (List<XAttribute>)attribute.GetAttributes());
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
