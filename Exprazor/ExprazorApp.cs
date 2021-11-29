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

        Action<List<DOMCommand>> _commandsHandler;
        Component _rootComponent;

        public ExprazorApp(Component rootComponent, Action<IEnumerable<DOMCommand>> commandHandler)
        {
            this._commandsHandler = commandHandler;
            this._rootComponent = rootComponent;
        }

        public void DispatchCommands(List<DOMCommand> commands)
        {
            _commandsHandler.Invoke(commands);
        }
    }
}
