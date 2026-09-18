# Phase 4-E: E2E テスト・フルフロー検証ガイド

## 📋 テスト目的

Authentication BC の完全なログインフロー を検証する：
1. **アプリ起動** → LoginDialog が表示
2. **認証処理** → ログイン成功/失敗の処理
3. **セッション管理** → RealCurrentUserService にユーザー情報が保持
4. **UI 遷移** → 成功時は Form1 表示、キャンセル時はアプリ終了

---

## 🔧 事前準備

### 1. テストユーザーをDB に挿入

```sql
-- SQL Server Management Studio で以下を実行
USE [SupportAdvance];

-- Setup スクリプトを実行
-- ファイル: tests/Contexts/Authentication.Infrastructure.Tests/Setup/InsertTestLoginCredentials.sql
```

**挿入されるテストデータ：**

| login_id | password | status | 用途 |
|----------|----------|--------|------|
| test_user_001 | password123 | active | ログイン成功ケース |
| test_user_inactive | password123 | inactive | ログイン失敗ケース |

### 2. アプリ構成確認

- Program.cs：LoginDialog が DI に登録されている ✅
- RealCurrentUserService：DI に登録されている ✅
- ビルド完了 ✅

---

## 🧪 テスト実行ステップ

### テストケース 1: 正常系（ログイン成功）

#### 前提
- テストユーザー `test_user_001` がアクティブ状態

#### 手順
1. **WinTrial.exe を実行**
   ```bash
   D:\SupportAdvance\src\Presentation\WinTrial\bin\Debug\net10.0-windows\WinTrial.exe
   ```

2. **LoginDialog が表示されることを確認**
   - ウィンドウタイトル: 「ログイン」
   - UI要素:
     - ログインID入力フィールド
     - パスワード入力フィールド
     - ログインボタン
     - キャンセルボタン

3. **テストユーザーでログイン**
   - ログインID: `test_user_001`
   - パスワード: `password123`
   - [ログイン] ボタンをクリック

4. **ログイン成功を確認**
   - LoginDialog が閉じられる
   - Form1（メイン画面）が表示される
   - 画面に「ログイン中のユーザー情報」が表示される（デバッグ表示）

#### 期待結果
✅ ログイン成功、Form1 表示、RealCurrentUserService に user_001 が保持

---

### テストケース 2: 異常系（パスワード間違い）

#### 手順
1. LoginDialog で以下を入力
   - ログインID: `test_user_001`
   - パスワード: `wrong_password`

2. [ログイン] ボタンをクリック

#### 期待結果
❌ エラーメッセージ表示: 「パスワードが間違っています」
- LoginDialog は閉じない
- パスワードフィールドがクリアされる

---

### テストケース 3: 異常系（ユーザー非アクティブ）

#### 手順
1. LoginDialog で以下を入力
   - ログインID: `test_user_inactive`
   - パスワード: `password123`

2. [ログイン] ボタンをクリック

#### 期待結果
❌ エラーメッセージ表示: 「このアカウントは無効です」

---

### テストケース 4: 異常系（ユーザー不在）

#### 手順
1. LoginDialog で以下を入力
   - ログインID: `nonexistent_user`
   - パスワード: `password123`

2. [ログイン] ボタンをクリック

#### 期待結果
❌ エラーメッセージ表示: 「ログインIDが見つかりません」

---

### テストケース 5: キャンセル

#### 手順
1. LoginDialog で [キャンセル] ボタンをクリック

#### 期待結果
❌ アプリケーションが終了される
- LoginDialog が閉じられ、Form1 は表示されない

---

## ✅ テスト完了チェックリスト

- [ ] テストケース 1（ログイン成功）合格
- [ ] テストケース 2（パスワード間違い）合格
- [ ] テストケース 3（ユーザー非アクティブ）合格
- [ ] テストケース 4（ユーザー不在）合格
- [ ] テストケース 5（キャンセル）合格
- [ ] ログイン後 RealCurrentUserService.IsLoggedIn == true
- [ ] ログイン後 RealCurrentUserService.EmployeeRowId == 2147483730
- [ ] ログイン後 RealCurrentUserService.LoginId == "test_user_001"

---

## 🐛 デバッグ情報

### ログイン情報表示（開発用）

RealCurrentUserService にデバッグメソッドを追加（オプション）：

```csharp
public void PrintDebugInfo()
{
    Console.WriteLine($"[Login Info]");
    Console.WriteLine($"  IsLoggedIn: {IsLoggedIn}");
    Console.WriteLine($"  EmployeeRowId: {_employeeRowId}");
    Console.WriteLine($"  LoginId: {_loginId}");
}
```

Form1 起動時に呼び出し：

```csharp
public partial class Form1 : Form
{
    private readonly ICurrentUserService _currentUserService;
    
    public Form1(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
        InitializeComponent();
        
        // デバッグ表示
        if (_currentUserService is RealCurrentUserService realService)
        {
            realService.PrintDebugInfo();
        }
    }
}
```

---

## 📊 テスト結果記録

| テストケース | 実行日時 | 結果 | 備考 |
|-----------|---------|------|------|
| 1（ログイン成功） | | | |
| 2（パスワード間違い） | | | |
| 3（ユーザー非アクティブ） | | | |
| 4（ユーザー不在） | | | |
| 5（キャンセル） | | | |

---

## 📝 次のステップ

- [ ] UI 自動テスト実装（UIAutomation）
- [ ] 認証フロー全体のドキュメント作成
- [ ] 本番環境へのデプロイ準備
