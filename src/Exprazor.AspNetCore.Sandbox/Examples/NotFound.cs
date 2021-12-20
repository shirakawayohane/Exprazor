namespace Exprazor.AspNetCore.Sandbox.Examples
{


    class NotFound : Component<Unit, Unit>
    {
        protected override Unit PropsChanged(Unit props, Unit? state) => Unit;

        protected override IExprazorNode Render(Unit state)
        {
            return 
                Elm("div", null, 
                    Text("Not Found")
                );
        }
    }
}
