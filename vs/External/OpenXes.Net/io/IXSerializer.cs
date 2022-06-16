using OpenXesNet.model;
using System.IO;

namespace OpenXesNet.io
{
    /// <summary>
    /// This interfaces defines the capabilities of a serialization for the XES 
    /// format, into a given representation.
    /// </summary>
    public interface IXSerializer
    {
        /// <summary>
        /// Returns the human-readable name of this serialization.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Returns a brief description of this serialization.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; }

        /// <summary>
        /// Returns the name of this serialization's author.
        /// </summary>
        /// <value>The author.</value>
        string Author { get; }

        /// <summary>
        /// Returns an array of possible file suffices for this serialization.
        /// </summary>
        /// <value>The suffices.</value>
        string[] Suffices { get; }

        /// <summary>
        /// Serializes a given log to the given output stream.
        /// </summary>
        /// <returns>The serialize.</returns>
        /// <param name="log">Log to be serialized.</param>
        /// <param name="stream">Output stream for serialization.</param>
        void Serialize(IXLog log, Stream stream);
    }
}
