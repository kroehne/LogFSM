using System;
using System.IO;
using OpenXesNet.model;
using OpenXesNet.logging;

namespace OpenXesNet.io
{
    /// <summary>
    /// This class implements a universal parser, using the parser registry to 
    /// find an appropriate parser for extracting an XES model from any given file.
    /// May be used as a convenience method for applications.
    /// </summary>
    public class XUniversalParser
    {
        /// <summary>
        ///  Checks whether the given file can be parsed by any parser.
        /// </summary>
        /// <returns>The parse.</returns>
        /// <param name="file">File.</param>
        public bool CanParse(FileInfo file)
        {
            foreach (XParser parser in XParserRegistry.Instance.GetAvailable())
            {
                if (parser.CanParse(file))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Attempts to parse a collection of XES models from the given file, using 
        /// all available parsers.
        /// </summary>
        /// <returns>The parse.</returns>
        /// <param name="file">File.</param>
        public IXLog Parse(FileInfo file)
        {
            IXLog result = null;
            foreach (XParser parser in XParserRegistry.Instance.GetAvailable())
            {
                if (parser.CanParse(file))
                {
                    try
                    {
                        result = parser.Parse(file.OpenRead());
                        return result;
                    }
                    catch (Exception)
                    {
                        XLogging.Trace(String.Format("Attempted to parse file {0} with parser {1}",
                                                     file.FullName, parser.Name));
                    }
                }
            }
            throw new Exception("No suitable parser could be found!");
        }

    }
}
