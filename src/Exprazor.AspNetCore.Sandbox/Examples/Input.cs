namespace Exprazor.AspNetCore.Sandbox.Examples
{
    public record InputState(string InputValue);
    public class Input : Component<Unit, InputState>
    {
        protected override InputState PropsChanged(Unit props, InputState? state) => new InputState("");

        protected override IExprazorNode Render(InputState state)
        {
            return
            Elm("div", null,
                Elm("div", null,
                    Elm("input", new() { ["oninput"] = (string s) => { SetState(new InputState(s)); } })
                ),
                Elm("p", null, Text($"Current input value is {state.InputValue}"))
            );
        }
    }
}
