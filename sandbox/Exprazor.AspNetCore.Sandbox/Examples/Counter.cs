namespace Exprazor.AspNetCore.Sandbox.Examples
{
    public record CounterProps(int InitialValue);
    public record CounterState(int Value);

    public class Counter : Component<CounterProps, CounterState>
    {
        protected override CounterState PropsChanged(CounterProps props, CounterState? state) => new CounterState(props.InitialValue);

        protected override IExprazorNode Render(CounterState state)
        {
            return
            Elm("div", new() { ["id"] = "counter" },
                Elm("div", null,
                    Text("Counter")
                ),
                Text(state.Value.ToString()),
                Elm("button", new() { ["onclick"] = () => SetState(state with { Value = state.Value + 1 }) },
                    Text("+")
                )
            );
        }
    }
}
