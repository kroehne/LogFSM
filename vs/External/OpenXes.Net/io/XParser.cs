using System;
using System.IO;
using System.Collections.Generic;
using OpenXesNet.model;

namespace OpenXesNet.io
{
    /// <summary>
    /// This abstract class describes a parser for reading XES models from a given input stream.
    /// </summary>
    public abstract class XParser : IDisposable
    {
        /// <summary>
        /// Returns the name of this parser or, more specifically, the name of the format it can process.
        /// </summary>
        /// <value>The name.</value>
        public abstract string Name { get; }

        /// <summary>
        /// Returns a brief description of this parser.
        /// </summary>
        /// <value>The description.</value>
        public abstract string Description { get; }

        /// <summary>
        /// Returns the name of the author of this parser.
        /// </summary>
        /// <value>The author.</value>
        public abstract string Author { get; }

        /// <summary>
        /// Checks whether this parser can handle the given file.
        /// </summary>
        /// <returns>Whether this parser can handle the given file.</returns>
        /// <param name="fileInfo">File to check against parser.</param>
        public abstract bool CanParse(FileInfo fileInfo);

        /// <summary>
        /// Parses the given input stream, and returns the XLog instances extracted.
        /// </summary>
        /// <returns>A XLog instance read from the given input stream. If no XLog 
        /// instance could be parsed, the parser is expected to throw an exception.</returns>
        /// <param name="stream">Stream to read XLog instances from.</param>
        public abstract IXLog Parse(Stream stream);

        /// <summary>
        /// Parses the given file, and returns the XLog instances extracted.The 
        /// file is first checked against this parser, to check whether it can be 
        /// handled. If the parser cannot handle the given file, or the extraction 
        /// itself fails,the parser should raise an <code>FileNotFoundException</code>.
        /// </summary>
        /// <returns>XLog instance parsed from the given file.</returns>
        /// <param name="fileName">The file to be parsed.</param>
        public abstract IXLog Parse(string fileName);

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns whether the given file name ends (ignoring the case) with the given suffix.
        /// </summary>
        /// <returns>Whether the given file name ends (ignoring the case) with the given suffix.</returns>
        /// <param name="name">The given suffix.</param>
        /// <param name="suffix">Suffix.</param>
        protected bool EndsWith(String name, String suffix)
        {
            if ((name == null) || (suffix == null))
            {
                return false;
            }
            int i = name.Length - suffix.Length;
            if (i < 0)
            {
                return false;
            }
            return name.Substring(i).Equals(suffix, StringComparison.InvariantCultureIgnoreCase);
        }

        public abstract void Dispose();
    }
}
