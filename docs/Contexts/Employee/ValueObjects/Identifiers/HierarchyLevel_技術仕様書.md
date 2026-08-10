# HierarchyLevel — 技術仕様書

**対象:** HierarchyLevel ValueObject  
**層:** SharedKernel / ValueObjects / Identifiers  
**作成日:** 2026-08-10  
**版:** 1.0

---

## 1. 概要

部署の階層レベルを表す ValueObject。DB スキーマの `m_departments.hierarchy_level` に対応。

### 責務
- 階層レベル（0-4）を型安全に保持
- 値の検証（0-4 の範囲）
- ビジネス意味を名称で表現（Company, Division, Department など）
- 等価性判定とハッシュコード計算

---

## 2. 仕様

### 型定義
```csharp
public sealed class HierarchyLevel : EnumValueObject<int>, IEquatable<HierarchyLevel>
```

### 基底型
- `EnumValueObject<int>`
- IsSet = true（常に設定済み、必須パターン）

### 値と表示名

| 値 | 表示名 | 説明 |
|---|-------|------|
| 0 | Company（会社） | 最上位の部署 |
| 1 | Division（本部） | 会社直下 |
| 2 | Department（部） | 本部直下 |
| 3 | Group（グループ） | 部直下 |
| 4 | Team（チーム） | グループ直下 |

---

## 3. 公開インターフェース

### 工場メソッド

#### From(int value)
```csharp
public static HierarchyLevel From(int value)
```
- **入力**: 階層レベル（0-4）
- **戻り値**: HierarchyLevel インスタンス
- **例外**: ArgumentOutOfRangeException（0-4 範囲外）

#### TryFrom(int value, out HierarchyLevel result)
```csharp
public static bool TryFrom(int value, out HierarchyLevel result)
```
- **入力**: 階層レベル
- **戻り値**: 成功時 true、失敗時 false
- **結果**: 検証失敗時も有効なインスタンスを返す（例外なし）

#### TryFromDbValue(int value, out HierarchyLevel result)
```csharp
public static bool TryFromDbValue(int value, out HierarchyLevel result)
```
- **入力**: DB読み込み値
- **戻り値**: 成功時 true、失敗時 false

### ファクトリメソッド（便宜）

```csharp
public static HierarchyLevel Company() => From(0);
public static HierarchyLevel Division() => From(1);
public static HierarchyLevel Department() => From(2);
public static HierarchyLevel Group() => From(3);
public static HierarchyLevel Team() => From(4);
```

### プロパティ

#### Value
```csharp
public int Value => ValueField;
```
- 階層レベル値を取得（0-4）

---

## 4. 検証ルール

| 項目 | ルール | 例 |
|-----|-------|-----|
| 範囲 | 0-4 のみ | 0 ✓, 4 ✓, -1 ✗, 5 ✗ |
| 型 | int | 0 ✓ |

---

## 5. テストケース

### From メソッド
- ✅ From(0) → IsSet=true, Value=0, ToString()="Company"
- ✅ From(1) → Value=1, ToString()="Division"
- ✅ From(2) → Value=2, ToString()="Department"
- ✅ From(3) → Value=3, ToString()="Group"
- ✅ From(4) → Value=4, ToString()="Team"
- ❌ From(-1) → ArgumentOutOfRangeException
- ❌ From(5) → ArgumentOutOfRangeException

### TryFrom メソッド
- ✅ TryFrom(0, out result) → true
- ✅ TryFrom(0-4, out result) → true
- ✅ TryFrom(-1, out result) → false
- ✅ TryFrom(5, out result) → false

### ファクトリメソッド
- ✅ Company() → Value=0, ToString()="Company"
- ✅ Division() → Value=1, ToString()="Division"
- ✅ Department() → Value=2
- ✅ Group() → Value=3
- ✅ Team() → Value=4

### 等価性
- ✅ From(0).Equals(From(0)) → true
- ❌ From(0).Equals(From(1)) → false
- ✅ From(0) == From(0) → true

### ToString
- ✅ From(0).ToString() → "Company"
- ✅ From(1).ToString() → "Division"
- ✅ From(2).ToString() → "Department"
- ✅ From(3).ToString() → "Group"
- ✅ From(4).ToString() → "Team"

---

## 6. 依存関係

- `SupportAdvance.SharedKernel.ValueObjects.EnumValueObject`
- `System.IEquatable`

---

## 7. 実装上の注意

1. **IsSet**: 常に true（必須フィールド）
2. **範囲**: 0-4 の固定値のみ許可
3. **Display Name**: GetDisplayName() で日本語名を返す
4. **ファクトリメソッド**: Company() など便宜メソッドを提供して可読性向上

