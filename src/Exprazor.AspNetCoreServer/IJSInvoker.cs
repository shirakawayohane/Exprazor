using MessagePipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exprazor.AspNetCoreServer
{
    public record struct InvokeVoidCommand(string FunctionName, object?[]? Args);
    //public record InvokeCommand(string FunctionName, object?[]? Args);
    public interface IJSInvoker
    {
        event Action<InvokeVoidCommand>? OnInvokeVoid;
        //event Action<InvokeCommand>? OnInvoke;
        /// <summary>
        /// Fire and forget.
        /// </summary>
        void InvokeVoid(string functionName);
    }

    public class RemoteJsInvoker : IJSInvoker
    {
        public event Action<InvokeVoidCommand>? OnInvokeVoid;
        //public event Action<InvokeCommand>? OnInvoke;

        public void InvokeVoid(string functionName)
        {
            OnInvokeVoid?.Invoke(new InvokeVoidCommand(functionName, null));
        }

        public void InvokeVoid(string functionName, object?[]? args)
        {
            OnInvokeVoid?.Invoke(new InvokeVoidCommand(functionName, args));
        }
    }
}
