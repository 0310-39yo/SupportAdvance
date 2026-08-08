# ValueObject 設計ガイド修正プラン

**版:** 2.0  
**日付:** 2026-08-08  
**対象ファイル：** 
- `docs/SharedKernel/ValueObjects/ValueObject_設計ガイド.md` → `docs/Assistance/Guides/ValueObject_設計ガイド.md`

---

## 🎯 修正の背景

実装と設計ドキュメントの矛盾を3つのパターンで統一化

---

## 📅 実施順序（詳細版）

### Step 1: ファイル移動とリンク修正

**移動:**
```
docs/SharedKernel/ValueObjects/ValueObject_設計ガイド.md 
  → docs/Assistance/Guides/ValueObject_設計ガイド.md
```

**相対リンク修正（参考資料セクション、末尾）:**

| リンク | 修正前 | 修正後 |
|---|---|---|
| ValueObject 技術仕様書 | `../Abstractions/ValueObject_技術仕様書.md` | `../../SharedKernel/ValueObjects/Abstractions/ValueObject_技術仕様書.md` |
| PrimitiveValueObject 技術仕様書 | `../Abstractions/PrimitiveValueObject_技術仕様書.md` | `../../SharedKernel/ValueObjects/Abstractions/PrimitiveValueObject_技術仕様書.md` |
| EnumValueObject 技術仕様書 | `../Abstractions/EnumValueObject_技術仕様書.md` | `../../SharedKernel/ValueObjects/Abstractions/EnumValueObject_技術仕様書.md` |
| ValueObjectComponentNormalizer | `../Abstractions/ValueObjectComponentNormalizer_技術仕様書.md` | `../../SharedKernel/ValueObjects/Abstractions/ValueObjectComponentNormalizer_技術仕様書.md` |

**メモリファイル修正（MEMORY.md）:**
- 新参照を追加：`[ValueObject_設計ガイド.md](../../../docs/Assistance/Guides/ValueObject_設計ガイド.md)`

---

### Step 2: セクション番号の修正（重複解消）

**問題:** セクション 10 が2つ存在

**修正:**
- 「ValueObject パターン別ガイド」を セクション 11 に変更
- サブセクション 10.1～10.6 を 11.1～11.6 に変更
- 「レイヤ制約」を セクション 12 に変更
- 目次を全て更新

---

### Step 3: セクション 4 を3分割（4.1/4.2/4.3）

#### 新セクション 4.1：PrimitiveValueObject + IsSet=true/false 両方（オプション項目）

**対象パターン:** UpdatedAt（更新日時）、RespondentName（回答者名）

**セクション構造:**

```markdown
### 4.1 【PrimitiveValueObject 向け】IsSet フラグによる未設定状態管理（オプション項目）

**概要:**
- IsSet フラグで "値あり" / "未設定" を区別
- オプション項目で Unset 状態が自然（例：フォーム未入力）
- Unset() メソッドで未設定インスタンスを生成

**IsSet = true — 値を保持している状態**
```csharp
var name = RespondentName.From("山田太郎");
Assert.IsTrue(name.IsSet);
Assert.AreEqual("山田太郎", name.Value);
```

**IsSet = false — 未設定状態（値を保持していない）**
```csharp
var unset = RespondentName.Unset();
Assert.IsFalse(unset.IsSet);
Assert.IsNull(unset.Value);  // 参照型 → null、値型 → default(T)
Assert.IsFalse(unset.TryGetValue(out _));  // 取得失敗
```

**【重要】Value プロパティの動作**
- IsSet=true：実際の値を返す
- IsSet=false：その型のデフォルト値
  - string/DateTime? → null
  - int → 0
  - DateTime → LocalDateTime.MinValue

**等価性判定:**
```csharp
var unset1 = RespondentName.Unset();
var unset2 = RespondentName.Unset();
var value = RespondentName.From("山田太郎");

Assert.AreEqual(unset1, unset2);     // true（両方 Unset）
Assert.AreNotEqual(unset1, value);   // true（状態が異なる）
```
```

#### 新セクション 4.2：PrimitiveValueObject + IsSet=true のみ（入力必須）

**対象パターン:** CreatedAt（作成日時）、DeletedAt（削除日時）

**実装例：CreatedAt**
```csharp
public sealed class CreatedAt : PrimitiveValueObject<DateTime>, IEquatable<CreatedAt>
{
    private CreatedAt(DateTime value) : base(value, true)  // isSet は常に true
    {
    }

    public static CreatedAt From(LocalDateTime value) => new(value.Value);
    public static CreatedAt From(DateTime value) => new(value);
    
    // 【注意】Unset() メソッドなし ← 入力必須を表現

    public DateTime Value => ValueField;

    public override void Validate(DateTime normalized)
    {
        if (normalized == LocalDateTime.MinValue || normalized == LocalDateTime.MaxValue)
            throw new ArgumentException("有効な日時ではありません。");
    }

    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return ValueField;  // IsSet=true のみなので、常に yield
    }
}
```

**CreatedAt と UpdatedAt の比較:**

| 項目 | CreatedAt | UpdatedAt |
|---|---|---|
| 基底クラス | PrimitiveValueObject<DateTime> | PrimitiveValueObject<DateTime?> |
| TValue | DateTime | DateTime? |
| IsSet | 常に true | true/false |
| Unset() メソッド | ❌ 無 | ✅ 有 |
| 役割 | 作成日時（不変） | 更新日時（変更可能） |
| ビジネス意味 | 作成時に必ず設定 | 初回作成後は null、初回更新で DateTime に |
| 永続化時 | 値をそのまま渡す | IsSet 確認後に値を渡す |

#### 新セクション 4.3：ValueObject 直接継承（RowId パターン）

**対象パターン:** RowId（DB物理キー）

**RowId のライフサイクル:**

| フェーズ | value | 永続化時の処理 | 用途 |
|---|---|---|---|
| Entity 新規作成 | 0 | INSERT: DB へ値を渡さない | 未採番状態 |
| DB 採番後 | >0 | — | 採番済み |
| UPDATE/DELETE | >0 | WHERE rowid = @rowid | WHERE 条件値として使用 |
| 以降永続化 | >0（不変） | WHERE rowid = @rowid | RowId は変わらない |

**【重要】value=0 と value>0 の意味の違い**
- value=0：未採番（INSERT 時、DB が採番）
- value>0：採番済み（UPDATE/DELETE で WHERE 条件に使用）
- 一度採番されると不変（Entity の更新時に RowId は変わらない）

**実装例：RowId**
```csharp
public sealed class RowId : ValueObject, IEquatable<RowId>
{
    private readonly long _value;

    private RowId(long value)
    {
        if (value < 0)
            throw new ArgumentException("RowId must be non-negative.", nameof(value));
        _value = value;
        IsSet = true;  // 常に true
    }

    /// <summary>未採番状態のRowIdを生成（value=0）</summary>
    public static RowId New() => new(0);

    /// <summary>DB から読み込まれた値から RowId を生成</summary>
    public static RowId From(long value) => new(value);

    public long Value => _value;

    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return _value;
    }
}
```

**永続化側での処理分岐（リポジトリ層）:**
```csharp
public async Task SaveAsync(Entity entity)
{
    if (entity.RowId.Value == 0)
    {
        // INSERT フロー：RowId=0 なので、DB へ値を渡さない
        var generatedId = await _database.InsertAsync(
            tableName: "t_Entity",
            columns: new[] { "column1", "column2", ... },
            values: new[] { val1, val2, ... }
            // rowid は含めない ← DB が自動採番
        );
        
        // 採番後に Entity を再構成
        entity.SetRowId(RowId.From(generatedId));
    }
    else
    {
        // UPDATE フロー：RowId>0 なので、WHERE 条件に使用
        await _database.UpdateAsync(
            tableName: "t_Entity",
            setClause: "column1 = @val1, column2 = @val2, ...",
            whereClause: "rowid = @rowid",
            parameters: new { val1, val2, ..., rowid = entity.RowId.Value }
        );
    }
}
```

---

### Step 4: セクション 4.4「状態管理パターン別比較表」を拡張

| 特性 | PrimitiveValueObject<T>（IsSet両方） | PrimitiveValueObject<T>（IsSetTrue） | RowId（ValueObject） |
|---|---|---|---|
| **例** | UpdatedAt, RespondentName | CreatedAt, DeletedAt | RowId |
| **基底クラス** | PrimitiveValueObject<TValue> | PrimitiveValueObject<TValue> | ValueObject |
| **IsSet フラグ** | ✅ 有 | ✅ 有 | ❌ 無 |
| **Unset 状態** | ✅ IsSet=false | ❌ 常に IsSet=true | ❌ value=0で未採番 |
| **Unset() メソッド** | ✅ 有 | ❌ 無 | ❌ 無 |
| **用途** | ビジネス属性（オプション） | 監査情報（必須） | DB物理キー |
| **永続化時** | IsSet確認 → 値を渡す | 値をそのまま渡す | value=0→採番フロー、value>0→WHERE条件 |

---

### Step 5: セクション 11.1「PrimitiveValueObject を使うべき場合」を修正

【重要】Step 2 で「ValueObject パターン別ガイド」がセクション 11 に変更されたため、旧 10.1 → 新 11.1

**修正対象セクション：** 現在のセクション 10.1（Step 2 後は 11.1）

**判定基準（2つのケース）:**

【ケース A】Unset 状態が自然（オプション項目）
- ✅ 例：UpdatedAt, RespondentName, RespondentAge
- Unset() メソッド実装あり
- IsSet = true/false 両方が存在

【ケース B】Unset 状態が不自然（入力必須）
- ✅ 例：CreatedAt, DeletedAt
- Unset() メソッド実装なし
- IsSet = true のみ（フラグはあるが常に true）

---

### Step 6: セクション 11.3「RowId を使うべき場合」を拡張

【重要】Step 2 で「ValueObject パターン別ガイド」がセクション 11 に変更されたため、旧 10.3 → 新 11.3

**修正対象セクション：** 現在のセクション 10.3（Step 2 後は 11.3）

**RowId のライフサイクル（完全版）:**

【Phase 1】Entity 新規作成
- `RowId.New()` → value=0（未採番）

【Phase 2】初回永続化（INSERT）
- 値を DB へ渡さない → DB が Sequence で採番

【Phase 3】Entity 再構成（Mapper）
- DB の値を使用して `RowId.From(dbModel.RowId)` で再構成

【Phase 4】以降の永続化（UPDATE/DELETE）
- `WHERE rowid = @rowid` の条件値として使用
- RowId は変わらない（不変）

---

### Step 7: セクション 11.4「パターン選択フローチャート」を3分岐に修正

【重要】Step 2 で「ValueObject パターン別ガイド」がセクション 11 に変更されたため、旧 10.4 → 新 11.4

**修正対象セクション：** 現在のセクション 10.4（Step 2 後は 11.4）

```
【ValueObject を設計する】
          │
          ├─ 集約（Aggregate）を識別する？
          │   YES → AggregateId 継承
          │   
          ├─ DB テーブル行の物理キー？
          │   YES → RowId パターン（ValueObject 直接継承）
          │   
          └─ スカラ値（単一フィールド）かつビジネス属性？
              YES → PrimitiveValueObject 継承
                  ├─ Unset 状態が必要か？
                  │   YES → IsSet=true/false 両方
                  │   NO → IsSet=true のみ
```

---

### Step 8: セクション 7.3 に詳細な比較表を追加

**CreatedAt と UpdatedAt の詳細比較**

状態の遷移:
- CreatedAt：作成時に DateTime → 以降変わらない
- UpdatedAt：作成時は null → 初回更新で DateTime → 更新のたびに変わる

---

### Step 9: セクション 10.4「実行順序」に図解を追加

【重要】セクション 10「検証・正規化の責務分離」はセクション番号が変わらない（セクション 11 以降が変わるだけ）

**修正対象セクション：** セクション 10.4（「検証・正規化の責務分離」の下位セクション）

**【PrimitiveValueObject のコンストラクタでの処理フロー】**

```
isSet = true か？
    ├─ YES → Normalize → Validate → ValueField 格納 → IsSet=true
    └─ NO → ValueField = default! → IsSet=false
```

【重要】IsSet=false の場合
- ValueField は「その型のデフォルト値」で初期化される

---

### Step 10: 関連ドキュメント確認・修正

**確認対象：**
- `docs/Assistance/Guides/Entity_設計ガイドライン.md` — RowId の使用例
- `docs/Assistance/Guides/Mapper_パターンガイド.md` — RowId の Mapper での処理
- `docs/Assistance/Guides/Repository_パターンガイド.md` — RowId の永続化時処理

---

### Step 11: 全体確認・バージョン更新

【重要】セクション番号の変更を確認

**セクション番号の変更一覧:**
```
【変更前】                          【変更後】
Section 10: 検証・正規化           →  Section 10: 検証・正規化（変わらず）
Section 10.4: 実行順序             →  Section 10.4: 実行順序（変わらず）

Section 10: ValueObject パターン別 →  Section 11: ValueObject パターン別
Section 10.1: PrimitiveValueObject →  Section 11.1: PrimitiveValueObject
Section 10.2: AggregateId          →  Section 11.2: AggregateId
Section 10.3: RowId                →  Section 11.3: RowId
Section 10.4: フローチャート       →  Section 11.4: フローチャート
Section 10.5: 比較表               →  Section 11.5: 比較表
Section 10.6: チェックリスト       →  Section 11.6: チェックリスト

Section 11: レイヤ制約             →  Section 12: レイヤ制約
```

**最終確認項目：**
- [ ] ファイルが `docs/Assistance/Guides/` に移動されている
- [ ] セクション 4 が3分割（4.1/4.2/4.3）されている
- [ ] セクション 10 の重複が解消されている（10.1～10.6 が 11.1～11.6 に変更）
- [ ] セクション 11「レイヤ制約」が セクション 12 に変更されている
- [ ] 目次が全て最新に更新されている
- [ ] 3パターンが明確に区別されている
- [ ] RowId のライフサイクル（value=0 → value>0 → 不変）が説明されている
- [ ] 永続化時の処理分岐が説明されている
- [ ] 実装例が修正内容を反映している
- [ ] メモリファイルが更新されている
- [ ] バージョンが更新されている

**バージョン更新:**
```markdown
| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-07 | Claude Code | 初版作成 — 現在の実装に基づくガイド |
| 2.0 | 2026-08-08 | Claude Code | セクション 4 を3分割、セクション 10 の重複を解消（旧10.1～10.6 → 新11.1～11.6、旧11「レイヤ制約」→ 新12）、RowId のライフサイクル詳細化、実装例を修正内容に反映 |
```

---

## ✅ 修正完了の判定基準

【Phase 1】ファイル移動・リンク修正
1. ✅ ファイルが `docs/SharedKernel/ValueObjects/ValueObject_設計ガイド.md` から `docs/Assistance/Guides/ValueObject_設計ガイド.md` に移動
2. ✅ 参考資料セクションのリンク（4つ）が修正されている
3. ✅ メモリファイル MEMORY.md が更新されている

【Phase 2】セクション番号修正
4. ✅ セクション 10「ValueObject パターン別ガイド」が セクション 11 に変更
5. ✅ サブセクション 10.1～10.6 が 11.1～11.6 に変更
6. ✅ セクション 11「レイヤ制約」が セクション 12 に変更
7. ✅ 目次が全て最新に更新されている

【Phase 3～8】内容修正
8. ✅ セクション 4 が3分割（4.1/4.2/4.3）されている
9. ✅ セクション 4.4 の表が3パターンに拡張されている
10. ✅ セクション 11.1（旧10.1）が「Unset有/無」で2ケースに分類されている
11. ✅ セクション 11.3（旧10.3）が RowId のライフサイクルで拡張
12. ✅ セクション 11.4（旧10.4）が3分岐フローチャートに修正
13. ✅ セクション 7.3 に CreatedAt/UpdatedAt 比較表が追加
14. ✅ セクション 10.4「実行順序」に図解が追加

【Phase 9～11】総仕上げ
15. ✅ 関連ドキュメントが確認・修正されている
16. ✅ 3パターンが明確に区別されている
17. ✅ RowId のライフサイクル（value=0 → value>0 → 不変）が詳細化されている
18. ✅ 永続化時の処理分岐（INSERT/UPDATE/DELETE）が説明されている
19. ✅ 実装例が修正内容を反映している
20. ✅ バージョンが v2.0 に更新されている
