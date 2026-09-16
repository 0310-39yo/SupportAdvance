# 結合テスト仕様書 — SequenceProvider

**プロジェクト:** SupportAdvance  
**テスト対象:** Application ↔ Infrastructure (SequenceProvider)  
**テストレベル:** 結合テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

SequenceProvider は、SQL Server の Sequence（`dbo.s_row_id_sequence`）から一意の ID 値を採番する Infrastructure 層のサービスです。

本結合テストは、**Application層が SequenceProvider に依存して ID を採番する際、実SQL Serverとの統合が正常に動作すること** を検証します。

---

## 1. テスト目的

SequenceProvider が以下を満たすことを確認する：

- **実SQL Server接続**: 実データベース接続経由で NEXT VALUE FOR s_row_id_sequence を実行できる
- **採番値の一意性**: 複数呼び出しで重複しない ID が採番される
- **連続性**: ID が単調増加する
- **トランザクション整合性**: 並行呼び出しでも ID が重複しない

---

## 2. テスト対象の層間・コンポーネント

| 項目 | 内容 |
|-----|-----|
| **層間** | Infrastructure（SequenceProvider）|
| **関連するApplication層** | Repository 層が SequenceProvider に依存 |
| **テスト用Mock/Stub** | 実SQL Server（テスト用DBインスタンス）|
| **テスト分類** | [Trait("Category", "Integration")] |

---

## 3. テスト環境要件

### 前提条件

- SQL Server 2019 以上（テスト環境）
- `dbo.s_row_id_sequence` が存在する
- 接続文字列が `IDbConnectionFactory` で取得可能

### セットアップ手順

```sql
-- テスト用DB初期化
IF EXISTS (SELECT * FROM sys.sequences WHERE name = 's_row_id_sequence')
    DROP SEQUENCE dbo.s_row_id_sequence;

CREATE SEQUENCE dbo.s_row_id_sequence
    AS BIGINT
    START WITH 10000
    INCREMENT BY 1
    NO CACHE;
```

---

## 4. テストケース

### TC-1: GetNextIdAsync 基本動作

**テスト名:** `GetNextIdAsync_WithValidConnection_ReturnsIncrementingIds`

**前提:** SQL Server に接続可能

**実行:**
```csharp
var provider = new SequenceProvider(dbConnectionFactory);
var id1 = await provider.GetNextIdAsync();
var id2 = await provider.GetNextIdAsync();
var id3 = await provider.GetNextIdAsync();
```

**期待結果:**
- id1 < id2 < id3
- 値は正の bigint
- 毎回異なる値

---

### TC-2: 並行呼び出し時の一意性

**テスト名:** `GetNextIdAsync_WithConcurrentCalls_EnsuresUniqueness`

**実行:**
```csharp
var provider = new SequenceProvider(dbConnectionFactory);
var tasks = Enumerable.Range(0, 10)
    .Select(_ => provider.GetNextIdAsync())
    .ToList();

var ids = await Task.WhenAll(tasks);
var distinctIds = ids.Distinct().Count();
```

**期待結果:**
- distinctIds == 10（重複なし）

---

### TC-3: DB接続失敗時の例外

**テスト名:** `GetNextIdAsync_WithInvalidConnection_ThrowsException`

**前提:** 接続文字列が無効

**期待結果:**
- SqlException がスロー

---

## 5. 実行方法

```bash
# 結合テストのみ実行
dotnet test --filter "Category=Integration"

# 特定のSequenceProviderテストのみ実行
dotnet test --filter "Category=Integration&FullyQualifiedName~SequenceProvider"
```

---

## 6. 前提条件・制限事項

- **必須環境**: 実SQL Server（テスト用インスタンス）
- **実装言語**: C# 11+
- **テスト実装**: xUnit 2.0+ with [Trait]
- **トランザクション**: 各テスト終了後に SEQUENCE をリセット

---

**ドキュメント完成** ✅
