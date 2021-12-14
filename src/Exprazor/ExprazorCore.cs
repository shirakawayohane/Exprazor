using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Exprazor;
using Attributes = Dictionary<string, object>;
using Id = System.Int32;

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
                    PatchAttribute(context, newId, key, null, value, in commands);
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
    static void PatchAttribute(ExprazorApp context, Id nodeId, string key, object? oldValue, object? newValue, in List<DOMCommand> commands)
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
                context.AddOrSetCallback(nodeId, key, newAct);
                commands.Add(new SetVoidCallback(nodeId, key));
            }
            else if (newValue is Action<string> stringAct && Object.ReferenceEquals(newValue, oldValue) == false)
            {
                context.AddOrSetCallback(nodeId, key, stringAct);
                commands.Add(new SetStringCallback(nodeId, key));
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
                    commands.Add(new SetNumberAttribute(nodeId, key, (double)newValue));
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
        newVNode.NodeId = nodeId;

        if (oldVNode == newVNode) return;

        if(oldVNode == null)
        {
            var createdId = CreateNode(context, newVNode, commands);
            commands.Add(new AppendChild(parentId, createdId));

            return;
        }
        
        if(oldVNode.GetType() != newVNode.GetType())
        {
            var createdId = CreateNode(context, newVNode, commands);
            commands.Add(new InsertBefore(parentId, createdId, nodeId));
            commands.Add(new RemoveChild(parentId, nodeId));
            oldVNode.Dispose();

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
                var createId = CreateNode(context, newVNode, commands);
                commands.Add(new InsertBefore(parentId, createId, nodeId));
                commands.Add(new RemoveChild(parentId, oldVNode.NodeId));
                oldVNode.Dispose();
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
                        PatchAttribute(context, nodeId, key, oldValue, newValue, commands);
                    }
                }
            }

            LinkedList<IExprazorNode> oldChildren = new LinkedList<IExprazorNode>(oldHTMLNode.Children ?? Enumerable.Empty<IExprazorNode>());
            LinkedList<IExprazorNode> newChildren = new LinkedList<IExprazorNode>(newHTMLNode.Children ?? Enumerable.Empty<IExprazorNode>());
            // STEP 0:
            // Patch same key nodes from both side.
            // A B C d ... x Y Z    =>  d ... x
            // A B C D ... X Y Z        D ... X
            while(oldChildren.Any() && newChildren.Any())
            {
                var oldChild = oldChildren.First!.Value;
                var newChild = newChildren.First!.Value;
                var oldKey = oldChild.GetKey();
                if (oldKey == null || oldKey != newChild.GetKey()) break;
                Patch(context, nodeId, oldChild.NodeId, oldChild, newChild, commands);
                oldChildren.RemoveFirst();
                newChildren.RemoveFirst();
            }
            while (oldChildren.Any() && newChildren.Any())
            {
                var oldChild = oldChildren.First!.Value;
                var newChild = newChildren.First!.Value;
                var oldKey = oldChild.GetKey();
                if (oldKey == null || oldKey != newChild.GetKey()) break;
                Patch(context, nodeId, oldChild.NodeId, oldChild, newChild, commands);
                oldChildren.RemoveLast();
                newChildren.RemoveLast();
            }
            // STEP 1:
            // old: A B C D E       => A B X...C D E
            // new: A B X...C D E 
            //          ↑ insert new node if vnode has inserted.
            if(!oldChildren.Any())
            {
                while(newChildren.Any())
                {
                    var createdId = CreateNode(context, newChildren.First!.Value, commands);
                    commands.Add(new InsertBefore(nodeId, createdId, oldVNode.NodeId));
                    newChildren.RemoveFirst();
                }
            // STEP 2:
            // old: A B C D E => A B E
            // new: A B     E
            //          ↑ remove node if vnode has removed
            } else if(!newChildren.Any())
            {
                while(oldChildren.Any())
                {
                    var nodeToRemove = oldChildren.First!.Value;
                    commands.Add(new RemoveChild(nodeId, nodeToRemove.NodeId));
                    nodeToRemove.Dispose();
                    oldChildren.RemoveFirst();
                }
            } else
            {
                var keyed = oldChildren.Where(x => x.GetKey() != null).ToDictionary(x => x.GetKey()!, x => x);
                // loop until all of the newChildren is patched.
                while (newChildren.Any())
                {
                    var oldChild = oldChildren.First();
                    var newChild = newChildren.First();
                    var oldKey = oldChild.GetKey();
                    var newKey = newChild.GetKey();
                    var nextKey = oldChildren.First?.Next?.Value?.GetKey();

                    // N is null. x and X are different.

                    // STEP 3:
                    // old : N x y z ...    =>    x y z ...
                    // new : X Y Z ...   (Remove) X Y Z ...
                    if (newKey != null && newKey.Equals(nextKey) && oldKey == null)
                    {
                        commands.Add(new RemoveChild(nodeId, oldChild.NodeId));
                        oldChild.Dispose();
                        oldChildren.RemoveFirst();
                        newChildren.RemoveFirst();
                        continue;
                    }

                    // STEP 4:
                    // if both null, patch and go next.
                    // old : N x y...    =>     x y...
                    // new : N X Y...  (Patch)  X Y...
                    if(newKey == null && oldKey == null)
                    {
                        Patch(context, nodeId, oldChild.NodeId, oldChild, newChild, commands);
                        oldChildren.RemoveFirst();
                        newChildren.RemoveFirst();
                        continue;
                    }
                    // STEP 5:
                    // If newKey is null, find similar node from old, if exists, patch with that, else create new node.
                    // old : x y N...  =>   N x y...    =>     x y...
                    // new : N X Y...       N X Y...  (Patch)  X Y...
                    if(newKey == null && oldKey != null)
                    {
                        var type = newChild.GetType();
                        var patchTarget = oldChildren.FirstOrDefault(x => x.GetKey() == null && x.GetType() == type);
                        if(patchTarget != null)
                        {
                            commands.Add(new InsertBefore(nodeId, patchTarget.NodeId, oldChild.NodeId));
                            Patch(context, nodeId, patchTarget.NodeId, patchTarget, newChild, commands);
                            oldChildren.Remove(patchTarget);
                            newChildren.RemoveFirst();
                            continue;
                        } else
                        {
                            CreateNode(context, newChild, commands);
                            newChildren.RemoveFirst();
                            continue;
                        }
                    }
                    // STEP 6:
                    // if both are same, Just patch and proceed.
                    // old : A y z...  => 
                    // new : A Y Z... (Patch)
                    if(oldKey != null && oldKey.Equals(newKey))
                    {
                        Patch(context, nodeId, oldChild.NodeId, oldChild, newChild, commands);
                        oldChildren.RemoveFirst();
                        newChildren.RemoveFirst();
                        continue;
                    }
                    // STEP 7:
                    // If old keys contains current newKey, insert it into current head and patch.
                    //      ---------  (A will be skipped from next time.)
                    //      ↓       ↑
                    // old : x y... A    =>    A x y...    =>     x y...
                    // new : A X Y...  (Sort)  A X Y...  (Patch)  X Y...
                    if(keyed.TryGetValue(newKey!, out var oldChildWithSameKey))
                    {
                        oldChildren.Remove(oldChildWithSameKey);
                        commands.Add(new InsertBefore(nodeId, oldChildWithSameKey.NodeId, oldChild.NodeId));
                        Patch(context, nodeId, oldChildWithSameKey.NodeId, oldChildWithSameKey, newChild, commands);
                        newChildren.RemoveFirst();
                        continue;
                    }
                    // STEP 8:
                    // if old keys don't contians 'X' and old one is null, create X node and proceed.
                    // old : ...    =>            ... 
                    // new : X Y...   (Create X)  Y...
                    Patch(context, nodeId, oldChild.NodeId, null, newChild, commands);
                    newChildren.RemoveFirst();
                }

                // STEP 9:
                // If oldchildren still left, remove them.
                while(oldChildren.Any())
                {
                    var oldChild = oldChildren.First!.Value;
                    commands.Add(new RemoveChild(nodeId, oldChild.NodeId));
                    oldChild.Dispose();
                    oldChildren.RemoveFirst();
                }
            }

        }
    }
}
