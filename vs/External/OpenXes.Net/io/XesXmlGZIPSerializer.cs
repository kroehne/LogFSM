using System.IO;
using System.IO.Compression;
using OpenXesNet.model;

namespace OpenXesNet.io
{
    /// <summary>
    /// XES compressed XML serialization for the XES format.
    /// </summary>
    public class XesXmlGZIPSerializer : XesXmlSerializer
    {
        public new string Description
        {
            get { return "XES XML Compressed Serialization"; }
        }

        public new string Name
        {
            get { return "XES XML Compressed"; }
        }

        public new string[] Suffices
        {
            get { return new string[] { "xez", "xes.gz" }; }
        }

        public new void Serialize(IXLog log, Stream stream)
        {
            this.Serialize(log, stream, CompressionLevel.Optimal);
        }

        public void Serialize(IXLog log, Stream stream, CompressionLevel level)
        {
            using (GZipStream gzos = new GZipStream(stream, level))
            using (BufferedStream bos = new BufferedStream(gzos))
            {
                base.Serialize(log, bos);
                bos.Flush();
            }
        }
    }
}
