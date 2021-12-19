 # Exprazor

Exprazorはユーザーインターフェースを構築するためのC#ライブラリです。

同様のライブラリとしては、React(JS), Vue(JS), Yew(Rust), Blazor(C#)などがありますが、中でもBlazorは直接の競合に当たるかもしれませんが。Exprazorでは以下のコンセプトを主眼において開発しています。


1. 式としてDOM構造を扱う
2. WebにおけるシームレスなTypescriptとの相互運用
3. 様々なプラットフォームで動く設計を重視する


まだ影響範囲の狭い個人開発なので、野心的な機能、OpinionatedなAPIを取り入れようという考えが根底にあり、それは他のコンセプトにも反映されています。

## Concepts

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