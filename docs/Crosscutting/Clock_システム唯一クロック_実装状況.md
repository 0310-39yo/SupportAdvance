# システム唯一クロック 実装状況

**バージョン:** 1.0  
**作成日:** 2025年  
**位置:** Common 層 (`src/Common/Clocks`)

---

## 1. 現状概要

システムで日時を取得する際に使用する「唯一のクロック」が完全に実装されています。

**主な特徴:**
- ✅ **中央集約**: IClock インターフェース経由で全システムが日時取得
- ✅ **環境別対応**: System（本番）/ Ticking（テスト手動）/ Offset（シミュレーション）
- ✅ **型安全**: LocalDateTime ValueObject で JST を表現
- ✅ **DI 統合**: Infrastructure層で Singleton 登録
- ✅ **テスト対応**: MockClock で単体テスト可能

---

## 2. アーキテクチャ

### 2.1 構成図

```
┌─────────────────────────────────────────────────┐
│ Application / Presentation 層                   │
│ （DateTime.Now/UtcNow 直接使用は禁止）          │
└─────────────────┬───────────────────────────────┘
                  │
         ┌────────▼────────┐
         │   IClock DI     │
         │ (Singleton)     │
         └────────┬────────┘
                  │
    ┌─────────────┼─────────────┐
    │             │             │
┌───▼────┐  ┌────▼────┐  ┌────▼────┐
│ System │  │ Ticking │  │ Offset  │
│ Clock  │  │ Clock   │  │ Clock   │
└───┬────┘  └────┬────┘  └────┬────┘
    │            │            │
    └────────────┼────────────┘
         (ClockFactory)
                  │
         ┌────────▼────────┐
         │  LocalDateTime   │
         │  (JST=Unspecified)
         └─────────────────┘
```

### 2.2 DI 登録フロー

```
appsettings.json
    ↓
ClockSettings
    ↓
Infrastructure.DependencyInjection
    ↓
ClockFactory.CreateClock()
    ↓
IClock Singleton 登録
    ↓
各層から DI 経由で注入
```

---

## 3. コアコンポーネント

### 3.1 IClock インターフェース

```csharp
public interface IClock
{
    /// 現在のJST日時を取得
    LocalDateTime JstNow { get; }
    
    /// 本日（00:00:00）のJST日付を取得
    LocalDateTime JstToday { get; }
}
```

**特性:**
- JST（日本標準時）のみを返す
- DateTime.Kind は常に Unspecified（タイムゾーン情報は LocalDateTime に埋め込み）

### 3.2 LocalDateTime ValueObject

```csharp
public readonly struct LocalDateTime : IComparable<LocalDateTime>, IEquatable<LocalDateTime>
{
    public DateTime Value { get; }  // Kind = Unspecified
    
    // JST → UTC 変換
    public DateTime ToUtc();
    
    // UTC → JST 変換
    public static LocalDateTime FromUtc(DateTime utcValue);
    
    // 演算子オーバーロード
    // +, -, ==, !=, <, >, <=, >=
}
```

**特性:**
- struct（ValueObject）
- DateTime.Kind は常に Unspecified
- タイムゾーン情報は型に埋め込み
- UTC との相互変換サポート

### 3.3 ClockFactory

設定に基づいて適切なクロック実装を生成

```csharp
public static IClock CreateClock(IClockSettings settings)
{
    return settings.ClockType?.ToUpperInvariant() switch
    {
        "SYSTEM" => new SystemClock(),
        "TICKING" => CreateTickingClock(settings),
        "OFFSET" => CreateOffsetClock(settings),
        // ...
    };
}
```

---

## 4. クロック実装詳細

### 4.1 SystemClock（本番環境用）

```csharp
public class SystemClock : IClock
{
    public LocalDateTime JstNow
    {
        get
        {
            var utcNow = DateTime.UtcNow;
            var jstNow = TimeZoneInfo.ConvertTime(
                utcNow, 
                TimeZoneInfo.Utc, 
                JstTimeZone);
            return new LocalDateTime(
                DateTime.SpecifyKind(jstNow, DateTimeKind.Unspecified));
        }
    }
}
```

**用途:** 本番環境・実運用  
**特性:**
- リアルタイムシステムクロックから取得
- 常に現在時刻を返す

### 4.2 TickingClock（テスト用・手動制御）

```csharp
public class TickingClock : IClock, IDisposable
{
    // 手動進行
    public void Tick() { _currentTime = _currentTime.Add(_tickInterval); }
    public void Tick(int count) { /* N回進める */ }
    public void Advance(TimeSpan timeSpan) { /* 指定量進める */ }
    
    // 自動進行
    public void StartAutoTick() { /* Timerで自動進行 */ }
    public void StopAutoTick() { /* 停止 */ }
    
    // 直接設定
    public void SetTime(DateTime dateTime) { /* 時刻を設定 */ }
    public void Reset(DateTime newStartTime) { /* リセット */ }
}
```

**用途:** ユニットテスト・統合テスト  
**特性:**
- 指定した開始時刻からスタート
- 手動で Tick() を呼んで時刻を進める
- または Timer で自動進行（テスト実行を高速化）
- 同期化対応（複数スレッドからのアクセス）

**使用例:**

```csharp
// 手動進行モード
var settings = new ClockSettings 
{ 
    ClockType = "Ticking",
    StartTime = "2024-01-01T00:00:00",
    TickIntervalSeconds = 1
};
var clock = (TickingClock)ClockFactory.CreateClock(settings);

// テスト中に手動で時刻を進める
var createdAt = CreatedAt.From(clock.JstNow.Value);
Assert.Equal(new DateTime(2024, 1, 1), createdAt.Value);

clock.Tick();  // 1秒進む
Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 1), clock.JstNow.Value);

// 自動進行モード（高速シミュレーション）
settings.TickIntervalSeconds = 86400;  // 1秒ごとに1日進む
clock.StartAutoTick();
// ... テスト実行 ...
clock.StopAutoTick();
```

### 4.3 OffsetClock（シミュレーション・過去/未来日付）

```csharp
public class OffsetClock : IClock, IDisposable
{
    // 現在のシステム時刻と初期化時のシステム時刻の差分を使用
    // 表示時刻 = オフセット日時 + (現在システム時刻 - 初期システム時刻)
    
    public LocalDateTime JstNow
    {
        get
        {
            var elapsed = DateTime.Now - _systemBaseTime;
            var displayTime = _offsetDateTime.Add(elapsed);
            return new LocalDateTime(
                DateTime.SpecifyKind(displayTime, DateTimeKind.Unspecified));
        }
    }
}
```

**用途:** シミュレーション・過去/未来日付処理の検証  
**特性:**
- 過去の日付（例：2020年1月1日）を設定
- 現在と同じ速度で時刻が進む
- 削除履歴処理、年末処理など特定時期の検証に便利

**使用例:**

```csharp
// 2020年1月1日としてシミュレーション（リアルタイム進行）
var settings = new ClockSettings 
{ 
    ClockType = "Offset",
    OffsetDateTime = "2020-01-01T00:00:00"
};
var clock = ClockFactory.CreateClock(settings);

// 2020年1月1日として動作
var createdAt = CreatedAt.From(clock.JstNow.Value);
Assert.Equal(2020, createdAt.Value.Year);

// 実時間で1秒経過
Thread.Sleep(1000);
// clock.JstNow は 2020年1月1日 00:00:01 を返す
```

### 4.4 MockClock（単体テスト用）

```csharp
public class MockClock : IClock
{
    private DateTime _fixedDateTime;
    
    public LocalDateTime JstNow => new(_fixedDateTime);
    
    // テスト中に変更可能
    public void SetDateTime(DateTime newDateTime) { _fixedDateTime = newDateTime; }
    public void Advance(TimeSpan timeSpan) { _fixedDateTime = _fixedDateTime.Add(timeSpan); }
    public void Reset(DateTime? resetDateTime = null) { /* リセット */ }
}
```

**用途:** 単体テスト・ValueObject のテスト  
**特性:**
- 固定された日時を返す
- テスト中に任意の時刻に変更可能
- Singleton でなく、各テストで新規生成

---

## 5. DI 登録詳細

### 5.1 Infrastructure.DependencyInjection

**ファイル:** `src/Infrastructure/DependencyInjection.cs`

```csharp
public static IServiceCollection AddInfrastructureModels(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ステップ3: クロック設定を登録（IClockSettings）
    var appSettings = configuration
        .GetSection("AppSettings")
        .Get<AppSettings>() 
        ?? throw new InvalidOperationException("AppSettings not found");
    
    services.AddSingleton(appSettings.ClockSettings);
    
    // ステップ4: IClock を Singleton 登録
    var clockSettings = appSettings.ClockSettings 
        ?? throw new InvalidOperationException("ClockSettings not found");
    
    var clockInstance = ClockFactory.CreateClock(clockSettings);
    services.AddSingleton(clockInstance);
    
    return services;
}
```

**重要:**
- **Singleton 登録**: アプリケーション全体で同一インスタンス
- **早期生成**: DI コンテナ構築時に作成（遅延初期化なし）

### 5.2 Presentation.Shared.DependencyInjection での使用

```csharp
// UseCase デコレーターでの使用例
services.AddScoped<IUseCase<TRequest, TResponse>>(provider =>
{
    // ...
    
    // PerformanceDecorator で IClock を注入
    var clock = provider.GetRequiredService<IClock>();
    useCase = new PerformanceDecorator<TRequest, TResponse>(
        useCase, 
        performanceLogger, 
        correlationContext, 
        clock,  // ← ここで注入
        warningThresholdMs);
    
    // ...
});
```

---

## 6. 設定方法

### 6.1 appsettings.json での設定

**ファイル:** `configs/AppSettings/appsettings.json`

```json
{
  "AppSettings": {
    "ClockSettings": {
      "ClockType": "System",           // System | Ticking | Offset
      "StartTime": "2024-01-01T00:00:00",
      "TickIntervalSeconds": 1,
      "OffsetDateTime": null
    }
  }
}
```

### 6.2 設定パターン

**本番環境（System Clock）:**
```json
{
  "ClockType": "System",
  "StartTime": "2024-01-01T00:00:00",
  "TickIntervalSeconds": 1,
  "OffsetDateTime": null
}
```

**テスト手動進行（Ticking Clock）:**
```json
{
  "ClockType": "Ticking",
  "StartTime": "2024-01-01T00:00:00",
  "TickIntervalSeconds": 1,
  "OffsetDateTime": null
}
```

**テスト自動進行（高速シミュレーション）:**
```json
{
  "ClockType": "Ticking",
  "StartTime": "2024-01-01T00:00:00",
  "TickIntervalSeconds": 86400,  // 1秒ごとに1日進む
  "OffsetDateTime": null
}
```

**シミュレーション（Offset Clock）:**
```json
{
  "ClockType": "Offset",
  "StartTime": "2024-01-01T00:00:00",
  "TickIntervalSeconds": 1,
  "OffsetDateTime": "2020-01-01T00:00:00"  // 2020年から開始
}
```

---

## 7. 現在の利用箇所

### 7.1 PerformanceDecorator

実行時間計測に IClock を使用

```csharp
public class PerformanceDecorator<TRequest, TResponse> : IUseCase<TRequest, TResponse>
{
    private readonly IClock _clock;
    
    public async Task<TResponse> ExecuteAsync(TRequest request)
    {
        var startTime = _clock.JstNow;
        // ... 処理実行 ...
        var endTime = _clock.JstNow;
        var elapsed = endTime - startTime;
        
        if (elapsed.TotalMilliseconds > _warningThresholdMs)
        {
            // 警告ログ出力
        }
    }
}
```

---

## 8. CreatedAt/UpdatedAt/DeletedAt との連携

### 8.1 推奨される使用パターン

```csharp
public class Entity
{
    private readonly IClock _clock;  // DI で注入
    
    // Entity 生成時
    public Entity(IClock clock)
    {
        _clock = clock;
        CreatedAt = CreatedAt.From(_clock.JstNow.Value);
        UpdatedAt = UpdatedAt.From(_clock.JstNow.Value);
        DeletedAt = DeletedAt.NotDeleted();
    }
    
    // Entity 更新時
    public void Update()
    {
        UpdatedAt = UpdatedAt.From(_clock.JstNow.Value);
    }
    
    // Entity 論理削除時
    public void SoftDelete()
    {
        if (!DeletedAt.IsDeleted)
        {
            DeletedAt = DeletedAt.From(_clock.JstNow.Value);
        }
    }
}
```

### 8.2 Application層での使用

```csharp
public class CreateEntityUseCase : IUseCase<CreateEntityRequest, CreateEntityResponse>
{
    private readonly IClock _clock;  // DI で注入
    private readonly IRepository _repository;
    
    public async Task<CreateEntityResponse> ExecuteAsync(CreateEntityRequest request)
    {
        var entity = new Entity(_clock)
        {
            Name = request.Name,
            // CreatedAt = CreatedAt.From(_clock.JstNow.Value);  自動生成
        };
        
        await _repository.SaveAsync(entity);
        return new CreateEntityResponse { Id = entity.Id };
    }
}
```

---

## 9. 注意点と制約

### 9.1 DateTime 直接使用は禁止

❌ **禁止:**
```csharp
var now = DateTime.Now;         // ← ローカルタイムゾーン（不確定）
var utcNow = DateTime.UtcNow;  // ← UTC に変換が必要
```

✅ **推奨:**
```csharp
var jstNow = _clock.JstNow;  // ← IClock 経由で JST を取得
var value = jstNow.Value;    // DateTime（Kind = Unspecified）
```

### 9.2 DateTime.Kind について

- **SystemClock**: UTC から JST に変換後、Kind = Unspecified に統一
- **TickingClock**: 直接 Unspecified で管理
- **LocalDateTime**: 常に Kind = Unspecified を強制

**理由:** JST のタイムゾーン情報は LocalDateTime の型に埋め込まれているため、DateTime の Kind は Unspecified に統一

### 9.3 テスト時の注意

```csharp
// ✅ 正しい: MockClock を生成して使用
var mockClock = new MockClock(new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified));
var createdAt = CreatedAt.From(mockClock.JstNow.Value);

// ❌ 誤り: DateTime.Now や DateTime.UtcNow を直接使用
// var createdAt = CreatedAt.From(DateTime.Now);  // タイムゾーン不確定
```

---

## 10. パフォーマンス考慮

### 10.1 SystemClock の呼び出しコスト

- **TimeZoneInfo.ConvertTime()**: ホイール検索（軽量）
- **DateTime.UtcNow**: システムコール（軽量）
- **全体**: マイクロ秒単位で高速

### 10.2 Singleton による最適化

- アプリケーション起動時に1度だけインスタンス化
- 各レイヤーから同一インスタンスを共有
- スレッドセーフ（TickingClock は Lock で同期）

---

## 11. 今後の検討項目

### 11.1 CreatedAt/UpdatedAt/DeletedAt との完全統合

現在の状態:
- ✅ IClock インターフェースが完成
- ✅ 複数のクロック実装（System/Ticking/Offset/Mock）
- ✅ DI Singleton 登録
- ✅ LocalDateTime ValueObject

推奨される統合手段:
- Entity のコンストラクタに IClock を DI
- CreatedAt.From(_clock.JstNow.Value) で生成
- UpdatedAt は変更時に _clock.JstNow.Value で更新
- DeletedAt は削除時に _clock.JstNow.Value で生成

### 11.2 Abstract Base Class での統一

```csharp
public abstract class AggregateRoot
{
    protected readonly IClock Clock;  // protected で子クラスでアクセス可能
    
    public CreatedAt CreatedAt { get; }
    public UpdatedAt UpdatedAt { get; protected set; }
    public DeletedAt DeletedAt { get; protected set; }
    
    protected AggregateRoot(IClock clock)
    {
        Clock = clock;
        CreatedAt = CreatedAt.From(clock.JstNow.Value);
        UpdatedAt = UpdatedAt.From(clock.JstNow.Value);
        DeletedAt = DeletedAt.NotDeleted();
    }
    
    protected void UpdateTimestamp()
    {
        UpdatedAt = UpdatedAt.From(Clock.JstNow.Value);
    }
    
    protected void SoftDelete()
    {
        if (!DeletedAt.IsDeleted)
        {
            DeletedAt = DeletedAt.From(Clock.JstNow.Value);
        }
    }
}
```

---

## 12. まとめ

### 現状評価: ✅ 優秀

| 項目 | 状態 | 備考 |
|---|---|---|
| **インターフェース** | ✅ | IClock で統一 |
| **実装の多様性** | ✅ | System/Ticking/Offset/Mock の4実装 |
| **型安全性** | ✅ | LocalDateTime ValueObject で JST を型化 |
| **DI 統合** | ✅ | Singleton 登録完了 |
| **テスト対応** | ✅ | MockClock/TickingClock で対応 |
| **設定管理** | ✅ | appsettings.json で環境切り替え可能 |
| **ドキュメント** | ⚠️ | appsettings.json に例示あり |
| **ValueObject 統合** | 🔄 | 推奨パターンを実装予定 |

### 推奨アクション

1. **短期**: CreatedAt/UpdatedAt/DeletedAt の生成時に IClock を使用するパターンを確立
2. **中期**: AggregateRoot 基底クラスで IClock 注入と日時フィールドの初期化を統一
3. **長期**: ドメイン全体でシステム唯一クロック経由の日時取得を徹底化

---

## 13. 参考資料

**クロック関連ファイル:**
- `src/Common/Clocks/IClock.cs` - インターフェース定義
- `src/Common/Clocks/SystemClock.cs` - 本番用実装
- `src/Common/Clocks/TickingClock.cs` - テスト用（手動/自動）
- `src/Common/Clocks/OffsetClock.cs` - シミュレーション用
- `src/Common/Clocks/MockClock.cs` - 単体テスト用
- `src/Common/Clocks/LocalDateTime.cs` - JST ValueObject
- `src/Common/Clocks/ClockFactory.cs` - ファクトリ
- `src/Common/Clocks/ClockSettings.cs` - 設定クラス

**設定ファイル:**
- `configs/AppSettings/appsettings.json` - クロック設定例
- `configs/AppSettings/appsettings.Debug.json` - Debug環境設定

**DI 登録:**
- `src/Infrastructure/DependencyInjection.cs` - Singleton 登録
- `src/Presentation/Shared/DependencyInjection.cs` - 使用例（PerformanceDecorator）
