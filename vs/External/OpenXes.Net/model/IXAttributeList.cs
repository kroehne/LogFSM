namespace OpenXesNet.model
{
    /// <summary>
    /// Attribute containing a list (may be empty) of child attributes. These children
    /// are ordered, and keys of these child attributes need not be unique. The value of
    /// a list is derived from the values of its child elemenets.
    /// </summary>
    /// <example>
    /// <list key="revisions">
    ///     <string key="name" value="XES standard"/>
    ///     <boolean key="stable" value="true"/> 
    ///     <string key="revision" value="2.0"/>
    ///     <string key="revision" value="1.4"/>
    ///     <string key="revision" value="1.3"/>
    ///     <string key="revision" value="1.2"/>
    ///     <string key="revision" value="1.1"/>
    ///     <string key="revision" value="1.0"/>
    /// </list>
    /// </example>
    public interface IXAttributeList : IXAttributeCollection
    {
    }
}
