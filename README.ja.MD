 # Exprazor
Exprazorはユーザーインターフェースを構築するためのC#ライブラリです。

同様のライブラリとしては、React(JS), Vue(JS), Yew(Rust), Blazor(C#)などがありますが、中でもBlazorは直接の競合に当たるかもしれませんが。Exprazorでは以下のコンセプトを主眼において開発しています。

1. 式としてDOM構造を扱う
2. WebにおけるシームレスなTypescriptとの相互運用
3. 様々なプラットフォームで動く設計を重視する

まだ影響範囲の狭い個人開発なので、野心的な機能、OpinionatedなAPIを取り入れようという考えが根底にあり、それは他のコンセプトにも反映されています。

## Installation
まだパッケージとして公開していないので、試してみたい方は、
Visual Studio 2022 などの .NET6 動作環境で、リポジトリをクローンし、[Examples](https://github.com/WiZLite/Exprazor/tree/master/src/Exprazor.AspNetCore.Sandbox/Examples)をベースに試してみてください。

## Getting Started (Web)

Exprazorは、Webアプリケーションの基盤として、AspNetCoreを用い、その上で動作します。

まずは、空のAspNet.Coreのプロジェクトを作ります。

エントリポイントのファイルに、Exprazorに必要な依存関係を登録します。
```cs
// Program.cs
builder.Services.AddExprazor();
```

### 初めてのコンポーネント
次に初めてのコンポーネントを作ってみましょう。
コンポーネントは、適当に切り分けられたビューと機能の一単位です。
Exprazorでは、コンポーネントをクラスとして定義します。
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

### ルーティング

Exprazorのルーティングは、Httpリクエストを経由する方式となっています。
ルーティングのマッチは正規表現で行います。

```cs
// Program.cs
app.MapExprazor(router =>
{
    router.Route("/counter/?(\\d+)?", _ => ExprazorApp.Create<Counter>(new CounterProps(0)));
});
```

URLの一部をパラメーターとして用いたい場合は以下のように、
正規表現で得られたグループの文字列の配列を使うことが出来ます。

```cs
// Program.cs
app.MapExprazor(router =>
{
    router.Route("/counter/?(\\d+)?", matches /* : string[] */ =>
    {
        if(matches == null || matches.Length == 0) return ExprazorApp.Create<Counter>(new CounterProps(0));
        return ExprazorApp.Create<Counter>(new CounterProps(int.Parse(matches![0])));
    });

    // このようにフォールバックを定義しておくと良いでしょう。
    router.Route(".*", _ => ExprazorApp.Create<NotFound>(Unit.Instance));
});
```

### ミドルウェアの追加
最後に、忘れずにミドルウェアを追加しておきましょう。
```cs
app.UseExprazor();
```

localhostで起動し、 `/counter/123`  等のパスで開いてみましょう。
以下のようなページが表示されるはずです。
https://discord.com/channels/857257615746400296/857257615746400301/920596766221557820


## Concepts

## Getting Started
### ミドルウェアの登録
Exprazorに必要な依存関係を登録します。

```cs
builder.Services.AddExprazor();
```
### Creating First Component

ではまず最初のコンポーネントを作ってみましょう！

コンポーネントは、ステートとプロパティ２つの型を持ちます
```cs
namespace Examples;

public record CounterProps(int InitialValue);
public record CounterState(int Value);
```
この２つをプロパティ/ステートを型引数として持った
Component<CounterProps, CounterState>を継承してコンポーネントを作成します。
```cs
public class Counter : Component<CounterProps, CounterState>
{
```
コンポーネントに必須な関数は、`PropsChanged`, `Render`のみです。
これらの関数は、IntelliSenseが効く環境であれば、すぐに補完することが出来るはずです。

`PropsChanged`はプロパティが最初に渡されたときと変更されたときに呼ばれ、ステートの初期化や、プロパティに応じたステートの更新を担います。
```cs
    protected override CounterState PropsChanged(CounterProps props, CounterState? state) => new CounterState(props.InitialValue);
```

Renderで、木構造のViewを定義します。

```cs
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
```

## コンセプト

### 1. 式としてDOM構造を扱う
BlazorやVueでは、「テンプレート」としてDOM構造を表現するのに対して、Exprazorでは、DOM構造を式として扱うことを重視しています。これには以下のメリットがあります。

- 特殊な構文やプリプロセッサを導入する必要が無い
  - より多くのエディタで動く。
  (今日のBlazorの開発は、安定性を増してきて、ホットリロードも伴った快適なものではありますが、その過程には、多くの、Anylyerのバグが発生しました。)
  - 早期リターンや小さなView関数の使いまわしなど、表現の自由度が上がり生産性が高まる
  - コードの一覧性が高まる

### 2. Web開発におけるシームレスなTypescriptとの相互運用
Exprazorは、Web以外のプラットフォームでも使えるようにすることを目標にしていますが、設計を損なわない限り、各プラットフォーム毎に強みとなる機能を実装します。
現在開発段階ですが、以下のようにTypescriptの関数をシームレスに呼ぶ為の機能を [Incremental Source Generator](https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/)によってバインディングを自動生成することにより実装する予定です。

```ts
// Split.ts
function split(a : HTMLElement, b : HTMLElement) {
    // Impl
}
```

```cs
// Split.cs

ElementReference a;
ElementReference b;

protected override void OnAfterRender() {
    split(a, b);
}
```

### 3. 様々なプラットフォームで動く設計を重視します
Exprazorは、[コアとなる部分](https://github.com/WiZLite/Exprazor/tree/master/src/Exprazor)はWebで以外で使う妨げにならないよう、疎結合な設計を心がけています。

具体的には、以下のインターフェースを一つ提供することにより成り立っています。

まず、[DOMCommand](https://github.com/WiZLite/Exprazor/blob/master/src/Exprazor/DOMCommands.cs) なるコマンド群があります。

コールバック実行等の入力と、それによって発行されたコマンド群のハンドラをpublicに提供することにより、以下の要件を満たせば、理論上どのようなプラットフォームでも動く設計になっています。

- WebベースのDOMCommandコマンド群に対応したUIの操作を持っている
- 何らかの方法でExprazorと通信することが出来る(WebSocket, Wasm, アセンブリ参照等)

現在未実装ですが、Unityのクライアントに対応する予定があります。
詳細は検討中ですが、DOMCommandのオペレーションの単位に最も近いオペレーションを持つAPI、第一候補として、UIElements、その他にもuGUIを元にした対応を考えています。