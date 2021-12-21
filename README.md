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
    // Exprazorのコンポーネントは、プロパティとステートの型引数をとるComponent<TProps,TState>を継承してつくります。
    public record CounterProps(int InitialValue);
    public record CounterState(int Value);

    public class Counter : Component<CounterProps, CounterState>
    {
        // 必須な関数は、`PropsChanged`と`Render`の２つです。
        // IntelliSenseが効く環境であれば、すぐに生成できるでしょう。

        // PropsChangedは、プロパティが初めて渡された時と、変更が検知された時によばれます。
        // プロパティに応じて、ステートを初期化する責務をもち、この中で任意の副作用を起こすことも出来ます。
        protected override CounterState PropsChanged(CounterProps props, CounterState? state) => new CounterState(props.InitialValue);

        // 木構造で出力するDOMを定義します。
        // 使う関数は `Elm`, `Text`の２種類です。
        protected override IExprazorNode Render(CounterState state)
        {
            return
            // Elmは第一引数にタグ名
            // 第二引数に Attributre を辞書型としてとります
            // 第三引数以降に子要素をとります。
            Elm("div", new() { ["id"] = "counter" },
                Elm("div", null,
                    Text("Counter")
                ),
                Text(state.Value.ToString()),
                // AttributeのValueに、関数を入れることにより、内部的に関数としてKeyと名前が一致するコールバックに登録されます
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
