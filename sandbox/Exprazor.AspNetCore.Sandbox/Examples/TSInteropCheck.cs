using Exprazor.AspNetCoreServer;
//using Exprazor.Web.TSInterop;

namespace Exprazor.AspNetCore.Sandbox.Examples
{
    //[TSInterop]
    public partial class TSInteropCheck : Component<Unit, Unit>
    {
        IJSInvoker jsInvoker { get; }
        public TSInteropCheck()
        {
            jsInvoker = Require<IJSInvoker>();
        }
        protected override Unit PropsChanged(Unit props, Unit? state) => Unit;
        protected override IExprazorNode Render(Unit state)
        {
            return Elm("div", null,
                Text("If JS interop is working, there should be some logs in console.")
            );
        }
        protected override ValueTask AfterRenderAsync()
        {
            //TSTSInteropCheck();
            jsInvoker.InvokeVoid("greet");

            return ValueTask.CompletedTask;
        }
    }
}