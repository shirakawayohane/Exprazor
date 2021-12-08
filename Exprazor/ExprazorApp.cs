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
    using Attributes = Dictionary<string, object>;
    using Id = System.Int64;
    public class ExprazorApp
    {
        public const Id MOUNT_ID = -1;
        Id _id = MOUNT_ID;
        internal Id NextId() => ++_id;

        internal List<DOMCommand> commands { get; } = new(256);
        public event Action<List<DOMCommand>>? CommandHandler;
        Component rootComponent = default!;
        Dictionary<Id, Dictionary<string, object>> callbacks = new();

        private ExprazorApp() {}

        public static ExprazorApp Create<TComponent>(object props) where TComponent : Component, new()
        {
            var ret = new ExprazorApp();
            ret.rootComponent = new TComponent
            {
                ParentId = MOUNT_ID,
                NodeId = ret.NextId(),
                Props = props
            };
            return ret;
        }

        public void Start()
        {
            rootComponent.SetState(rootComponent.State!);
        }

        internal void DispatchCommands()
        {
            CommandHandler?.Invoke(commands);
            commands.Clear();
        }

        internal void AddCallback(Id nodeId, string key, object callback)
        {
            if(callbacks.TryGetValue(nodeId, out var callbacksOfNode))
            {
                callbacksOfNode.Add(key, callback);
            } else
            {
                callbacks.Add(nodeId, new Dictionary<string, object>()
                {
                    [key] = callback
                });
            }
        }

        internal void RemoveCallback(Id nodeId, string key)
        {
            callbacks[nodeId].Remove(key);
        }

        internal void TryRemoveCallbacksOfNode(Id nodeId)
        {
            if(callbacks.ContainsKey(nodeId))
            {
                callbacks.Remove(nodeId);
            }
        }

        public void InvokeVoidCallback(Id nodeId, string key)
        {
            (callbacks[nodeId][key] as Action)!.Invoke();
        }
    }
}
