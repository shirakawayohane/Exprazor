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

    public static partial class ExprazorBuilderExtentions
    {
        public interface ServerCommand
        {
            ServerCommandType Type { get; }
        };
        record struct HandleCommands(IEnumerable<DOMCommand> Commands) : ServerCommand
        {
#if DEBUG
            public ServerCommandType Type => nameof(HandleCommands);
#else
            public ServerCommandType Type => 0;
#endif
        }

        record struct SetAsDevelopment() : ServerCommand
        {
#if DEBUG
            public ServerCommandType Type => nameof(SetAsDevelopment);
#else
            public ServerCommandType Type => 1;
#endif
        }

        public interface ClientCommand { }

        record struct Connected() : ClientCommand { } // "Hello"
        record struct InvokeVoid(Id Id, string Key) : ClientCommand { } // 1

        class ClientCommandDeserializer : JsonConverter<ClientCommand>
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
                    case "Hello":
                        return new Connected();
#if DEBUG
                    case nameof(InvokeVoid):
#else
                    case 1:
#endif
                        var id = reader.GetInt64();
                        reader.Read();
                        string? key = reader.GetString()!;

                        if (key == null) throw new InvalidDataException("Callback key cannot be null.");

                        reader.Read();
                        if (reader.TokenType != JsonTokenType.EndArray) throw new JsonException();

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

        class ServerCommandSerializer : JsonConverter<ServerCommand>
        {
            public override ServerCommand? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotSupportedException("ServerCommand is not intended to be deserialized on server for now.");
            }

            public override void Write(Utf8JsonWriter writer, ServerCommand value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                if(value is HandleCommands handleCommands)
                {
#if DEBUG
                    writer.WriteString(nameof(ServerCommand.Type), handleCommands.Type);
#else
                    writer.WriteNumber(nameof(ServerCommand.Type), handleCommands.Type);
#endif
                    writer.WritePropertyName(nameof(HandleCommands.Commands));
                    JsonSerializer.Serialize<IEnumerable<object>>(writer, handleCommands.Commands.Cast<object>(), options);
                }
                writer.WriteEndObject();
            }
        }

    }

}
