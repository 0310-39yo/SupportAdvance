# ManagerEmployeeRowId — 技術仕様書

**対象:** ManagerEmployeeRowId ValueObject  
**層:** SharedKernel / ValueObjects / Identifiers  
**作成日:** 2026-08-10  
**版:** 1.0

---

## 1. 概要

部署の管理者（従業員）の行IDを表すオプション ValueObject。DB スキーマの `m_departments.manager_employee_row_id` に対応。

### 責務
- 管理者従業員行ID（1以上）をオプション型で型安全に保持
- 値の検証（正の整数）
- null を Unset に変換（IOptionalValueObject パターン）
- 等価性判定とハッシュコード計算

---

## 2. 仕様

### 型定義
```csharp
public sealed class ManagerEmployeeRowId : PrimitiveValueObject<long?>, IEquatable<ManagerEmployeeRowId>
```

### 基底型
- `PrimitiveValueObject<long?>`
- IsSet = true/false（管理者がいる/いない）

### 値の制約
- **型**: long?（nullable）
- **範囲**: 1 以上（管理者がいない場合は NULL/Unset）
- **null**: 許可（管理者なしの部署）

---

## 3. 公開インターフェース

### 工場メソッド

#### From(long value)
```csharp
public static ManagerEmployeeRowId From(long value)
```
- **入力**: 管理者従業員行ID（1以上）
- **戻り値**: ManagerEmployeeRowId インスタンス
- **例外**: ArgumentException（0以下の場合）

#### Unset()
```csharp
public static ManagerEmployeeRowId Unset()
```
- **戻り値**: IsSet=false のインスタンス（管理者なし）

#### TryFrom(long? input, out ManagerEmployeeRowId result)
```csharp
public static bool TryFrom(long? input, out ManagerEmployeeRowId result)
```
- **入力**: 管理者従業員行ID（null許容）
- **戻り値**: 成功時 true、検証失敗時 false
- **動作**: null は Unset に変換して成功を返す

#### TryFromDbValue(long? input, out ManagerEmployeeRowId result)
```csharp
public static bool TryFromDbValue(long? input, out ManagerEmployeeRowId result)
```
- **入力**: DB読み込み値（null許容）
- **戻り値**: 成功時 true、失敗時 false

### プロパティ

#### Value
```csharp
public long? Value => ValueField;
```
- 管理者従業員行IDを取得（IsSet=false の場合は null）

#### HasManager
```csharp
public bool HasManager => IsSet;
```
- 管理者がいるかどうかを判定（IsSet の別名）

---

## 4. 検証ルール

| 項目 | ルール | 例 |
|-----|-------|-----|
| 値あり | 1以上 | 1 ✓, 9999 ✓ |
| 値なし | NULL → Unset | null ✓（Unset に変換） |
| 無効 | 0以下 | 0 ✗, -1 ✗ |

---

## 5. テストケース

### From メソッド
- ✅ From(1) → IsSet=true, Value=1
- ❌ From(0) → ArgumentException
- ❌ From(-1) → ArgumentException

### Unset メソッド
- ✅ Unset() → IsSet=false, Value=null

### TryFrom メソッド
- ✅ TryFrom(1, out result) → true, IsSet=true
- ✅ TryFrom(null, out result) → true, IsSet=false（null吸収）
- ✅ TryFrom(0, out result) → false

### 等価性
- ✅ From(1).Equals(From(1)) → true
- ✅ Unset().Equals(Unset()) → true
- ❌ From(1).Equals(Unset()) → false

---

## 6. 依存関係

- `SupportAdvance.SharedKernel.ValueObjects.Abstractions.PrimitiveValueObject`
- `System.IEquatable`

---

## 7. 実装上の注意

1. **IsSet**: true=管理者あり, false=管理者なし
2. **null吸収**: TryFrom(null) は false ではなく Unset + true を返す
3. **参考**: ParentDepartmentRowId と同じオプションパターン

