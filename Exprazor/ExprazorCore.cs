using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Exprazor;
using Attributes = Dictionary<string, object>;
using Id = System.Int64;

internal static class ExprazorCore
{
    /// <returns>Id of created node</returns>
    internal static Id CreateNode(ExprazorApp context, Id parentId, IExprazorNode vdom, in List<DOMCommand> commands)
    {
        if (vdom is Component) throw new ArgumentException($"Passing Component to {CreateNode} directry is invalid.", nameof(vdom));
        var newId = context.NextId();
        vdom.NodeId = newId;
        if (vdom is TextNode textNode)
        {
            commands.Add(new CreateTextNode(newId, textNode.Text));
        }
        else if (vdom is HTMLNode htmlNode)
        {
            commands.Add(new CreateElement(newId, htmlNode.Tag));
            if (htmlNode.Attributes != null)
            {
                foreach (var (key, value) in htmlNode.Attributes)
                {
                    PatchAttribute(newId, key, null, value, in commands);
                }
            }
            if (htmlNode.Children != null)
            {
                foreach (var child in htmlNode.Children)
                {
                    var createdId = CreateNode(context, newId, child, in commands);
                    commands.Add(new AppendChild(newId, createdId));
                }
            }
        }

        return vdom.NodeId;
    }
    static void PatchAttribute(Id nodeId, string key, object? oldValue, object? newValue, in List<DOMCommand> commands)
    {
        if (key == "key") { }
        else if (key.StartsWith("on"))
        {
            if (newValue == null)
            {
                commands.Add(new RemoveCallback(nodeId, key));
            }
            else if (newValue is Action newAct && Object.ReferenceEquals(newValue, oldValue) == false)
            {
                commands.Add(new SetVoidCallback(nodeId, key, ((Id)((IntPtr)GCHandle.Alloc(newAct, GCHandleType.Weak)))));
            }
        }
        else
        {
            if (newValue == null)
            {
                commands.Add(new RemoveAttribute(nodeId, key));
            }
            else if (oldValue == null || !newValue.Equals(oldValue))
            {
                if (newValue is byte or sbyte or short or ushort or int or long or ulong or ulong or float or double or decimal)
                {
                    commands.Add(new SetNumberAttribute(nodeId, key, (decimal)newValue));
                }
                else if (newValue is string str)
                {
                    commands.Add(new SetStringAttribute(nodeId, key, str));
                }
                else if (newValue is bool b)
                {
                    commands.Add(new SetBooleanAttribute(nodeId, key, b));
                }
            }
        }
    }

    // まずは、ルート同士で、タグとアトリビュートを比較して差分を発行する。
    // 次に、子ノードに対して、再帰的に同じ比較を行うが、
    // keyが同じ場合は比較、
    // oldNodeには存在するが、newNodeには存在しないものはまるごと削除
    // oldNodeには存在するが、newNodeには存在するものはまるごと追加し、子供に対してもPatchする

    // Patchが長すぎるので、LINQを使ってでも短く書き直す
    static internal void Patch(ExprazorApp context, Id parentId, Id nodeId, IExprazorNode? oldVNode, IExprazorNode newVNode, in List<DOMCommand> commands)
    {
        if (oldVNode == newVNode) return;


        if (oldVNode == null || oldVNode.GetType() != newVNode.GetType())
        {
            oldVNode = null;
        }
        if (oldVNode == null || oldVNode.GetType() != newVNode.GetType())
        {
            if (newVNode is Component newComponent)
            {
                var newState = newComponent.PropsChanged(newComponent.Props);
                newComponent.State = newState;
                newComponent.lastTree = newComponent.Render(newState);
                var createdId = CreateNode(context, parentId, newVNode, commands);
                commands.Add(new InsertBefore(parentId, createdId, nodeId));
            } else
            {
                var createdId = CreateNode(context, parentId, newVNode, commands);
                commands.Add(new InsertBefore(parentId, createdId, nodeId));
            }

            if(oldVNode != null)
            {
                commands.Add(new RemoveChild(parentId, nodeId));
            }

            return;
        }

        if (oldVNode is TextNode oldTextNode && newVNode is TextNode newTextNode)
        {
            if (oldTextNode.Text != newTextNode.Text)
            {
                commands.Add(new SetTextNodeValue(newTextNode.NodeId, newTextNode.Text));
            }
        }
        else if (oldVNode is Component oldComponent && newVNode is Component newComponent)
        {
            if (!oldComponent.Props.Equals(newComponent.Props))
            {
                var newState = newComponent.PropsChanged(newComponent.Props);
                var newTree = newComponent.Render(newState);
                Patch(context, newComponent.ParentId, newComponent.NodeId, newComponent.lastTree, newTree, commands);
                newComponent.lastTree = newTree;
            }
            else
            {
                newComponent.State = oldComponent.State;
                newComponent.lastTree = oldComponent.lastTree;
            }
        }
        else if (oldVNode is HTMLNode oldHTMLNode && newVNode is HTMLNode newHTMLNode)
        {
            if (oldHTMLNode.Tag != newHTMLNode.Tag)
            {
                var createId = CreateNode(context, parentId, newVNode, commands);
                commands.Add(new InsertBefore(parentId, createId, nodeId));
                commands.Add(new RemoveChild(parentId, oldVNode.NodeId));
            }
            else
            {
                foreach (var key in (oldHTMLNode.Attributes?.Keys ?? Enumerable.Empty<string>())
                .Union(newHTMLNode.Attributes?.Keys ?? Enumerable.Empty<string>()))
                {
                    object? oldValue = null;
                    object? newValue = null;
                    oldHTMLNode.Attributes?.TryGetValue(key, out oldValue);
                    newHTMLNode.Attributes?.TryGetValue(key, out newValue);
                    if (oldValue != newValue)
                    {
                        PatchAttribute(nodeId, key, oldValue, newValue, commands);
                    }
                }
            }

            IExprazorNode[] oldChildren = oldHTMLNode.Children?.ToArray() ?? Array.Empty<IExprazorNode>();
            IExprazorNode[] newChildren = oldHTMLNode.Children?.ToArray() ?? Array.Empty<IExprazorNode>();

        }
    }
}
