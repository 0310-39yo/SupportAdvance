using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ドメインエンティティの基底クラス
/// </summary>
/// <typeparam name="TId">エンティティ ID の型。<see cref="RowId"/> を継承する型（DB 行ID ベース）で、<see langword="null"/> 不可。<c>Equals</c>／<c>GetHashCode</c> のオーバーライドが必要</typeparam>
/// <remarks>
/// <para>【責務】ビジネスオブジェクト（Entity）の表現、ドメインイベントの発行と管理、Entity の等価性判定（ID ベース）</para>
/// <para>【ライフサイクル】Entity は ID（<typeparamref name="TId"/>）で一意に識別される。同じ ID を持つ Entity は等価で、他のプロパティは比較の対象外</para>
/// <para>【ドメインイベント】Entity がビジネスロジックの実行中に発行し、Application 層がディスパッチする。イベント処理後の <see cref="DomainEvents"/> のクリアは Application 層の責任</para>
/// </remarks>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull, RowId
{
    /// <summary>
    /// Entity の行識別子
    /// </summary>
    /// <value>等価性判定と集約ルートの識別に使用。<c>protected set</c> のため、設定は派生クラスのみ可能</value>
    /// <remarks>
    /// <para>【責務】派生クラスによる、必ずの値の設定</para>
    /// <para>【不変性】設定後の変更は非推奨（ただし強制はしない）</para>
    /// </remarks>
#pragma warning disable CS8618
    public TId RowId { get; protected set; }
#pragma warning restore CS8618

    /// <summary>
    /// 発行されたドメインイベント（内部管理用）
    /// </summary>
    /// <remarks>
    /// <para>【要素】<see cref="IDomainEvent"/> を実装したイベント。発行順序を保持</para>
    /// <para>【寿命】Application 層がイベント処理後にクリア</para>
    /// <para>【アクセス】<see cref="DomainEvents"/> で読み取り専用に公開</para>
    /// </remarks>
    private protected List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// 発行されたドメインイベントの一覧（読み取り専用）
    /// </summary>
    /// <value>変更不可のリスト。Application 層でのイベント処理に使用</value>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// ドメインイベントの発行
    /// </summary>
    /// <param name="domainEvent">発行するドメインイベント。<see cref="IDomainEvent"/> を実装し、<c>OccurredAt</c> に有効な <c>LocalDateTime</c> を持つこと</param>
    /// <exception cref="ArgumentNullException"><paramref name="domainEvent"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【責務】ドメインロジックの実行中のイベントの発行</para>
    /// <para>【アクセス】<c>protected</c>。派生 Entity の Domain 層のビジネスメソッド内から呼び出し</para>
    /// <para>【副作用】<see cref="DomainEvents"/> への追加</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// protected void UpdateStatus(Status newStatus, IClock clock)
    /// {
    ///     _status = newStatus;
    ///     this.RaiseDomainEvent(new StatusChangedEvent(
    ///         this.RowId,
    ///         newStatus,
    ///         clock.JstNow
    ///     ));
    /// }
    /// </code>
    /// </example>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// ドメインイベントのクリア
    /// </summary>
    /// <remarks>
    /// <para>【責務】Application 層でのイベント処理の完了後の、イベントリストのクリア</para>
    /// <para>【アクセス】<c>internal</c>（同じアセンブリ内のみ）</para>
    /// <para>【呼び出し元】Application 層の <c>EventDispatcher</c> など。すべてのイベント処理の完了後</para>
    /// </remarks>
    internal void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    /// <summary>
    /// Entity の等価性の判定（型安全版）
    /// </summary>
    /// <param name="other">比較対象。<see langword="null"/> の場合は <see langword="false"/></param>
    /// <returns>同じインスタンス、または <see cref="Entity{TId}.RowId"/> が等しい場合は <see langword="true"/></returns>
    /// <remarks>
    /// <para>【判定基準】ID のみ。他のプロパティの値は無視</para>
    /// </remarks>
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

    /// <inheritdoc/>
    public override int GetHashCode() => RowId.GetHashCode();

    /// <summary>
    /// デバッグ用の文字列表現の取得
    /// </summary>
    /// <returns><c>{型名} { RowId = … }</c> 形式の文字列。画面表示やデータの解析には使用禁止</returns>
    public override string ToString() => $"{GetType().Name} {{ RowId = {RowId} }}";
}
