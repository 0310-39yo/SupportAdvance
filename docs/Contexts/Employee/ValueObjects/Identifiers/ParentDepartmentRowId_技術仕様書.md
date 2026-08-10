# ParentDepartmentRowId — 技術仕様書

**対象:** ParentDepartmentRowId ValueObject  
**層:** SharedKernel / ValueObjects / Identifiers  
**作成日:** 2026-08-10  
**版:** 1.0

---

## 1. 概要

親部署の行IDを表すオプション ValueObject。DB スキーマの `m_departments.parent_row_id` に対応。

### 責務
- 親部署行ID（1以上）をオプション型で型安全に保持
- 値の検証（正の整数）
- null を Unset に変換（IOptionalValueObject パターン）
- 等価性判定とハッシュコード計算

---

## 2. 仕様

### 型定義
```csharp
public sealed class ParentDepartmentRowId : PrimitiveValueObject<long?>, IEquatable<ParentDepartmentRowId>
```

### 基底型
- `PrimitiveValueObject<long?>`
- IsSet = true/false（親部署がある/ない）

### 値の制約
- **型**: long?（nullable）
- **範囲**: 1 以上（ルート部署は NULL/Unset）
- **null**: 許可（ルート部署は親なし）

---

## 3. 公開インターフェース

### 工場メソッド

#### From(long value)
```csharp
public static ParentDepartmentRowId From(long value)
```
- **入力**: 親部署行ID（1以上）
- **戻り値**: ParentDepartmentRowId インスタンス
- **例外**: ArgumentException（0以下の場合）

#### Unset()
```csharp
public static ParentDepartmentRowId Unset()
```
- **戻り値**: IsSet=false のインスタンス（親部署なし）

#### TryFrom(long? input, out ParentDepartmentRowId result)
```csharp
public static bool TryFrom(long? input, out ParentDepartmentRowId result)
```
- **入力**: 親部署行ID（null許容）
- **戻り値**: 成功時 true、検証失敗時 false
- **動作**: null は Unset に変換して成功を返す

#### TryFromDbValue(long? input, out ParentDepartmentRowId result)
```csharp
public static bool TryFromDbValue(long? input, out ParentDepartmentRowId result)
```
- **入力**: DB読み込み値（null許容）
- **戻り値**: 成功時 true、失敗時 false

### プロパティ

#### Value
```csharp
public long? Value => ValueField;
```
- 親部署行IDを取得（IsSet=false の場合は null）

#### HasParent
```csharp
public bool HasParent => IsSet;
```
- 親部署があるかどうかを判定（IsSet の別名）

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

1. **IsSet**: true=親あり, false=ルート部署（親なし）
2. **null吸収**: TryFrom(null) は false ではなく Unset + true を返す
3. **参考**: UpdatedBy, DeletedBy と同じオプションパターン

