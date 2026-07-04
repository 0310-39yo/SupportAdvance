---
applyTo: '**/*.cs'
---

# SupportAdvance 実装指示（具体ルール）

## 1. 基本方針
- AGENTS.mdの原則を必ず遵守する
- 実装は必ずレイヤ責務に従う

---

## 2. コーディング規約

### 命名
- クラス：PascalCase
- メソッド：動詞から開始
- プロパティ：PascalCase
- ローカル変数：camelCase

### コメント
- 日本語で記述
- 体言止め
- 句点なし

---

## 3. DIルール

### 必須
- コンストラクタインジェクション（CI）を使用

### 禁止
- Service Locator パターン
- static DIアクセス
- new による依存生成（Domain/Application）

---

## 4. Domainルール

### 必須
- null禁止
- ValueObjectで表現
- 不変性を維持

### 禁止
- DateTime.Now / UtcNow の使用
- DB / Logger / UI 依存

---

## 5. Applicationルール

### 必須
- UseCase単位で実装
- DTOで入出力

### 禁止
- Infrastructureの具象参照

---

## 6. Infrastructureルール

### 必須
- Repository実装
- 外部依存処理を担当

### DBアクセス
- 参照：Dapper
- 更新：RepoDB

---

## 7. Presentationルール

### 必須
- DI構成を担当
- ViewModel中心設計

### 禁止
- Domain / Application直接参照

---

## 8. ポート設計

### 原則
- RepositoryはDomainに定義
- UseCaseはApplicationに定義

---

## 9. 時刻

### 必須
IClock をDIで受け取る

例：
private readonly IClock _clock;

### 禁止
DateTime.Now  
DateTime.UtcNow  

---

## 10. テスト

### 必須
- Domain：単体テスト
- Application：UseCaseテスト
- Infrastructure：結合テスト

---

## 11. NGパターン

### 禁止例

・具象依存  
private SqlRepository _repo;

・Service Locator  
var repo = provider.GetService<IRepo>();

・Domainでnew  
new SqlRepository();

---

## 12. 判断ルール

不明な場合は以下を必ず守る：

- 推測しない
- 未確定として明示する
- 複数案を提示する