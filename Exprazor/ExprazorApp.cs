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
        Id _id = -1;
        internal Id NextId() => ++_id;

        Action<List<DOMCommand>>? commandHandler;
        Component rootComponent;

        public ExprazorApp(Component rootComponent)
        {
            rootComponent.Context = this;
            this.rootComponent = rootComponent; ;
        }

        public void SetCommandHandler(Action<List<DOMCommand>> commandHandler)
        {
            this.commandHandler = commandHandler;
        }

        public void Initialize(Action<List<DOMCommand>> commandHandler)
        {
            this.commandHandler = commandHandler;
            var commands = new List<DOMCommand>();
            ExprazorCore.CreateNode(this, rootComponent, commands);
            DispatchCommands(commands);
        }

        internal void DispatchCommands(List<DOMCommand> commands)
        {
            if(commandHandler == null) throw new InvalidOperationException("Please set one or more handler");
            commandHandler.Invoke(commands);
        }
    }
}
