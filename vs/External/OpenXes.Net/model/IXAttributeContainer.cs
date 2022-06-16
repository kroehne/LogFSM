using System;
namespace OpenXesNet.model
{
    /// <summary>
    /// Attribute containing any number (may be emprty) of child attributes. Child attributes
    /// are not ordered. The value of a container is derived from the values of its
    /// child attributes.
    /// </summary>
    /// <example>
    /// <container key="location">
    ///     <string key = "street" value="Den Dolech"/>
    ///     <int key = "number" value="2"/>
    ///     <string key = "zip" value="5612 AZ"/>
    ///     <string key = "city" value="Eindhoven"/>
    ///     <string key = "country" value="The Netherlands"/>
    /// </container>
    /// </example>
    public interface IXAttributeContainer : IXAttributeCollection
    {
    }
}
