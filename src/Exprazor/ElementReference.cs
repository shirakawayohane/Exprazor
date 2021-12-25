using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exprazor
{
    using Id = System.Int32;

    [MessagePackFormatter(typeof(ElementReferenceFormatter))]
    public struct ElementReference
    {
        internal Id _id { get; set; }
    }

    public class ElementReferenceFormatter : IMessagePackFormatter<ElementReference>
    {
        public ElementReference Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return new ElementReference
            {
                _id = reader.ReadInt32(),
            };
        }

        public void Serialize(ref MessagePackWriter writer, ElementReference value, MessagePackSerializerOptions options)
        {
            writer.WriteInt32(value._id);
        }
    }
}
