using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ドメインエンティティの基底クラス
///
/// 【責務1】ビジネスオブジェクト（Entity）を表現
/// 【責務2】ドメインイベント発行・管理
/// 【責務3】Entity の等価性判定（ID ベース）
///
/// 【ライフサイクル】
/// - Entity は TId（ID）を持つ不変値オブジェクト
/// - Entity のインスタンスは ID で一意に識別される
/// - 同じ ID を持つ Entity は等価（他のプロパティは無視）
///
/// 【ドメインイベント】
/// - Entity がビジネスロジック実行中に発行
/// - Application層でディスパッチされる
/// - イベント処理後は DomainEvents をクリア（Application層の責任）
/// </summary>
/// <typeparam name="TId">
/// エンティティ ID の型
///
/// 【制約】
/// - RowId を継承する型（DB行ID ベース）
/// - notnull（null 許容不可）
/// - Equals / GetHashCode をオーバーライドしていること
/// </typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull, RowId
{
    /// <summary>
    /// Entity の行識別子（RowId）
    ///
    /// 【アクセス】protected set（派生クラスでのみ設定可能）
    /// 【不変性】設定後の変更は推奨されない（ただし強制しない）
    /// 【用途】等価性判定、集約ルートの識別
    /// 【責務】派生クラスが必ず値を設定する
    /// </summary>
#pragma warning disable CS8618
    public TId RowId { get; protected set; }
#pragma warning restore CS8618

    /// <summary>
    /// 発行されたドメインイベント（内部管理用）
    ///
    /// 【要素】IDomainEvent を実装したイベント
    /// 【順序】発行順序を保持
    /// 【寿命】Application層がイベント処理後にクリア
    /// 【アクセス】DomainEvents プロパティで読み取り専用公開
    /// </summary>
    private protected List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// ドメインイベント取得（読み取り専用）
    ///
    /// 【戻り値】不変リスト（変更不可）
    /// 【用途】Application層でのイベント処理
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// ドメインイベント発行
    ///
    /// 【責務】ドメインロジック実行中にイベントを発行
    /// 【アクセス】protected（派生 Entity クラスでのみ呼び出し可能）
    /// 【実行階層】Domain層のビジネスメソッド内
    /// 【例外】イベントが null の場合 ArgumentNullException をスロー
    ///
    /// 【使用例】
    /// protected void UpdateStatus(Status newStatus, IClock clock)
    /// {
    ///     _status = newStatus;
    ///     this.RaiseDomainEvent(new StatusChangedEvent(
    ///         this.RowId,
    ///         newStatus,
    ///         clock.JstNow
    ///     ));
    /// }
    /// </summary>
    /// <param name="domainEvent">
    /// 発行するドメインイベント
    ///
    /// 【要件】
    /// - IDomainEvent を実装
    /// - OccurredAt に有効な LocalDateTime を含む
    /// - null 不可
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// domainEvent が null の場合
    /// </exception>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// ドメインイベントをクリア
    ///
    /// 【責務】Application層でイベント処理後、イベントリストをクリア
    /// 【アクセス】internal（同じアセンブリ内でのみアクセス可能）
    /// 【呼び出し元】Application層の EventDispatcher など
    /// 【タイミング】すべてのイベント処理完了後
    /// </summary>
    internal void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Entity の等価性判定（ID ベース）
    ///
    /// 【判定基準】ID のみで判定
    /// 【戻り値】同じ TId 値を持つなら true
    /// 【注記】他のプロパティ値は無視
    ///
    /// 【例】
    /// var user1 = new User(userId: "123");
    /// var user2 = new User(userId: "123");
    /// Assert.True(user1.Equals(user2));  // ID が同じなら等価
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    /// <summary>
    /// Entity の等価性判定（型安全版）
    ///
    /// 【判定基準】
    /// 1. 参照が同じなら true
    /// 2. ID が等しいなら true
    /// 3. それ以外は false
    ///
    /// 【null 対応】other が null なら false
    /// </summary>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return RowId.Equals(other.RowId);
    }

    /// <summary>
    /// ハッシュコード取得（RowId ベース）
    ///
    /// 【用途】HashSet, Dictionary など集合型での使用
    /// 【実装】RowId.GetHashCode() をそのまま返す
    /// 【注記】Equals をオーバーライドしたので必ず実装
    /// </summary>
    public override int GetHashCode() => RowId.GetHashCode();

    /// <summary>
    /// Entity の文字列表現
    ///
    /// 【形式】"{ClassName} {{ RowId = {RowId} }}"
    /// 【用途】デバッグ時の表示
    /// </summary>
    public override string ToString() => $"{GetType().Name} {{ RowId = {RowId} }}";
}
