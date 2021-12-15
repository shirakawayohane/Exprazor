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
    using Id = System.Int32;
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
        }

        public static implicit operator TextNode(string str)
        {
            return new TextNode(str);
        }

        public string Text { get; init; }

        public Id NodeId { get; set; }

        public void Dispose() {/* Do nothing. */}
        public object? GetKey() => null;
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

        public object? GetKey() => (Attributes?.TryGetValue("key", out var key) ?? false) ? key : null;
    }

    public abstract class Component : IExprazorNode
    {
        // Non-nullability is ensured by Elm function.
        internal ExprazorApp Context { get; set; } = default!;
        public Id ParentId { get; init; } = ExprazorApp.MOUNT_ID;
        public Id NodeId { get; set; }
        public virtual object GetKey() => Props;
        internal IExprazorNode? lastTree { get; set; }
        /// <summary>
        /// State will be assigned later in `Patch` function.
        /// </summary>
        internal object? State { get; set; } = default!;
        /// <summary>
        /// Non-nullability is ensured by Elm<TComponent> function.
        /// </summary>
        protected internal object Props { get; set; } = default!;
        protected internal virtual void Init() { }
        protected internal abstract object /* State */ PropsChanged(object props);
        protected IExprazorNode Elm(string tag, Attributes? attributes, IEnumerable<IExprazorNode>? children) => new HTMLNode(Context, tag, attributes, children);
        
        protected IExprazorNode Elm(string tag, Attributes? attributes, params IExprazorNode[] children) => new HTMLNode(Context, tag, attributes, children);
       
        internal IExprazorNode Elm<TComponent>(object props) where TComponent : Component, new()
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
        internal async void SetState(object newState)
        {
            var newTree = Render(newState);
            Patch(Context, ParentId, NodeId, lastTree, newTree, Context.commands);
            lastTree = newTree;
            await Context.DispatchAsync();
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
    public record Unit();
    public abstract class Component<TProps, TState> : Component where TState : class
    {
        protected static Unit Unit = new();

        public Component() {}

        protected abstract TState PropsChanged(TProps props, TState? state);
        protected abstract IExprazorNode Render(TState state);

        protected internal override object PropsChanged(object props)
        {
            if(State != null)
            {
                return PropsChanged((TProps)props, (TState)State)!;
            } else
            {
                return PropsChanged((TProps)props, null);
            }
        }

        protected internal override IExprazorNode Render(object state) => Render((TState)state)!;

        protected IExprazorNode Elm<TComponent>(TProps props) where TComponent : Component<TProps, TState>, new() => base.Elm<TComponent>(props!);

        protected void SetState(TState newState) => base.SetState(newState!);
    }
}
