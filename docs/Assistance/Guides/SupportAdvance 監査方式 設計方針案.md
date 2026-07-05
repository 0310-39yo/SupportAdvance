# SupportAdvance 監査方式 設計方針案

## 1. 目的

本システムにおける監査の目的は、以下を追跡可能にすることである。

- 誰が変更したか
- いつ変更したか
- 何を変更したか
- 変更前はどのような値であったか

監査は業務ロジックではなく、データ変更履歴の保全を目的とする。

------

# 2. 基本方針

SupportAdvance では監査専用ライブラリや独自監査ログテーブルを使用せず、SQL Server の Temporal Table 機能を利用する。

構成は以下とする。



Application

  ↓

RepoDB

  ↓

SQL Server

 ├─ Trigger

 ├─ Temporal Table

 └─ Session Context



------

# 3. テーブル設計

全業務テーブルに以下の監査項目を持たせる。



CreatedAt

CreatedBy

UpdatedAt

UpdatedBy

UpdatedSource



## 項目説明

| 項目          | 内容           |
| ------------- | -------------- |
| CreatedAt     | 作成日時       |
| CreatedBy     | 作成者         |
| UpdatedAt     | 更新日時       |
| UpdatedBy     | 更新者         |
| UpdatedSource | 更新元(App/DB) |

------

# 4. Temporal Table の利用

主要テーブルは Temporal Table とする。

対象例



Orders

OrderDetails

Customers

Products

ProductionPlans



Temporal Table を有効にすると SQL Server が履歴テーブルを自動管理する。

例



Orders

↓

OrdersHistory



更新時



Orders

  ↓

UPDATE

  ↓

OrdersHistoryに旧データ保存

Ordersに新データ保存



削除時



DELETE

  ↓

削除前データを履歴保存



アプリケーション側で履歴保存処理は不要。

------

# 5. Session Context 利用方針

アプリ接続時にログインユーザーを Session Context に設定する。



EXEC sp_set_session_context

  @key = N'UserId',

  @value = N'KATO';



------

# 6. Trigger による監査項目自動更新

更新時はトリガーで監査項目を設定する。

## 更新日時

UpdatedAt = SYSUTCDATETIME()

------

## 更新者



COALESCE(

  SESSION_CONTEXT(N'UserId'),

  ORIGINAL_LOGIN()

)



利用。

------

## 更新元



CASE

  WHEN SESSION_CONTEXT(N'UserId') IS NULL

​    THEN 'DB'

  ELSE 'APP'

END



利用。

------

# 7. 更新時の動作

## アプリ経由



Application

 ↓

UserId = KATO

 ↓

UPDATE Orders



結果



UpdatedBy   = KATO

UpdatedSource = APP



------

## SSMS直接更新



UPDATE Orders

SET Amount = 999999



結果



UpdatedBy   = Administrator

UpdatedSource = DB



------

# 8. 取得できる監査情報

Temporal Table と監査項目により以下を取得可能。

| 内容                 | 取得可否 |
| -------------------- | -------- |
| 変更前の値           | ○        |
| 変更後の値           | ○        |
| 更新日時             | ○        |
| 更新者               | ○        |
| アプリ更新かDB更新か | ○        |
| 削除前の値           | ○        |
| 過去時点データ       | ○        |

------

# 9. 取得できない情報

以下は Temporal Table では取得できない。

| 内容                   | 取得可否 |
| ---------------------- | -------- |
| どの画面から変更したか | ×        |
| 変更理由               | ×        |
| 承認した               | ×        |
| 差戻した               | ×        |
| ログイン履歴           | ×        |

------

# 10. SQL Server Audit の位置づけ

本システムでは原則として DB 直接更新を禁止する。

ただし内部統制上、

誰がSQLを実行したか

まで追跡する必要が生じた場合は SQL Server Audit を利用する。

役割分担は以下とする。

| 機能         | Temporal Table | SQL Server Audit |
| ------------ | -------------- | ---------------- |
| 変更前後の値 | ○              | ×                |
| 削除履歴     | ○              | ×                |
| 過去時点復元 | ○              | ×                |
| 実行ユーザー | △（UpdatedBy） | ○                |
| 実行SQL      | ×              | ○                |

------

# 11. 結論

SupportAdvance の監査方式としては、



CreatedAt

CreatedBy

UpdatedAt

UpdatedBy

UpdatedSource

＋

SQL Server Temporal Table

＋

SESSION_CONTEXT

＋

Trigger



を採用する。

これにより、

- RepoDBに追加ライブラリ不要
- DDDに監査ロジックを持ち込まない
- アプリ更新とDB直接更新を識別可能
- 変更前後の全履歴を自動保存可能
- 将来的に SQL Server Audit を追加可能

というシンプルで保守性の高い監査基盤を実現できる。