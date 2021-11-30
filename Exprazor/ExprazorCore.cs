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
    internal static Id CreateNode(ExprazorApp context, IExprazorNode vdom, in List<DOMCommand> commands)
    {
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
                    var createdId = CreateNode(context, child, in commands);
                    commands.Add(new AppendChild(newId, createdId));
                }
            }
        } else if(vdom is Component component)
        {
            var initialState = component.PropsChanged(component.Props);
            component.State = initialState;
            component.lastTree = component.Render(initialState);
            return CreateNode(context, component.lastTree, commands);
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
    static internal Id Patch(ExprazorApp context, Id parentId, Id nodeId, IExprazorNode? oldVNode, IExprazorNode newVNode, in List<DOMCommand> commands)
    {
        if (oldVNode == newVNode) return oldVNode.NodeId;

        if (oldVNode == null || oldVNode.GetType() != newVNode.GetType())
        {
            var createdId = CreateNode(context, newVNode, commands);
            commands.Add(new InsertBefore(parentId, createdId, nodeId));

            if (oldVNode != null)
            {
                commands.Add(new RemoveChild(parentId, nodeId));
            }

            return newVNode.NodeId;
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
                var createId = CreateNode(context, newVNode, commands);
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

            var oldChildren = oldHTMLNode.Children?.ToArray() ?? Array.Empty<IExprazorNode>();
            var newChildren = newHTMLNode.Children?.ToArray() ?? Array.Empty<IExprazorNode>();
            int oldHead = 0;
            int oldTail = oldChildren.Length - 1;
            int newHead = 0;
            int newTail = newChildren.Length - 1;

            // STEP 0:
            // Patch same key nodes from both side.
            // A B C d ... x Y Z    =>  d ... x
            // A B C D ... X Y Z        D ... X
            while(true)
            {
                var oldKey = oldChildren[oldHead].GetKey();
                if (oldKey == null || oldKey != newChildren[newHead].GetKey()) break;
                Patch(context, nodeId, oldChildren[oldHead].NodeId, oldChildren[oldHead], newChildren[newHead], commands);
                oldHead++; newHead++;
            }
            while(true)
            {
                var oldKey = oldChildren[oldTail].GetKey();
                if (oldKey == null || oldKey != newChildren[newTail].GetKey()) break;
                Patch(context, nodeId, oldChildren[oldTail].NodeId, oldChildren[oldTail], newChildren[newTail], commands);
                oldTail--; newTail--;
            }
            // STEP 1:
            // old: A B C D E       => A B X... C D E
            // new: A B X...  C D E
            //          ↑ insert new node if vnode has inserted.
            if(oldHead > oldTail)
            {
                while(newHead <= newTail)
                {
                    var createdId = CreateNode(context, newChildren[newHead], commands);
                    commands.Add(new InsertBefore(nodeId, createdId, oldVNode.NodeId));
                }
            // STEP 2:
            // old: A B C D E => A B E
            // new: A B _ _←E
            //          ↑ remove node if vnode has removed
            } else if(newHead > newTail)
            {
                while(oldHead <= oldTail)
                {
                    commands.Add(new RemoveChild(nodeId, oldChildren[oldHead++].NodeId));
                }
            } else
            {
                var keyed = oldChildren.Where(x => x.GetKey() != null).ToDictionary(x => x.GetKey()!, x => x);
                var nullPatched = new List<IExprazorNode>();
                var newKeyed = new HashSet<object>();
                while(newHead <= newTail)
                {
                    var oldChild = oldChildren[oldHead];
                    var newChild = newChildren[newHead];
                    var oldKey = oldChild.GetKey();
                    var newKey = newChild.GetKey();
                    var nextKey = oldChildren[oldHead + 1].GetKey();

                    // N is null. x and X are different.

                    // STEP 3:
                    // old : N x y z ...    =>    x y z ...
                    // new : X Y Z ...   (Remove) X Y Z ...
                    if (newKey != null && newKey.Equals(nextKey) && oldKey == null)
                    {
                        commands.Add(new RemoveChild(nodeId, newTail));
                        oldHead++;
                        continue;
                    }
                    // STEP 4:
                    // if already patched, then skip.
                    // newKeyd: [ A ...]
                    // old : A x y z...  =>    x y z...
                    // new : X Y Z...  (Skip)  X Y Z...
                    if(oldKey != null && newKeyed.Contains(oldKey))
                    {
                        oldHead++;
                        continue;
                    }
                    // STEP 5:
                    // if both null, patch and go next.
                    // old : N x y...    =>     x y...
                    // new : N X Y...  (Patch)  X Y...
                    if(newKey == null && oldKey == null)
                    {
                        Patch(context, nodeId, oldChild.NodeId, oldChild, newChild, commands);
                        oldHead++; newHead++;
                        continue;
                    }
                    // STEP 6:
                    // If newKey is null, but old key is not null, skip old.
                    // old : x y z...   =>    y z...
                    // new : N X Y... (Skip)  N X Y...
                    if(newKey == null && oldKey != null)
                    {
                        // Same as FirstOrDefault
                        IExprazorNode? patchTarget = null;
                        for(int i = oldHead; i < oldTail; i++)
                        {
                            var _oldChild = oldChildren[i];
                            if (_oldChild.GetKey() == null && _oldChild.GetType() != newChild.GetType() && nullPatched.Contains(_oldChild) == false)
                            {
                                patchTarget = _oldChild;
                            }
                        }
                        if(patchTarget != null)
                        {
                            nullPatched.Add(patchTarget);
                            commands.Add(new InsertBefore(nodeId, patchTarget.NodeId, oldChild.NodeId));
                            Patch(context, nodeId, patchTarget.NodeId, patchTarget, newChild, commands);
                            newHead++;
                        }
                        oldHead++;
                        continue;
                    }
                    // STEP 7:
                    // if both are same, Just patch and proceed.
                    // old : A y z...  => 
                    // new : A Y Z... (Patch)
                    if(oldKey != null && oldKey.Equals(newKey))
                    {
                        Patch(context, nodeId, oldChild.NodeId, oldChild, newChild, commands);
                        oldHead++; newHead++;
                        continue;
                    }
                    // STEP 8:
                    // If old keys contains current newKey, insert it into current head and patch.
                    //      ---------  (A will be skipped from next time.)
                    //      ↓       ↑
                    // old : x y... A    =>    A x y...    =>     x y...
                    // new : A X Y...  (Sort)  A X Y...  (Patch)  X Y...
                    if(keyed.TryGetValue(newKey!, out var oldChildWithSameKey))
                    {
                        commands.Add(new InsertBefore(nodeId, oldChildWithSameKey.NodeId, oldChild.NodeId));
                        Patch(context, nodeId, oldChildWithSameKey.NodeId, oldChildWithSameKey, newChild, commands);
                        newKeyed.Add(newKey!);
                        newHead++; // Don't skip oldChild.
                        continue;
                    }
                    // STEP 9:
                    // if old keys don't contians 'X' and old one is null, create X node and proceed.
                    // old : N x y...    =>       N x y... 
                    // new : X Y...   (Create X)  Y...
                    Patch(context, nodeId, oldChild.NodeId, null, newChild, commands);
                    newHead++;
                }

                // STEP 10:
                // Remove disappeared old nodes.
                while(oldHead <= oldTail)
                {
                    if(oldChildren[oldHead].GetKey() == null)
                    {
                        commands.Add(new RemoveChild(nodeId, oldChildren[oldHead].NodeId));
                        oldHead++;
                    }
                }
                foreach(var (key, _oldVNode) in keyed)
                {
                    if (newKeyed.Contains(key) == false) commands.Add(new RemoveChild(nodeId, _oldVNode.NodeId));
                }
            }

            newVNode.NodeId = oldVNode.NodeId;
        }
    }
}
