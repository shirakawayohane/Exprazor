using Exprazor;
using MessagePack;
using MessagePack.Formatters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Builder
{
    using Id = System.Int32;

    public static partial class ExprazorBuilderExtentions
    {
        [MessagePackFormatter(typeof(FromServerCommandFormatter))]
        interface FromServerCommand { }
        record struct HandleCommands(IEnumerable<DOMCommand> Commands) : FromServerCommand; // 0

        [MessagePackFormatter(typeof(FromClientCommandFormatter))]
        interface FromClientCommand { }
        record struct Connected() : FromClientCommand { } // 0
        record struct InvokeVoid(Id Id, string Key) : FromClientCommand { } // 1
        record struct InvokeWithString(Id Id, string Key, string Argument) : FromClientCommand { } // 2

        class FromServerCommandFormatter : IMessagePackFormatter<FromServerCommand>
        {
            public FromServerCommand Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                throw new NotSupportedException("Deserializing FromServerCommand is not supported for now.");
            }

            public void Serialize(ref MessagePackWriter writer, FromServerCommand value, MessagePackSerializerOptions options)
            {
                switch(value)
                {
                    case HandleCommands hc:
                        writer.WriteArrayHeader(2);
                        writer.WriteUInt8(0);
                        MessagePackSerializer.Serialize(ref writer, hc.Commands.ToArray(), options);
                        break;
                }
            }
        }

        class FromClientCommandFormatter : IMessagePackFormatter<FromClientCommand>
        {
            public FromClientCommand Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                reader.ReadArrayHeader();
                var type = reader.ReadInt32();
                return type switch
                {
                    0 => new Connected(),
                    1 => new InvokeVoid(reader.ReadInt32(), reader.ReadString()),
                    2 => new InvokeWithString(reader.ReadInt32(), reader.ReadString(), reader.ReadString()),
                    _ => throw new InvalidDataException()
                };
            }

            public void Serialize(ref MessagePackWriter writer, FromClientCommand value, MessagePackSerializerOptions options)
            {
                throw new NotSupportedException("Serializing FromServerCommand is not supported for now.");
            }
        }

    }
}
