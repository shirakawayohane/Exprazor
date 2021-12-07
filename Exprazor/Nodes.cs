using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Exprazor
{
    using Attributes = Dictionary<string, object>;
    using Id = System.Int64;
    using static ExprazorCore;

    public interface IExprazorNode : IDisposable
    {
        object? GetKey();
        public Id NodeId { get; set; }
    }

    public class TextNode : IExprazorNode
    {
        internal TextNode(string text)
        {
            Text = text;
            // NodeIdはPatch時にSetされる。
        }
        public string Text { get; init; }
        public Id NodeId { get; set; }

        public void Dispose() {/* Do nothing for now. */}

        public object? GetKey() => Text;
    }

    internal class HTMLNode : IExprazorNode
    {
        public HTMLNode(ExprazorApp context, string tag, Attributes? attributes, IEnumerable<IExprazorNode>? children)
        {
            Context = context;
            Tag = tag;
            Attributes = attributes;
            Children = children;
            // NodeId will be asigned later.
        }
        public ExprazorApp Context { get; }
        public string Tag { get; init; }
        public Attributes? Attributes { get; init; }
        public IEnumerable<IExprazorNode>? Children { get; init; }
        public Id NodeId { get; set; }

        public void Dispose()
        {
            if (Attributes != null) {
                Context.TryRemoveCallbacksOfNode(NodeId);
            }
            if(Children != null)
            {
                foreach(var child in Children)
                {
                    child.Dispose();
                }
            }
        }

        public object GetKey() => Attributes?.TryGetValue("key", out var key) ?? false ? key : NodeId;
    }

    public abstract class Component : IExprazorNode
    {
        // Non-nullability is ensured by Elm function.
        internal ExprazorApp Context { get; set; } = default!;
        public Id ParentId { get; init; }
        public Id NodeId
        {
            get
            {
                return lastTree!.NodeId;
            }
            set
            {
                lastTree!.NodeId = value;
            }
        }
        public virtual object GetKey() => Props;
        internal IExprazorNode? lastTree { get; set; }
        /// <summary>
        /// Patch中にあとから代入される
        /// </summary>
        protected internal object? State { get; set; } = default!;
        /// <summary>
        /// Elm<TComponent>で非nullを担保
        /// </summary>
        protected internal object Props { get; init; } = default!;
        protected internal abstract void Init();
        protected internal abstract object /* State */ PropsChanged(object props);
        protected IExprazorNode Elm(string tag, Attributes? attributes, IEnumerable<IExprazorNode>? children) => new HTMLNode(Context, tag, attributes, children);
        protected IExprazorNode Elm<TComponent>(object props) where TComponent : Component, new()
        {
            var ret = new TComponent
            {
                Context = Context,
                Props = props,
                ParentId = this.ParentId,
            };
            return ret;
        }
        protected IExprazorNode Text(string text) => new TextNode(text);
        protected void SetState(object newState)
        {
            var commands = new List<DOMCommand>();
            var newTree = Render(newState);
            Patch(Context, NodeId, ParentId, lastTree, newTree, commands);
            lastTree = newTree;
            Context.DispatchCommands(commands);
        }

        protected internal abstract IExprazorNode Render(object state);

        public void Dispose()
        {
            if(lastTree != null)
            {
                lastTree.Dispose();
            }
        }
    }
}
