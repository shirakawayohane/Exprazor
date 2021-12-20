using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exprazor
{
    public interface InvokeCommand { }
    public record struct InvokeVoid(string FuncName, object?[]? Arguments) : InvokeCommand;
    public record struct Invoke(string FuncName, object?[]? Arguments) : InvokeCommand;

    public class InvokeCommandFormatter : IMessagePackFormatter<InvokeCommand>
    {
        public InvokeCommand Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var length = reader.ReadArrayHeader();
            var type = reader.ReadByte();
            var funcName = reader.ReadString();

            throw new NotImplementedException();
        }

        public void Serialize(ref MessagePackWriter writer, InvokeCommand value, MessagePackSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
