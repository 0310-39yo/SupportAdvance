# PermissionCode — 技術仕様書

**対象:** PermissionCode ValueObject  
**層:** SharedKernel / ValueObjects / Identifiers  
**作成日:** 2026-08-10  
**版:** 1.0

---

## 1. 概要

権限コードを表す ValueObject。DB スキーマの `m_permissions.permission_code` に対応。

### 責務
- 権限コードを型安全に保持
- 値の検証（1-100文字、英数字と一部特殊文字）
- 等価性判定とハッシュコード計算

---

## 2. 仕様

### 型定義
```csharp
public sealed class PermissionCode : PrimitiveValueObject<string>, IEquatable<PermissionCode>
```

### 基底型
- `PrimitiveValueObject<string>`
- IsSet = true（常に設定済み）

### 値の制約
- **型**: string
- **長さ**: 1-100文字
- **文字種**: 英数字、アンダースコア（_）、ドット（.）のみ
- **例**: "Employee.Create", "Employee.Read.Own", "Employee.Delete"
- **null**: 許可しない（必須）

---

## 3. 公開インターフェース

### 工場メソッド

#### From(string value)
```csharp
public static PermissionCode From(string value)
```
- **入力**: 権限コード文字列
- **戻り値**: PermissionCode インスタンス
- **例外**: ArgumentException（検証失敗時）

#### TryFrom(string? input, out PermissionCode result)
```csharp
public static bool TryFrom(string? input, out PermissionCode result)
```
- **入力**: 権限コード文字列（null許容）
- **戻り値**: 成功時 true、失敗時 false
- **結果**: 検証失敗時も有効なインスタンスを返す（例外なし）

#### TryFromDbValue(string? input, out PermissionCode result)
```csharp
public static bool TryFromDbValue(string? input, out PermissionCode result)
```
- **入力**: DB読み込み値（null許容）
- **戻り値**: 成功時 true、失敗時 false

### プロパティ

#### Value
```csharp
public string Value => ValueField;
```
- 権限コード文字列を取得

---

## 4. 検証ルール

| 項目 | ルール | 例 |
|-----|-------|-----|
| 長さ | 1-100文字 | "Employee.Create" ✓, "" ✗, "A"*101 ✗ |
| 文字種 | 英数字、_、.のみ | "Employee.Create" ✓, "A-D" ✗ |
| null | 許可しない | null ✗ |

---

## 5. テストケース

### From メソッド
- ✅ From("Employee.Create") → IsSet=true, Value="Employee.Create"
- ✅ From("Employee.Read.Own") → IsSet=true
- ❌ From(null) → ArgumentException
- ❌ From("") → ArgumentException
- ❌ From("A"*101) → ArgumentException（101文字）
- ❌ From("A-D") → ArgumentException（ハイフン不可）

### TryFrom メソッド
- ✅ TryFrom("Employee.Create", out result) → true
- ✅ TryFrom(null, out result) → false
- ✅ TryFrom("", out result) → false

### 等価性
- ✅ From("Employee.Create").Equals(From("Employee.Create")) → true
- ❌ From("Employee.Create").Equals(From("Employee.create")) → false

### ToString
- ✅ From("Employee.Create").ToString() → "Employee.Create"

---

## 6. 依存関係

- `SupportAdvance.SharedKernel.ValueObjects.Abstractions.PrimitiveValueObject`
- `System.IEquatable`

---

## 7. 実装上の注意

1. **IsSet**: 常に true（必須フィールド）
2. **大文字小文字区別**: "Employee.Create" と "Employee.create" は別の値
3. **許可文字**: A-Z, a-z, 0-9, _, . のみ
4. **最大長**: 100文字（DB制約に合わせる）
5. **ドット**: 階層構造を表現する（例：Resource.Action.Scope）

