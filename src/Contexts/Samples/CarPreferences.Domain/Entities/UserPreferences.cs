using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;

/// <summary>
/// ユーザーの自動車関連の好みを管理する AggregateRoot
///
/// 【責務】
/// - ユーザーの好み情報を保持
/// - 好み変更時にドメインイベント発行
/// - ビジネスルール検証
///
/// 【ライフサイクル】ユーザー登録～退会まで
///
/// 【識別子設計】
/// - Entity.Id: UserPreferencesId（集約のビジネスID、GUID、型安全）
/// - RowId: テーブルの物理キー（プライベート属性、DB主キー、SQL Serverシーケンス）
/// - UserId: ユーザーID（ビジネス識別子、1000～9999）
/// </summary>
public class UserPreferences : AggregateRoot<UserPreferencesId>
{
    /// <summary>DB テーブル行を一意識別（プライベート属性）</summary>
    private RowId _rowId = null!;
    /// <summary>ユーザーID（ビジネス識別子、1000～9999）</summary>
    private RespondentPersonId _userId = null!;

    /// <summary>希望車種</summary>
    private CarModel? _preferredModel;

    /// <summary>希望ボディタイプ</summary>
    private BodyType? _preferredBodyType;

    /// <summary>オートマ希望フラグ（true: オートマ, false: マニュアル）</summary>
    private bool _prefersAutomatic = true;

    /// <summary>予算下限</summary>
    private Money? _budgetFrom;

    /// <summary>予算上限</summary>
    private Money? _budgetTo;

    /// <summary>ユーザーが質問に回答した日時</summary>
    private RespondentAt _respondedAt = RespondentAt.Unset();

    /// <summary>作成日時（ValueObject）</summary>
    private CreatedAt _createdAt = null!;

    /// <summary>最終更新日時（ValueObject、未更新状態対応）</summary>
    private UpdatedAt _updatedAt = null!;

    /// <summary>削除日時（ValueObject、論理削除対応）</summary>
    private DeletedAt _deletedAt = null!;

    // 公開プロパティ（読み取り専用）

    /// <summary>DB テーブル行の物理キー</summary>
    public RowId RowId => _rowId;

    /// <summary>ユーザーID（ビジネス識別子）</summary>
    public RespondentPersonId UserId => _userId;

    /// <summary>希望車種</summary>
    public CarModel? PreferredModel => _preferredModel;

    /// <summary>希望ボディタイプ</summary>
    public BodyType? PreferredBodyType => _preferredBodyType;

    /// <summary>オートマ希望フラグ</summary>
    public bool PrefersAutomatic => _prefersAutomatic;

    /// <summary>予算下限</summary>
    public Money? BudgetFrom => _budgetFrom;

    /// <summary>予算上限</summary>
    public Money? BudgetTo => _budgetTo;

    /// <summary>回答日時</summary>
    public RespondentAt RespondedAt => _respondedAt;

    /// <summary>作成日時（ValueObject）</summary>
    public CreatedAt CreatedAt => _createdAt;

    /// <summary>最終更新日時（ValueObject）</summary>
    public UpdatedAt UpdatedAt => _updatedAt;

    /// <summary>削除日時（ValueObject）</summary>
    public DeletedAt DeletedAt => _deletedAt;

    /// <summary>
    /// UserPreferences を生成（新規作成用）
    ///
    /// 【責務】Entity の初期化
    /// 【新規作成】id と rowId は自動生成
    /// 【既存読み込み】Reconstruct() を使用
    /// </summary>
    /// <param name="userId">ユーザーID（ビジネス識別子）</param>
    /// <param name="respondedAt">回答日時</param>
    /// <param name="clock">現在時刻取得用</param>
    public UserPreferences(
        RespondentPersonId userId,
        RespondentAt respondedAt,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(respondedAt);
        ArgumentNullException.ThrowIfNull(clock);

        Id = UserPreferencesId.New();  // 集約ID を新規生成
        _rowId = RowId.New();  // DB物理キーを新規生成（value=0）
        _userId = userId;
        _respondedAt = respondedAt;
        _createdAt = CreatedAt.From(clock.JstNow);
        _updatedAt = UpdatedAt.From(clock.JstNow);
        _deletedAt = DeletedAt.Unset();
        _prefersAutomatic = true;  // デフォルト値
    }

    /// <summary>
    /// DB から復元した UserPreferences を再構築
    ///
    /// 【責務】Mapper/Repository が DB 読み込み値を Domain Entity に変換
    /// 【用途】MapToDomain() → Reconstruct() の流れで使用
    /// </summary>
    public static UserPreferences Reconstruct(
        UserPreferencesId id,
        RespondentPersonId userId,
        RespondentAt respondedAt,
        CreatedAt createdAt,
        UpdatedAt updatedAt,
        DeletedAt deletedAt,
        RowId rowId,
        CarModel? preferredModel = null,
        BodyType? preferredBodyType = null,
        bool prefersAutomatic = true,
        Money? budgetFrom = null,
        Money? budgetTo = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(respondedAt);
        ArgumentNullException.ThrowIfNull(createdAt);
        ArgumentNullException.ThrowIfNull(updatedAt);
        ArgumentNullException.ThrowIfNull(deletedAt);
        ArgumentNullException.ThrowIfNull(rowId);

        // ダミーの Clock で一時的に Entity を構築（フィールドはすぐ上書き）
        var entity = new UserPreferences(userId, respondedAt, new SystemClock());

        // DB値で ID と監査フィールドと ビジネスプロパティを上書き
        entity.Id = id;
        entity._rowId = rowId;
        entity._createdAt = createdAt;
        entity._updatedAt = updatedAt;
        entity._deletedAt = deletedAt;
        entity._preferredModel = preferredModel;
        entity._preferredBodyType = preferredBodyType;
        entity._prefersAutomatic = prefersAutomatic;
        entity._budgetFrom = budgetFrom;
        entity._budgetTo = budgetTo;

        return entity;
    }

    /// <summary>
    /// 希望車種を更新してイベント発行
    /// </summary>
    public void UpdatePreferredModel(CarModel model, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(clock);

        var oldModel = _preferredModel;
        _preferredModel = model;
        _updatedAt = UpdatedAt.From(clock.JstNow);

        this.RaiseDomainEvent(new PreferencesUpdatedEvent(
            DomainEventId.New(),  // イベント ID（GUID）
            PreferenceChangeType.ModelUpdated,
            oldModel?.ToString() ?? "未設定",
            model.ToString(),
            _updatedAt.Value!.Value));  // LocalDateTime? から LocalDateTime を抽出
    }

    /// <summary>
    /// 予算範囲を更新してイベント発行
    ///
    /// 【ビジネスルール】from != null && to != null の場合、from <= to を検証
    /// </summary>
    public void UpdateBudget(Money? from, Money? to, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (from != null && to != null && from > to)
        {
            throw new ArgumentException(
                "BudgetFrom must be less than or equal to BudgetTo",
                nameof(from));
        }

        var oldFrom = _budgetFrom;
        var oldTo = _budgetTo;

        _budgetFrom = from;
        _budgetTo = to;
        _updatedAt = UpdatedAt.From(clock.JstNow);

        this.RaiseDomainEvent(new BudgetUpdatedEvent(
            DomainEventId.New(),  // イベント ID（GUID）
            oldFrom, oldTo,
            from, to,
            _updatedAt.Value!.Value));  // LocalDateTime? から LocalDateTime を抽出
    }

    /// <summary>
    /// ボディタイプを更新してイベント発行
    /// </summary>
    public void UpdateBodyType(BodyType bodyType, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(bodyType);
        ArgumentNullException.ThrowIfNull(clock);

        if (_preferredBodyType == bodyType)
        {
            return;  // 変更なし
        }

        var oldBodyType = _preferredBodyType;
        _preferredBodyType = bodyType;
        _updatedAt = UpdatedAt.From(clock.JstNow);

        this.RaiseDomainEvent(new BodyTypeUpdatedEvent(
            DomainEventId.New(),  // イベント ID（GUID）
            oldBodyType,
            bodyType,
            _updatedAt.Value!.Value));  // LocalDateTime? から LocalDateTime を抽出
    }

    /// <summary>
    /// トランスミッション希望を更新してイベント発行
    /// </summary>
    public void UpdateTransmissionPreference(bool prefersAutomatic, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (_prefersAutomatic == prefersAutomatic)
        {
            return;  // 変更なし
        }

        var oldPreference = _prefersAutomatic;
        _prefersAutomatic = prefersAutomatic;
        _updatedAt = UpdatedAt.From(clock.JstNow);

        this.RaiseDomainEvent(new TransmissionPreferenceUpdatedEvent(
            DomainEventId.New(),  // イベント ID（GUID）
            oldPreference,
            prefersAutomatic,
            _updatedAt.Value!.Value));  // LocalDateTime? から LocalDateTime を抽出
    }

    /// <summary>
    /// 複数の好みを一括更新
    ///
    /// 【責務】各プロパティが値を持つ場合のみ更新
    /// </summary>
    public void UpdatePreferences(
        CarModel? model,
        BodyType? bodyType,
        bool? prefersAutomatic,
        Money? budgetFrom,
        Money? budgetTo,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (model != null)
        {
            UpdatePreferredModel(model, clock);
        }

        if (bodyType != null)
        {
            UpdateBodyType(bodyType, clock);
        }

        if (prefersAutomatic.HasValue && prefersAutomatic != _prefersAutomatic)
        {
            UpdateTransmissionPreference(prefersAutomatic.Value, clock);
        }

        if (budgetFrom != null || budgetTo != null)
        {
            UpdateBudget(budgetFrom, budgetTo, clock);
        }
    }

    /// <summary>
    /// ユーザーの好みを論理削除（DeletedAt を設定）
    ///
    /// 【責務】Domain層での削除状態設定
    /// 【戻り値】削除成功時 true、既に削除済みなら false
    /// </summary>
    public bool SoftDelete(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (_deletedAt.IsDeleted)
        {
            return false;
        }

        _deletedAt = DeletedAt.From(clock.JstNow);
        _updatedAt = UpdatedAt.From(clock.JstNow);

        return true;
    }

    /// <summary>
    /// 削除状態を判定
    /// </summary>
    public bool IsDeleted => _deletedAt.IsDeleted;

    /// <summary>
    /// RowId を更新（Repository 用、DB採番後に呼び出される）
    ///
    /// 【責務】DB 採番後の RowId（value>0）を Entity に反映
    /// 【用途】Repository.AddAsync で新規作成直後に呼び出し
    /// 【設計】public だが、Repository のみが呼び出すべき
    /// </summary>
    public void SetRowId(RowId rowId)
    {
        ArgumentNullException.ThrowIfNull(rowId);
        _rowId = rowId;
    }
}
