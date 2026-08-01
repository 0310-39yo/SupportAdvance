using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;
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
/// - Entity.Id: RowId（row_id ValueObject、DB主キー、SQL Serverシーケンス）
/// - UserId: ユーザーID（ビジネス識別子、1000～9999）
/// </summary>
public class UserPreferences : AggregateRoot<RowId>
{
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

    /// <summary>最終更新日時</summary>
    private LocalDateTime _updatedAt;

    /// <summary>作成日時</summary>
    private LocalDateTime _createdAt;

    // 公開プロパティ（読み取り専用）

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

    /// <summary>最終更新日時</summary>
    public LocalDateTime UpdatedAt => _updatedAt;

    /// <summary>作成日時</summary>
    public LocalDateTime CreatedAt => _createdAt;

    /// <summary>
    /// UserPreferences を生成
    ///
    /// 【責務】Entity の初期化
    /// 【新規作成】rowId = RowId.New()（value=0、DB挿入後に採番される）
    /// 【既存読み込み】rowId を指定
    /// </summary>
    /// <param name="userId">ユーザーID（ビジネス識別子）</param>
    /// <param name="respondedAt">回答日時</param>
    /// <param name="clock">現在時刻取得用</param>
    /// <param name="rowId">row_id ValueObject（DB主キー、デフォルト=RowId.New()）</param>
    public UserPreferences(
        RespondentPersonId userId,
        RespondentAt respondedAt,
        IClock clock,
        RowId? rowId = null)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(respondedAt);
        ArgumentNullException.ThrowIfNull(clock);

        Id = rowId ?? RowId.New();
        _userId = userId;
        _respondedAt = respondedAt;
        _createdAt = clock.JstNow;
        _updatedAt = clock.JstNow;
        _prefersAutomatic = true;  // デフォルト値
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
        _updatedAt = clock.JstNow;

        this.RaiseDomainEvent(new PreferencesUpdatedEvent(
            this.Id,  // RowId（システム基本ID）
            PreferenceChangeType.ModelUpdated,
            oldModel?.ToString() ?? "未設定",
            model.ToString(),
            clock.JstNow));
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
        _updatedAt = clock.JstNow;

        this.RaiseDomainEvent(new BudgetUpdatedEvent(
            this.Id,  // RowId（システム基本ID）
            oldFrom, oldTo,
            from, to,
            clock.JstNow));
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
        _updatedAt = clock.JstNow;

        this.RaiseDomainEvent(new BodyTypeUpdatedEvent(
            this.Id,  // RowId（システム基本ID）
            oldBodyType,
            bodyType,
            clock.JstNow));
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
        _updatedAt = clock.JstNow;

        this.RaiseDomainEvent(new TransmissionPreferenceUpdatedEvent(
            this.Id,  // RowId（システム基本ID）
            oldPreference,
            prefersAutomatic,
            clock.JstNow));
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
}
