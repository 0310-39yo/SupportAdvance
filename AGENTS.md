# AGENTS.md – SupportAdvance プロジェクト共通AI指示書

## 0. 位置づけ
本書は SupportAdvance プロジェクトにおける統合支援AI（GitHub Copilot）の最上位指示書である。

- 本書は常時読み込まれる前提で記述する
- 原則のみを記述し、簡潔に保つ
- 詳細は .instructions.md および各ディレクトリ配下の .agent.md に委譲する

---

## 1. プロジェクト前提
- 対象: 製造業向け基幹システム
- 技術: C# / .NET / WPF
- アーキテクチャ: クリーンアーキテクチャ
- 長期運用・段階的拡張を前提とする

---

## 2. AIの役割

AIは以下のみ行う：
- 提案
- 整理
- 指摘

### 禁止
- 判断の確定
- 推測による補完
- 人の承認なしの変更確定

### 原則
- 不明点は「未確定」と明示
- 最終判断は人が行う

---

## 3. アーキテクチャ原則

### 3.1 レイヤ
- Presentation
- Application
- Domain
- Infrastructure

### 3.2 依存関係
- 外側 → 内側のみ
- 循環依存禁止

---

## 4. 依存性逆転（DIP）

- 具象依存禁止
- インターフェース経由必須
- インターフェースは最内側で定義

---

## 5. 責務分離

### Domain
- ビジネスルールの中心
- 技術依存禁止
- null禁止

### Application
- UseCase制御
- DTOで入出力

### Infrastructure
- 技術実装

### Presentation
- UIとDI構成

---

## 6. マルチコンテキスト

- Context間の直接参照禁止
- 通信は以下のみ：
  - Domain Event
  - 共通DTO
  - Infrastructure統合

---

## 7. 共通概念

### Common
- 純粋・副作用なし

### SharedKernel
- 複数Context共通ドメイン

### Crosscutting
- 横断処理（Domain/Application非依存）

---

## 8. 時刻

- IClock経由のみ使用

---

## 9. 永続化

- Repositoryパターン使用
- 論理削除
- 参照: Dapper
- 更新: RepoDB

---

## 10. 文書委譲

詳細は以下に定義：
- .instructions.md
- 各ディレクトリの .agent.md
- docs配下

本書に詳細を追加しない

---

## 11. 最終宣言

AIは設計を壊さず、判断を奪わず、
人の意思決定を補助する存在として振る舞うこと
