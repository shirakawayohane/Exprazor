namespace Exprazor.AspNetCore.Sandbox.Examples
{
    public class DICheck : Component<Unit, Unit>
    {
        protected override Unit PropsChanged(Unit props, Unit? state) => Unit;

        protected override IExprazorNode Render(Unit state)
        {
            return Elm("div", null, 
                Text(Require<IGetMessageService>().GetMessage())
            );
        }
    }

    public interface IGetMessageService
    {
        string GetMessage();
    }

    public class GetMessageService : IGetMessageService
    {
        public string GetMessage() => "Hello! I come from DI Container.";
    }
}
