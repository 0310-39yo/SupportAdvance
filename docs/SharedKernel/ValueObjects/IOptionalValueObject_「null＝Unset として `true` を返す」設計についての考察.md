## 最終決定

**"null は Unset として正常扱いする" というルールを XML コメントで明示し、呼び出し側の期待とズレないようにする**

**必須／任意の区別は、ValueObject の外側（Entity・集約・アプリケーション層）で表現する**



# 「null＝Unset として `true` を返す」設計についての考察

結論から言うと、**変ではありませんが、ドメインの意味論と .NET の慣例のどちらを優先するかで判断が分かれる**設計です。以下、論点を整理します。

------

## 1. .NET の `TryParse` 慣例との相違

標準的な `TryParse` 系メソッド（`int.TryParse`, `DateTime.TryParse` など）は、**null 入力に対して `false` を返す**のが一般的です。

int.TryParse(null, out var _); // false

そのため、`TryFrom` という命名だけを見た利用者は「null なら false が返ってくる」と暗黙に期待する可能性があります。

------

## 2. ただし、ドメイン設計としては妥当

DDD の ValueObject では、**「未設定」を第一級の状態として明示的に表現する**のは Null Object パターンに近く、むしろ推奨されるアプローチです。

- `null`（参照が無い）＝ 「入力そのものが提供されていない」
- 無効文字列 ＝ 「入力は提供されたが値として不正」

この 2 つは**意味論的に別物**なので、区別して扱うのは筋が通っています。特に SampleName が **任意項目（Optional）** の場合、null を Unset として吸収するのは自然です。

------

## 3. 「変」に感じる原因

利用側で以下のような区別ができない点が引っかかります。



if (SampleName.TryFrom(input, out var name))

{

  // ここに来た時、name は「有効な値」か「Unset」か区別できない

}



つまり、**成功パス内でさらに `name.IsUnset` を判定しないと意味のある処理ができない**ため、`bool` の戻り値だけでは情報が不足しています。

------

## 4. 推奨する整理

用途に応じて設計を分けるとブレません。

### パターン A: SampleName が「任意項目」の場合（現行設計を維持）

XMLコメントに以下のように**意図を明示**することで、利用者の誤解を防げます。



/// <returns>

/// null 入力は業務上「未設定」を意味するため <c>true</c> を返す。

/// 「未設定」と「有効値」を区別したい場合は <see cref="SampleName.IsUnset"/> を参照。

/// </returns>



### パターン B: SampleName が「必須項目」の場合

null を `false` として扱う方が、`TryParse` 慣例と整合し、呼び出し側もシンプルになります。



if (input is null)

{

  result = Unset();

  return false; // 必須なのに未指定 → 変換失敗

}



### パターン C: より厳密に表現したい場合

戻り値を 3 値化する（`enum ConversionResult { Success, Unset, Invalid }`）ことで、意味の取りこぼしが無くなります。

public static ConversionResult TryFrom(string? input, out SampleName result)

------

## まとめ

| 観点                     | 判定                           |
| ------------------------ | ------------------------------ |
| .NET の慣例との整合性    | △（やや外れる）                |
| DDD / ValueObject の思想 | ◎（Null Object として正当）    |
| 呼び出し側の使いやすさ   | △（成功時に Unset 判定が必要） |
| ドキュメント明記の必要性 | ◎（必須）                      |

**変ではないが、"null は Unset として正常扱いする" というルールを XML コメントで明示し、呼び出し側の期待とズレないようにする**のがベストプラクティスです。SampleName が任意項目なのか必須項目なのかで、パターン A / B を選ぶのが良いと思います。

# ValueObject に「必須フラグ」を持たせる設計についての考察

結論から言うと、**推奨しません**。ValueObject の設計原則から外れ、複雑さを招く可能性が高いです。以下、理由と代替案を整理します。

------

## 1. なぜ推奨しないのか

### 1-1. ValueObject の不変条件（Invariant）が揺らぐ

ValueObject は「同じ属性を持てば同一」という**等価性**が根幹です。必須フラグを内部に持たせると、以下の問題が発生します。



var name1 = SampleName.From("山田", isRequired: true);

var name2 = SampleName.From("山田", isRequired: false);

// name1 == name2 は true か false か？



- **`true` にする** → 等価性判定でフラグを無視することになり、フラグの存在意義が薄れる
- **`false` にする** → 「同じ"山田"」なのに別物扱いされ、ValueObject の本質に反する

どちらを選んでも設計上の矛盾が残ります。

### 1-2. 必須性は「ValueObject 自身」ではなく「使う文脈」の関心事

`SampleName` が必須かどうかは、**それを保持する Entity や集約側の業務ルール**で決まります。

- 顧客登録では必須
- 検索条件では任意
- インポート処理では必須だが空文字許容

同じ `SampleName` 型でも、使う場所によって必須性が変わるため、**型自身にフラグを持たせるとコンテキストが固定化**されてしまいます。

### 1-3. 責務が肥大化する（SRP 違反）

現状の `SampleName` の責務は「名前としての妥当性を保証する」ことに集約されています。ここに必須性の判定を加えると、以下の責務が混在します。

- 値の正規化 (Normalize)
- 値の検証 (Validate)
- **未設定の許容判定（新規追加）** ← 業務ルール寄り

これは SOLID の単一責任原則（SRP）から外れます。加藤さんが以前から重視されている設計原則ですね。

------

## 2. 代替案：必須性は「利用側」で表現する

### 案 A: Entity / 集約側でルールを持つ（推奨）



public class Customer

{

  public SampleName Name { get; }

  public Customer(SampleName name)

  {

​    // 必須性はここで表現する

​    if (name.IsUnset)

​      throw new DomainException("顧客名は必須です");

​    

​    Name = name;

  }

}



**メリット**

- `SampleName` はどんな文脈でも再利用可能
- 業務ルールが Entity に集約され、意図が明確
- 別の Entity では任意項目として扱える

### 案 B: 型で表現する（強く保証したい場合）

必須と任意を型レベルで区別します。



public class Customer

{

  public SampleName Name { get; }    // 必須：Unset を許容しない

  public SampleName? Nickname { get; }  // 任意：null 可

}



`SampleName` 自身は常に「有効な値」を保証し、任意性は **`?` （Nullable Reference Types）** で表現します。C# 8.0 以降の NRT を活用する現代的なやり方です。

### 案 C: 専用型で明示（DDD 的により厳密）



public readonly struct Required<T> where T : class

{

  public T Value { get; }

  public Required(T value) => Value = value ?? throw new ArgumentNullException();

}

public class Customer

{

  public Required<SampleName> Name { get; }

  public SampleName? Nickname { get; }

}



型シグネチャだけで必須／任意が読み取れます。ただし記述量が増えるので、プロジェクト規模に応じて判断します。

------

## 3. 比較まとめ

| 設計                      | ValueObject の純粋性 | 再利用性 | 業務ルールの明確さ | 実装コスト |
| ------------------------- | -------------------- | -------- | ------------------ | ---------- |
| ValueObject に必須フラグ  | ✕                    | ✕        | △                  | 低         |
| **案 A: Entity 側で判定** | ◎                    | ◎        | ◎                  | 低         |
| 案 B: NRT で表現          | ◎                    | ◎        | ○                  | 低         |
| 案 C: `Required<T>` 型    | ◎                    | ◎        | ◎                  | 中         |

------

## 4. 結論

**必須／任意の区別は、ValueObject の外側（Entity・集約・アプリケーション層）で表現するのが DDD の定石**です。ValueObject は「値としての正しさ」だけに集中させることで、以下が実現できます。

- 単一責任原則（SRP）の遵守
- 型の再利用性向上
- 業務ルールの一元管理
- 等価性の意味論を保つ

加藤さんのプロジェクト（`Meikikou.SupportAdvance.Domain` 等）でも、この方針で統一すると層の責務が綺麗に分離できると思います。

------

案 A（Entity 側で判定）と 案 B（NRT で表現）を組み合わせるのが実務的にはバランスが良いのですが、**SampleName を保持する Entity 側で「必須項目として扱う場所」と「任意項目として扱う場所」は両方ありそうでしょうか？** それによって推奨案を絞り込めます。