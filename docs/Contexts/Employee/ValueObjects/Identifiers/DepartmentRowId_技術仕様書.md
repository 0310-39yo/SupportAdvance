# DepartmentRowId — 技術仕様書

**対象:** DepartmentRowId ValueObject  
**層:** SharedKernel / ValueObjects / Identifiers  
**作成日:** 2026-08-10  
**版:** 1.0

---

## 1. 概要

データベース上の部署レコードの行ID（rowId）を表す ValueObject。DB スキーマの `m_departments.row_id` に対応。

### 責務
- 部署行ID（1以上）を型安全に保持
- 値の検証（正の整数）
- 等価性判定とハッシュコード計算

---

## 2. 仕様

### 型定義
```csharp
public sealed class DepartmentRowId : PrimitiveValueObject<long>, IEquatable<DepartmentRowId>
```

### 基底型
- `PrimitiveValueObject<long>`
- IsSet = true（常に設定済み）

### 値の制約
- **型**: long
- **範囲**: 1 以上（long.MaxValue 以下）
- **null**: 許可しない（必須）
- **ゼロ以下**: 許可しない

---

## 3. 公開インターフェース

### 工場メソッド

#### From(long value)
```csharp
public static DepartmentRowId From(long value)
```
- **入力**: 部署行ID（1以上）
- **戻り値**: DepartmentRowId インスタンス
- **例外**: ArgumentOutOfRangeException（0以下の場合）

#### TryFrom(long value, out DepartmentRowId result)
```csharp
public static bool TryFrom(long value, out DepartmentRowId result)
```
- **入力**: 部署行ID
- **戻り値**: 成功時 true、失敗時 false
- **結果**: 検証失敗時も有効なインスタンスを返す（例外なし）

#### TryFromDbValue(long value, out DepartmentRowId result)
```csharp
public static bool TryFromDbValue(long value, out DepartmentRowId result)
```
- **入力**: DB読み込み値
- **戻り値**: 成功時 true、失敗時 false

### プロパティ

#### Value
```csharp
public long Value => ValueField;
```
- 部署行ID を取得（常に 1 以上）

---

## 4. 検証ルール

| 項目 | ルール | 例 |
|-----|-------|-----|
| 範囲 | 1以上 | 1 ✓, 9999 ✓, 0 ✗, -1 ✗ |
| 型 | long | 1L ✓ |

---

## 5. テストケース

### From メソッド
- ✅ From(1) → IsSet=true, Value=1
- ✅ From(9999) → IsSet=true, Value=9999
- ❌ From(0) → ArgumentOutOfRangeException
- ❌ From(-1) → ArgumentOutOfRangeException

### TryFrom メソッド
- ✅ TryFrom(1, out result) → true
- ✅ TryFrom(0, out result) → false
- ✅ TryFrom(-1, out result) → false

### 等価性
- ✅ From(1).Equals(From(1)) → true
- ❌ From(1).Equals(From(2)) → false

### ToString
- ✅ From(1).ToString() → "1"

---

## 6. 依存関係

- `SupportAdvance.SharedKernel.ValueObjects.Abstractions.PrimitiveValueObject`
- `System.IEquatable`

---

## 7. 実装上の注意

1. **IsSet**: 常に true（必須フィールド）
2. **Nullable パターン**: TryFrom は null ではなく long value を受け取る
3. **参考**: EmployeeRowId, PersonRowId と同じパターン

