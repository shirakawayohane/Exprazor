using Exprazor;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Builder
{
    using Id = System.Int64;
#if DEBUG
    using ServerCommandType = System.String;
#else
    using ServerCommandType = System.Int32;
#endif

    public static partial class ExprazorApplicationBuilderExtentions
    {
        public interface ServerCommand
        {
            ServerCommandType Type { get; }
        };
        record HandleCommands(DOMCommand[] commands) : ServerCommand
        {
#if DEBUG
            public ServerCommandType Type => "handleCommands";
#else
            public Type Type => 0;
#endif
        }

        public interface ClientCommand { }

        record InvokeVoid(Id Id, string Key) : ClientCommand { } // 0

        public class ClientCommandDeserializer : JsonConverter<ClientCommand>
        {
            // Debug: ["invokeVoid", 1, "onclick"]
            // Prod: [0, 1, "onclick"]
            public override ClientCommand? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException();
                reader.Read();
#if DEBUG
                var type = reader.GetString();
#else
                var type = reader.GetInt32();
#endif
                reader.Read();
                switch (type)
                {
#if DEBUG
                    case nameof(InvokeVoid):
#else
                    case 0:
#endif
                        var id = reader.GetInt64();
                        reader.Read();
                        string? key = reader.GetString()!;
                        if (key == null) throw new InvalidDataException("Callback key cannot be null.");
                        if (reader.TokenType != JsonTokenType.EndArray) throw new JsonException();
                        reader.Read();
                        return new InvokeVoid(id, key);
                    default:
                        throw new InvalidDataException("Got unrecognized command.");
                }
            }

            public override void Write(Utf8JsonWriter writer, ClientCommand value, JsonSerializerOptions options)
            {
                throw new NotSupportedException("ClientCommand is not intended to be serialized on server for now.");
            }
        }

        public class ServerCommandSerializer : JsonConverter<ServerCommand>
        {
            public override ServerCommand? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotSupportedException("ServerCommand is not intended to be deserialized on server for now.");
            }

            public override void Write(Utf8JsonWriter writer, ServerCommand value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                var _type = options.PropertyNamingPolicy?.ConvertName(value.Type) ?? value.Type;
                writer.WritePropertyName(_type);
#if DEBUG
                writer.WriteStringValue(value.Type);
#else
#endif
            }
        }
    }

}
