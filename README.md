# Exprazor
Exprazor is a C# library for buildling user interfaces.

It's like Blazor, Vue, but more like React.
Though there is an [blazor](https://github.com/dotnet/aspnetcore/tree/main/src/Components) as a competing library, Exprazor has different between blazor in these points below.

1. Treat view as an expression.
2. Support seamless typescript interop.
3. Targets to be used beyond the web development.

## Caution
Exprazor is now experimental project.
Not a little breaking change may occur in future.

## Installation
Since exprazor is yet to be published as packages, Please clone this repository and firstly try based on the [Examples](https://github.com/WiZLite/Exprazor/tree/master/src/Exprazor.AspNetCore.Sandbox/Examples).
Make sure have .NET6 compatible environment.

## Getting Started (Web)
For web, exprazor provide a middleware which can be used on ASP.NET Core.

### Create Project
First, create empty ASP.NET Core Project.

### Add Services
Add the services required for Exprazor in the entry point class.
```cs
// Program.cs
builder.Services.AddExprazor();
```

## Create your first component.
Next, let's create your first component.
The component can be thought of as a unit of properly-separated view and functions.
In Exprazor, you can declare a component using a class that inherits `Component<TProps, TState>` class.
```cs
// Counter.cs
namespace Examples
{
    // Exprazor component require its Props and State type.
    public record CounterProps(int InitialValue);
    public record CounterState(int Value);

    public class Counter : Component<CounterProps, CounterState>
    {
        // Only two functions : `PropsChanged` and `Render` are required.
        // You can easily auto complete those function with IntelliSence.

        // `PropsChanged` is called when property was provided for the first time, and when props has changed.
        // `PropsChanged` is responsible for initializing or changing state depending on the props.
        protected override CounterState PropsChanged(CounterProps props, CounterState? state) => new CounterState(props.InitialValue);

        // `Render` is responsible for returning IExprazorNode(Virtual DOM) tree.
        // Basically, you gonna use only three functions : `Elm`, `Text` and `Elm<TComponent>`.
        protected override IExprazorNode Render(CounterState state)
        {
            return
            // Elm function receives 
            // - tag : string
            // - attributes : Dictionary<string, object>
            // - params IEnumerable<IExprazorNode>
            // in order.
            Elm("div", new() { ["id"] = "counter" },
                Elm("div", null,
                    Text("Counter")
                ),
                Text(state.Value.ToString()),
                // Exprazor will automatically bind functions if any function is provided to an Attribute.
                Elm("button", new() { ["onclick"] = () => SetState(state with { Value = state.Value + 1 }) },
                    Text("+")
                )
            );
        }
    }
}
```

### Routing
Routing is Exprazor is via HTTP requests.

[work on progresss...]
### DI Support

## JS/TS Interop
