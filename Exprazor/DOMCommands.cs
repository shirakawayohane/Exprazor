using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exprazor
{
#if DEBUG
    using CommandType = System.String;
    using Id = System.Int64;
#else
    using CommandType = System.UInt32;
#endif

    public interface DOMCommand
    {
        /// <summary>
        /// Treat Type as string at least until this starts working.
        /// </summary>
        CommandType Type { get; }
    }

    public record struct SetStringAttribute(Id Id, string Key, string Value) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(SetStringAttribute);
#else
            public CommandType Type => 1;
#endif
    }
    public record struct SetNumberAttribute(Id Id, string Key, decimal Value) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(SetNumberAttribute);
#else
            public CommandType Type => 1;
#endif
    }
    public record struct SetBooleanAttribute(Id Id, string Key, bool Value) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(SetBooleanAttribute);
#else
            public CommandType Type => 1;
#endif
    }
    public record struct RemoveAttribute(Id Id, string Key) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(RemoveAttribute);
#else
            public CommandType Type => 1;
#endif
    }

    public record struct SetVoidCallback(Id Id, string Key, Id ActPtr) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(SetVoidCallback);
#else
            public CommandType Type => 1;
#endif
    }
    public record struct RemoveCallback(Id Id, string Key) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(RemoveCallback);
#else
            public CommandType Type => 1;
#endif
    }
    public record struct CreateTextNode(Id Id, string Text) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(CreateTextNode);
#else
            public CommandType Type => 1;
#endif
    }
    public record struct CreateElement(Id Id, string Tag) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(CreateElement);
#else
            public CommandType Type => 1;
#endif
    }
    public record struct AppendChild(Id ParentId, Id NewId) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(AppendChild);
#else
            public CommandType Type => 1;
#endif
    }
    public record struct SetTextNodeValue(Id Id, string Text) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(SetTextNodeValue);
#else
            public CommandType Type => 1;
#endif
    }
    public record struct InsertBefore(Id ParentId, Id NewId, Id BeforeId) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(InsertBefore);
#else
            public CommandType Type => 1;
#endif
    }
    public record struct RemoveChild(Id ParentId, Id ChildId) : DOMCommand
    {
#if DEBUG
        public CommandType Type => nameof(RemoveChild);
#else
            public CommandType Type => 1;
#endif
    }
}
