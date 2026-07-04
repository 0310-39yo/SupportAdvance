# アーキテクチャ概要 - SupportAdvance

**最終更新**: 2026-06-30  
**準拠レベル**: 8.5 / 10 (良好)

---

## 1. プロジェクトの目的

SupportAdvance は、**製造業向け基幹システム**です。

- **長期運用を前提**: 段階的な拡張に対応できる設計
- **複数コンテキスト対応**: ビジネスドメインごとの機能分離
- **技術スタック**: C# / .NET / WPF

---

## 2. アーキテクチャスタイル

### 採用アーキテクチャ

**クリーンアーキテクチャ** ✅

クリーンアーキテクチャは、以下の原則に基づいています:

1. **独立性**: UI・DB・フレームワークに依存しない
2. **テスト性**: ビジネスロジックをテスト容易に
3. **保守性**: 変更による影響を最小化
4. **拡張性**: 新機能追加時の影響を限定

### 特徴

- ✅ **層の分離**: 外側 → 内側のみ参照（一方向）
- ✅ **循環依存なし**: グラフが DAG（有向非環グラフ）
- ✅ **インターフェース経由**: 依存性逆転で具象依存なし
- ✅ **責務分離**: 各層は明確な責務を持つ

---

## 3. 層構造

```
┌─────────────────────────────────────────────────────────┐
│ PRESENTATION層（UI・DI）                               │
│ ├─ Presentation.WinTrial (Windows Forms)               │
│ ├─ Presentation.WpfTrial (WPF)                         │
│ └─ Presentation.Shared (DI・UI共通)                    │
└─────────────────┬───────────────────────────────────────┘
                  ↓ 依存
┌─────────────────────────────────────────────────────────┐
│ APPLICATION層（UseCase・DTO）                           │
│ ├─ Application (汎用基盤)                              │
│ └─ CarPreferences.Application (Context固有)            │
└─────────────────┬───────────────────────────────────────┘
                  ↓ 依存
┌─────────────────────────────────────────────────────────┐
│ DOMAIN層（ビジネスルール）                              │
│ ├─ SharedKernel (基底・共通)                           │
│ └─ CarPreferences.Domain (Context固有)                │
└─────────────────┬───────────────────────────────────────┘
                  ↓ 依存 (逆向き許容)
┌─────────────────────────────────────────────────────────┐
│ INFRASTRUCTURE層（永続化・技術実装）                    │
│ ├─ Infrastructure (汎用基盤)                           │
│ └─ CarPreferences.Infrastructure (Context固有)        │
└─────────────────┬───────────────────────────────────────┘
                  ↓ 依存
┌─────────────────────────────────────────────────────────┐
│ CROSSCUTTING・COMMON層（横断・基盤）                    │
│ ├─ Crosscutting (ロギング等)                           │
│ └─ Common (設定・時刻等)                               │
└─────────────────────────────────────────────────────────┘
```

### 層の概要

| 層 | 責務 | 参照先 | 依存 |
|---|---|----|-----|
| **Presentation** | UI・DI設定 | Application, Common, Crosscutting | 外側 |
| **Application** | UseCase・DTO | Domain, SharedKernel | 内側 |
| **Domain** | ビジネスルール | Common, SharedKernel | 最内側 |
| **Infrastructure** | DB・永続化 | Application, Crosscutting | 外側（逆向き） |
| **Crosscutting** | 横断処理 | Common | 基盤 |
| **Common** | 設定・ユーティリティ | なし | 最下層 |

---

## 4. 主要な原則

### 原則1: 依存関係は外側 → 内側のみ

```
❌ 禁止:
  Domain → Infrastructure  (内側が外側に依存)
  Infrastructure → Domain  (循環依存)

✅ 許可:
  Presentation → Application (外側が内側に依存)
  Infrastructure → Application (実装層が抽象層に依存 via 依存性逆転)
```

### 原則2: インターフェースは依存される側で定義

```
❌ 禁止:
  Infrastructure に IRepository定義
    ↓ Application が Implementation版を参照

✅ 許可:
  Application に IRepository定義
    ↓ Infrastructure が実装を提供
```

### 原則3: 各層は独立に存在

```
❌ 禁止:
  Presentation層のコードが DB操作を直接実行
  Domain層が SQLクエリを知る

✅ 許可:
  各層は自身の責務のみ実行
  層間通信は定義されたインターフェース経由
```

---

## 5. プロジェクト構成

### 現在のプロジェクト (11個)

| # | プロジェクト名 | 層 | 説明 |
|---|-----------|----|------|
| 1 | Common | Common | 設定・時刻等の純粋ユーティリティ |
| 2 | Crosscutting | Crosscutting | ロギング等の横断処理 |
| 3 | SharedKernel | Domain | エンティティ基底・共通ビジネスルール |
| 4 | Application | Application | UseCase定義・DTO基盤 |
| 5 | Infrastructure | Infrastructure | DB接続・ORM（汎用） |
| 6 | Presentation.Shared | Presentation | DI設定・UI共通 |
| 7 | Presentation.WinTrial | Presentation | Windows Forms UI |
| 8 | Presentation.WpfTrial | Presentation | WPF UI |
| 9 | CarPreferences.Domain | Domain | ビジネスルール（Context固有） |
| 10 | CarPreferences.Application | Application | UseCase実装（Context固有） |
| 11 | CarPreferences.Infrastructure | Infrastructure | 永続化実装（Context固有） |

### マルチコンテキスト構造

```
SupportAdvance/
├─ 汎用層
│  ├─ Common
│  ├─ Crosscutting
│  ├─ Application
│  ├─ Infrastructure
│  └─ Presentation.Shared
│
└─ Context別層（拡張可能）
   ├─ CarPreferences
   │  ├─ CarPreferences.Domain
   │  ├─ CarPreferences.Application
   │  └─ CarPreferences.Infrastructure
   │
   └─ [新規Context例]
      ├─ [NewContext].Domain
      ├─ [NewContext].Application
      └─ [NewContext].Infrastructure
```

---

## 6. 次のステップ

- **詳細**: `01-Layer-Architecture.md` → 各層の詳細責務
- **ルール**: `02-Dependency-Rules.md` → 依存関係の詳細ルール
- **構造**: `07-Project-Structure.md` → プロジェクト構成図

---

## 7. よくある質問

### Q: なぜクリーンアーキテクチャを採用した？

**A**: 
- 長期運用に耐える設計（5年+の保守を想定）
- 新機能追加時の既存コードへの影響を最小化
- ビジネスロジックのテスト化が容易
- チームメンバーの入れ替わりに強い

### Q: Infrastructure層はどこに配置されるのか？

**A**:
- グラフの外側に配置される（逆向き依存）
- Application層が定義したインターフェースを実装
- 依存性逆転により、ビジネスロジックは技術に影響されない

### Q: 新規Contextを追加する場合は？

**A**:
- `[ContextName].Domain`
- `[ContextName].Application`
- `[ContextName].Infrastructure`
- の3層を追加
- 詳細は `06-Multi-Context-Design-Pattern.md` 参照

---

**作成日**: 2026-06-30  
**次回更新予定**: フェーズ2（詳細ドキュメント完成後）
