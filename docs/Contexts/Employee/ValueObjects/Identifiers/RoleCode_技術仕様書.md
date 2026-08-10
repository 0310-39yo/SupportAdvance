# RoleCode — 技術仕様書

**対象:** RoleCode ValueObject  
**層:** SharedKernel / ValueObjects / Identifiers  
**作成日:** 2026-08-10  
**版:** 1.0

---

## 1. 概要

ロールコードを表す ValueObject。DB スキーマの `m_roles.role_code` に対応。

### 責務
- ロールコードを型安全に保持
- 値の検証（1-50文字、英数字と一部特殊文字）
- 等価性判定とハッシュコード計算

---

## 2. 仕様

### 型定義
```csharp
public sealed class RoleCode : PrimitiveValueObject<string>, IEquatable<RoleCode>
```

### 基底型
- `PrimitiveValueObject<string>`
- IsSet = true（常に設定済み）

### 値の制約
- **型**: string
- **長さ**: 1-50文字
- **文字種**: 英数字、アンダースコア（_）、ドット（.）のみ
- **例**: "ADMIN", "MANAGER", "VIEWER", "user.viewer"
- **null**: 許可しない（必須）

---

## 3. 公開インターフェース

### 工場メソッド

#### From(string value)
```csharp
public static RoleCode From(string value)
```
- **入力**: ロールコード文字列
- **戻り値**: RoleCode インスタンス
- **例外**: ArgumentException（検証失敗時）

#### TryFrom(string? input, out RoleCode result)
```csharp
public static bool TryFrom(string? input, out RoleCode result)
```
- **入力**: ロールコード文字列（null許容）
- **戻り値**: 成功時 true、失敗時 false
- **結果**: 検証失敗時も有効なインスタンスを返す（例外なし）

#### TryFromDbValue(string? input, out RoleCode result)
```csharp
public static bool TryFromDbValue(string? input, out RoleCode result)
```
- **入力**: DB読み込み値（null許容）
- **戻り値**: 成功時 true、失敗時 false

### プロパティ

#### Value
```csharp
public string Value => ValueField;
```
- ロールコード文字列を取得

---

## 4. 検証ルール

| 項目 | ルール | 例 |
|-----|-------|-----|
| 長さ | 1-50文字 | "ADMIN" ✓, "" ✗, "A"*51 ✗ |
| 文字種 | 英数字、_、.のみ | "ADMIN" ✓, "admin_viewer" ✓, "A-D" ✗ |
| null | 許可しない | null ✗ |

---

## 5. テストケース

### From メソッド
- ✅ From("ADMIN") → IsSet=true, Value="ADMIN"
- ✅ From("user_viewer") → IsSet=true
- ✅ From("admin.role") → IsSet=true
- ❌ From(null) → ArgumentException
- ❌ From("") → ArgumentException
- ❌ From("A-D") → ArgumentException（ハイフン不可）

### TryFrom メソッド
- ✅ TryFrom("ADMIN", out result) → true
- ✅ TryFrom(null, out result) → false
- ✅ TryFrom("", out result) → false

### 等価性
- ✅ From("ADMIN").Equals(From("ADMIN")) → true
- ❌ From("ADMIN").Equals(From("admin")) → false（大文字小文字区別）

### ToString
- ✅ From("ADMIN").ToString() → "ADMIN"

---

## 6. 依存関係

- `SupportAdvance.SharedKernel.ValueObjects.Abstractions.PrimitiveValueObject`
- `System.IEquatable`

---

## 7. 実装上の注意

1. **IsSet**: 常に true（必須フィールド）
2. **大文字小文字区別**: "ADMIN" と "admin" は別の値
3. **許可文字**: A-Z, a-z, 0-9, _, . のみ
4. **最大長**: 50文字（DB制約に合わせる）

