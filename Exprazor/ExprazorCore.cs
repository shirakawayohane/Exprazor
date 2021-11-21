using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Exprazor
{
    internal class ExprazorCore
    {
        public enum NodeType
        {
            SSR_NODE = 1,
            TEXT_NODE = 3
        }

        public string SVG_NS = "http://www.w3.org/2000/svg";

        public string CreateClass(string str) => str;
        public string CreateClass(string[] strArray) => string.Join(" ", strArray.Where(x => string.IsNullOrEmpty(x)));
        public string CreateClass(Dictionary<string, object> dic)
        {
            var ret = string.Empty;
            foreach (var (k, v) in dic)
            {
                if (v is bool b && b)
                {
                    ret += k + " ";
                }
                if (v is string s)
                {
                    ret += k + " ";
                }
            }

            return ret.TrimEnd();
        }

        bool ShouldRestart(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            var keys = a.Keys.Union(b.Keys);
            foreach (var key in keys)
            {
                if (!a.ContainsKey(key)) return false;
                object? action = null;
                if (a[key] is Action act)
                {
                    action = act;
                }
                else if (a[key] is Array arr && arr.Length > 0 && arr.GetValue(0) is Action _act)
                {
                    action = _act;
                }
                if (action != null)
                {
                    b[key] = action;
                } else
                {
                    return a[key] != b[key];
                }
            }
            return false;
        }

        delegate void Dispatcher();

        // 一旦飛ばす
        void PatchSubscriptions(object[] oldSubs, object[]? newSubs, Dispatcher dispatch)
        {
            int largerLength = Math.Max(oldSubs.Length, newSubs?.Length ?? 0);
            var subs = new object[largerLength];
            for (int i = 0; i < largerLength; i++)
            {
                var oldSub = oldSubs[i];
                var newSub = newSubs[i];

                //   subs[i] = newSub != null ? oldSub != null || newSub[0] != oldSub[0]
            }
        }

        record VDOM(object Key);

        object? GetKey(IHTMLNode? vdom) => vdom?.Key;

        public interface IHTMLNode
        {
            long Id { get; }
        }
        public record struct TextNode(string Text) : IHTMLNode
        {
            public long Id => ((IntPtr)GCHandle.Alloc(this, GCHandleType.Weak)).ToInt64();
        }

        public record struct HTMLNode(string Tag, Dictionary<string, object>? Attributes, IEnumerable<HTMLNode>? Children) : IHTMLNode
        {
            public long Id => ((IntPtr)GCHandle.Alloc(this, GCHandleType.Weak)).ToInt64();
        }

        internal interface DOMCommand
        {
            /// <summary>
            /// Treat Type as string at least until this starts working.
            /// </summary>
            string Type { get; }
        }

        internal record struct SetStringAttribute(long Id, string Key, string Value) : DOMCommand
        {
            public string Type => nameof(SetStringAttribute);
        }
        internal record struct SetNumberAttribute(long Id, string Key, decimal Value) : DOMCommand
        {
            public string Type => nameof(SetNumberAttribute);
        }
        internal record struct SetBooleanAttribute(long Id, string Key, bool Value) : DOMCommand
        {
            public string Type => nameof(SetBooleanAttribute);
        }
        internal record struct RemoveAttribute(long Id, string Key) : DOMCommand
        {
            public string Type => nameof(RemoveAttribute);
        }

        internal record struct SetVoidCallback(long Id, string Key, long ActPtr) : DOMCommand
        {
            public string Type => nameof(SetVoidCallback);
        }
        internal record struct RemoveCallback(long Id, string Key) : DOMCommand
        {
            public string Type => nameof(RemoveCallback);
        }
        internal record struct CreateTextNode(long Id, string Text) : DOMCommand
        {
            public string Type => nameof(CreateTextNode);
        }
        internal record struct CreateElement(long Id, string Tag) : DOMCommand
        {
            public string Type => nameof(CreateElement);
        }
        internal record struct AppendChild(long ParentId, long NewId, string Tag) : DOMCommand
        {
            public string Type => nameof(AppendChild);
        }
        internal record struct SetTextNodeValue(long Id, string Text) : DOMCommand
        {
            public string Type => nameof(SetTextNodeValue);
        }
        internal record struct InsertBefore(long ParentId, long NewId, long BeforeId) : DOMCommand
        {
            public string Type => nameof(InsertBefore);
        }
        internal record struct RemoveChild(long ParentId, long ChildId) : DOMCommand
        {
            public string Type => nameof(RemoveChild);
        }

        void PatchAttribute(long nodeId, string key, object? oldValue, object? newValue, in List<DOMCommand> commands) {
            if (key == "key") { }
            else if(key.StartsWith("on"))
            {
                if(newValue == null)
                {
                    commands.Add(new RemoveCallback(nodeId, key));
                }
                else if(newValue is Action newAct && Object.ReferenceEquals(newValue, oldValue) == false)
                {
                    commands.Add(new SetVoidCallback(nodeId, key, ((long)((IntPtr)GCHandle.Alloc(newAct, GCHandleType.Weak)))));
                }
            } else
            {
                if(newValue == null)
                {
                    commands.Add(new RemoveAttribute(nodeId, key));
                } else if(oldValue == null || !newValue.Equals(oldValue))
                {
                    if(newValue is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal)
                    {
                        commands.Add(new SetNumberAttribute(nodeId, key, (decimal)newValue));
                    } else if(newValue is string str)
                    {
                        commands.Add(new SetStringAttribute(nodeId, key, str));
                    } else if(newValue is bool b)
                    {
                        commands.Add(new SetBooleanAttribute(nodeId, key, b));
                    }
                }
            }
        }

        /// <returns>Id of created node</returns>
        long CreateNode(IHTMLNode vdom, in List<DOMCommand> commands)
        {
            if (vdom is TextNode textNode)
            {
                commands.Add(new CreateTextNode(textNode.Id, textNode.Text));
                return textNode.Id;
            }
            else if(vdom is HTMLNode htmlNode)
            {
                commands.Add(new CreateElement(htmlNode.Id, htmlNode.Tag));
                if (htmlNode.Attributes != null)
                {
                    foreach (var (key, value) in htmlNode.Attributes)
                    {
                        PatchAttribute(htmlNode.Id, key, null, value, in commands);
                    }
                }
                if(htmlNode.Children != null)
                {
                    foreach(var child in htmlNode.Children)
                    {
                       var createdId = CreateNode(child, in commands);
                        commands.Add(new AppendChild(htmlNode.Id, createdId, child.Tag));
                    }
                }
            }

            throw new Exception("Unreachable code reached.");
        }

        void Patch(HTMLNode parent, long nodeId, IHTMLNode? oldVNode, IHTMLNode newVNode, object listener, in List<DOMCommand> commands)
        {
            if(oldVNode == newVNode)
            {
            } else if(oldVNode is TextNode oldTextNode && newVNode is TextNode newTextNode)
            {
                if(oldTextNode.Text != newTextNode.Text)
                {
                    commands.Add(new SetTextNodeValue(newTextNode.Id, newTextNode.Text));
                }
            } else if(oldVNode == null || oldVNode is HTMLNode _oldHtmlNode && newVNode is HTMLNode _newHtmlNode && _oldHtmlNode.Tag != _newHtmlNode.Tag)
            {
                var newId = CreateNode(newVNode, in commands);
                commands.Add(new InsertBefore(parent.Id, newId, nodeId));
                if(oldVNode != null)
                {
                    commands.Add(new RemoveChild(parent.Id, oldVNode.Id));
                }
            } else if(oldVNode is HTMLNode oldHtmlNode  && newVNode is HTMLNode newHtmlNode)
            {
                IHTMLNode tmpVKid;
                IHTMLNode oldVKid;

                object oldKey;
                object newKey;

                var oldAttributes = oldHtmlNode.Attributes;
                var newAttributes = newHtmlNode.Attributes;

                var oldVKids = oldHtmlNode.Children?.ToArray();
                var newVKids = newHtmlNode.Children?.ToArray();

                var oldHead = 0;
                var newHead = 0;
                var oldTail = oldVKids.Length - 1;
                var newTail = newVKids.Length - 1;

                // Compare Attributes, if any diff, patch and make commands.
                foreach(var key in (oldAttributes?.Keys ?? Enumerable.Empty<string>())
                .Union(newAttributes?.Keys ?? Enumerable.Empty<string>()))
                {
                    object? oldValue = null;
                    object? newValue = null;
                    oldAttributes?.TryGetValue(key, out oldValue);
                    newAttributes?.TryGetValue(key, out newValue);
                    if(oldValue != newValue)
                    {
                        PatchAttribute(nodeId, key, oldValue, newValue, commands);
                    }
                }

                while(newHead <= newTail && oldHead <= oldTail)
                {
                    if((oldKey = getKey(oldVKids[oldHead]))) == null || oldKey !== GetKey
                }
            }
        }

    }
}
