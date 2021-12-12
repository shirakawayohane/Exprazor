using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Formatters;

namespace Exprazor
{
    using Id = System.UInt64;
    [MessagePackFormatter(typeof(DOMCommandFormatter))]
    public interface DOMCommand {}

    public record struct SetStringAttribute(Id Id, string Key, string Value) : DOMCommand;
    public record struct SetNumberAttribute(Id Id, string Key, double Value) : DOMCommand;
    public record struct SetBooleanAttribute(Id Id, string Key, bool Value) : DOMCommand;
    public record struct SetTextNodeValue(Id Id, string Text) : DOMCommand;
    public record struct RemoveAttribute(Id Id, string Key) : DOMCommand;
    public record struct CreateTextNode(Id Id, string Text) : DOMCommand;
    public record struct CreateElement(Id Id, string Tag) : DOMCommand;
    public record struct AppendChild(Id ParentId, Id NewId) : DOMCommand;
    public record struct InsertBefore(Id ParentId, Id NewId, Id BeforeId) : DOMCommand;
    public record struct RemoveChild(Id ParentId, Id ChildId) : DOMCommand;
    public record struct RemoveCallback(Id Id, string Key) : DOMCommand;
    public record struct SetVoidCallback(Id Id, string Key) : DOMCommand;
    public record struct SetStringCallback(Id Id, string Key) : DOMCommand;

    public class DOMCommandFormatter : IMessagePackFormatter<DOMCommand>
    {
        public DOMCommand Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            reader.ReadArrayHeader();
            return reader.ReadByte() switch
            {
                0 => new SetStringAttribute(reader.ReadUInt32(), reader.ReadString(), reader.ReadString()),
                1 => new SetNumberAttribute(reader.ReadUInt32(), reader.ReadString(), reader.ReadDouble()),
                2 => new SetBooleanAttribute(reader.ReadUInt32(), reader.ReadString(), reader.ReadBoolean()),
                3 => new RemoveAttribute(reader.ReadUInt32(), reader.ReadString()),
                4 => new SetTextNodeValue(reader.ReadUInt32(), reader.ReadString()),
                10 => new CreateTextNode(reader.ReadUInt32(), reader.ReadString()),
                11 => new CreateElement(reader.ReadUInt32(), reader.ReadString()),
                20 => new AppendChild(reader.ReadUInt32(), reader.ReadUInt32()),
                21 => new InsertBefore(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32()),
                22 => new RemoveChild(reader.ReadUInt32(), reader.ReadUInt32()),
                30 => new RemoveCallback(reader.ReadUInt32(), reader.ReadString()),
                31 => new SetVoidCallback(reader.ReadUInt32(), reader.ReadString()),
                32 => new SetStringCallback(reader.ReadUInt32(), reader.ReadString()),
                _ => throw new InvalidDataException(),
            };
        }

        public void Serialize(ref MessagePackWriter writer, DOMCommand value, MessagePackSerializerOptions options)
        {
            switch (value)
            {
                case SetStringAttribute ssa:
                    writer.WriteArrayHeader(4);
                    writer.WriteUInt8(0);
                    writer.Write(ssa.Id);
                    writer.Write(ssa.Key);
                    writer.Write(ssa.Value);
                    break;
                case SetNumberAttribute sna:
                    writer.WriteArrayHeader(4);
                    writer.WriteUInt8(1);
                    writer.Write(sna.Id);
                    writer.Write(sna.Key);
                    writer.Write(sna.Value);
                    break;
                case SetBooleanAttribute sba:
                    writer.WriteArrayHeader(4);
                    writer.WriteUInt8(2);
                    writer.Write(sba.Id);
                    writer.Write(sba.Key);
                    writer.Write(sba.Value);
                    break;
                case RemoveAttribute ra:
                    writer.WriteArrayHeader(3);
                    writer.WriteUInt8(3);
                    writer.Write(ra.Id);
                    writer.Write(ra.Key);
                    break;
                case SetTextNodeValue stn:
                    writer.WriteArrayHeader(3);
                    writer.WriteUInt8(4);
                    writer.Write(stn.Id);
                    writer.Write(stn.Text);
                    break;
                case CreateTextNode ct:
                    writer.WriteArrayHeader(3);
                    writer.WriteUInt8(10);
                    writer.Write(ct.Id);
                    writer.Write(ct.Text);
                    break;
                case CreateElement ce:
                    writer.WriteArrayHeader(3);
                    writer.WriteUInt8(11);
                    writer.Write(ce.Id);
                    writer.Write(ce.Tag);
                    break;
                case AppendChild ac:
                    writer.WriteArrayHeader(3);
                    writer.WriteUInt8(20);
                    writer.Write(ac.ParentId);
                    writer.Write(ac.NewId);
                    break;
                case InsertBefore ib:
                    writer.WriteArrayHeader(4);
                    writer.WriteUInt8(21);
                    writer.Write(ib.ParentId);
                    writer.Write(ib.NewId);
                    writer.Write(ib.BeforeId);
                    break;
                case RemoveChild rc:
                    writer.WriteArrayHeader(3);
                    writer.WriteUInt8(22);
                    writer.Write(rc.ParentId);
                    writer.Write(rc.ChildId);
                    break;
                case RemoveCallback rmc:
                    writer.WriteArrayHeader(3);
                    writer.WriteUInt8(30);
                    writer.Write(rmc.Id);
                    writer.Write(rmc.Key);
                    break;
                case SetVoidCallback svc:
                    writer.WriteArrayHeader(3);
                    writer.WriteUInt8(31);
                    writer.Write(svc.Id);
                    writer.Write(svc.Key);
                    break;
                case SetStringCallback ssc:
                    writer.WriteArrayHeader(3);
                    writer.WriteUInt8(32);
                    writer.Write(ssc.Id);
                    writer.Write(ssc.Key);
                    break;
            }
        }
    }

}
