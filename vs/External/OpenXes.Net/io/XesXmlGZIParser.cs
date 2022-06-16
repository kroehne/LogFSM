using System;
using OpenXesNet.factory;
using System.IO;
using System.IO.Compression;
using OpenXesNet.model;

namespace OpenXesNet.io
{
    /// <summary>
    /// Parser for the compressed XES XML serialization.
    /// </summary>
    public class XesXmlGzipParser : XesXmlParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.in.XesXmlGzipParser"/> class.
        /// </summary>
        public XesXmlGzipParser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.in.XesXmlGzipParser"/> class.
        /// </summary>
        /// <param name="factory">The XES model factory instance used to build the 
        /// model from the serialization.</param>
        public XesXmlGzipParser(IXFactory factory) : base(factory)
        {
        }

        public override bool CanParse(FileInfo fileInfo)
        {
            String filename = fileInfo.FullName;
            return EndsWith(fileInfo.Name, ".xez") || EndsWith(fileInfo.Name, ".xes.gz");
        }

        public override String Description
        {
            get { return "Reads XES models from compressed XML serializations"; }
        }

        public new String Name
        {
            get { return "XES XML Compressed"; }
        }

        public IXLog Parse(FileStream stream)
        {
            if (CanParse(new FileInfo(stream.Name)))
            {
                using (GZipStream compressed = new GZipStream(stream, CompressionMode.Decompress))
                {
                    return Parse(new GZipStream(stream, CompressionMode.Decompress));
                }
            }
            throw new NotSupportedException("Cannot parse this stream as a XES log");
        }

        public override IXLog Parse(string fileName)
        {
            FileInfo info = new FileInfo(fileName);
            if (!info.Exists)
            {
                throw new FileNotFoundException();
            }
            return this.Parse(info.OpenRead());
        }
    }
}