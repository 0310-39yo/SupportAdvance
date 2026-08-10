# DepartmentCode — 技術仕様書

**対象:** DepartmentCode ValueObject  
**層:** SharedKernel / ValueObjects / Identifiers  
**作成日:** 2026-08-10  
**版:** 1.0

---

## 1. 概要

部署コードを表す ValueObject。DB スキーマの `m_departments.department_code` に対応。

### 責務
- 部署コード（4文字固定）を型安全に保持
- 値の検証（4文字、英数字）
- 等価性判定とハッシュコード計算

---

## 2. 仕様

### 型定義
```csharp
public sealed class DepartmentCode : PrimitiveValueObject<string>, IEquatable<DepartmentCode>
```

### 基底型
- `PrimitiveValueObject<string>`
- IsSet = true（常に設定済み）

### 値の制約
- **型**: string
- **長さ**: 固定4文字
- **文字種**: 英数字のみ（A-Z, 0-9）
- **例**: "G100", "D200", "S050"
- **null**: 許可しない（必須）

---

## 3. 公開インターフェース

### 工場メソッド

#### From(string value)
```csharp
public static DepartmentCode From(string value)
```
- **入力**: 部署コード文字列
- **戻り値**: DepartmentCode インスタンス
- **例外**: ArgumentException（検証失敗時）

#### TryFrom(string? input, out DepartmentCode result)
```csharp
public static bool TryFrom(string? input, out DepartmentCode result)
```
- **入力**: 部署コード文字列（null許容）
- **戻り値**: 成功時 true、失敗時 false
- **結果**: 検証失敗時も有効なインスタンスを返す（例外なし）

#### TryFromDbValue(string? input, out DepartmentCode result)
```csharp
public static bool TryFromDbValue(string? input, out DepartmentCode result)
```
- **入力**: DB読み込み値（null許容）
- **戻り値**: 成功時 true、失敗時 false

### プロパティ

#### Value
```csharp
public string Value => ValueField;
```
- 部署コード文字列を取得（常に4文字）

---

## 4. 検証ルール

| 項目 | ルール | 例 |
|-----|-------|-----|
| 長さ | 固定4文字 | "G100" ✓, "AB" ✗, "ABCDE" ✗ |
| 文字種 | 英数字のみ | "G100" ✓, "G-100" ✗ |
| null | 許可しない | null ✗ |
| 空文字列 | 許可しない | "" ✗ |

---

## 5. テストケース

### From メソッド
- ✅ From("G100") → IsSet=true, Value="G100"
- ✅ From("D200") → IsSet=true, Value="D200"
- ❌ From(null) → ArgumentException
- ❌ From("") → ArgumentException
- ❌ From("AB") → ArgumentException（3文字）
- ❌ From("ABCDE") → ArgumentException（5文字）
- ❌ From("G@00") → ArgumentException（特殊文字）

### TryFrom メソッド
- ✅ TryFrom("G100", out result) → true
- ✅ TryFrom(null, out result) → false
- ✅ TryFrom("", out result) → false
- ✅ TryFrom("AB", out result) → false

### 等価性
- ✅ From("G100").Equals(From("G100")) → true
- ❌ From("G100").Equals(From("D200")) → false
- ✅ From("G100") == From("G100") → true
- ❌ From("G100") == From("g100") → false（大文字小文字区別）

### ToString
- ✅ From("G100").ToString() → "G100"

---

## 6. 依存関係

- `SupportAdvance.SharedKernel.ValueObjects.Abstractions.PrimitiveValueObject`
- `System.IEquatable`

---

## 7. 実装上の注意

1. **大文字小文字区別**: "G100" と "g100" は別の値
2. **先頭/末尾のスペース**: トリムしない
3. **IsSet**: 常に true（必須フィールド）

