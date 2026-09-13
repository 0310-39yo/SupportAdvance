using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Identity.Domain.Entities;

/// <summary>
/// Identity 集約（認証情報管理）
///
/// 【責務】
/// - ユーザー認証情報の管理（ログインID、認証方式、EmployeeRowId への紐づけ）
/// - 監査情報の保持（CreatedAt, UpdatedAt, DeletedAt など）
/// - 認証ロジックは Application層 UseCase で実行（Domain層は状態のみ保持）
///
/// 【特徴】
/// - 認証のみ責務（認可は分離）
/// - Windows AD / ローカル認証の両対応
/// - 論理削除対応（削除はsoftdelete、deleted_atで管理）
///
/// 【不変性】
/// 生成後、LoginId / AuthMethod / EmployeeRowId の変更不可
/// （更新が必要な場合は Delete して新規作成）
/// </summary>
public sealed class Identity : AggregateRoot<IdentityRowId>
{
    /// <summary>
    /// 従業員RowId（m_employees.row_id）
    /// 【特徴】Identity が紐づいた従業員を示す外部参照
    /// 【制約】NOT NULL、FK → m_employees(row_id)
    /// </summary>
    public EmployeeRowId EmployeeRowId { get; private set; }

    /// <summary>
    /// ログインID（従業員番号など）
    /// 【特徴】ユーザーが入力するログイン用ID
    /// 【不変性】生成後変更不可
    /// </summary>
    public LoginId LoginId { get; private set; }

    /// <summary>
    /// 認証方式（LocalAuth / WindowsAD）
    /// 【特徴】ログイン時の認証方式を示す
    /// 【不変性】生成後変更不可
    /// </summary>
    public AuthMethod AuthMethod { get; private set; }

    /// <summary>
    /// 監査情報：作成日時
    /// 【型】LocalDateTime（JST）
    /// 【不変性】生成後変更不可
    /// </summary>
    public CreatedAt CreatedAt { get; private set; }

    /// <summary>
    /// 監査情報：作成者（m_persons.row_id）
    /// 【型】EmployeeRowId
    /// 【特徴】誰が認証情報を作成したか
    /// </summary>
    public EmployeeRowId CreatedBy { get; private set; }

    /// <summary>
    /// 監査情報：更新日時
    /// 【型】LocalDateTime（JST）または未設定
    /// 【特徴】最終更新日時。未更新時は Unset()
    /// </summary>
    public UpdatedAt UpdatedAt { get; private set; }

    /// <summary>
    /// 監査情報：更新者（m_persons.row_id）
    /// 【型】EmployeeRowId または未設定
    /// 【特徴】最後に更新した人。未更新時は Unset()
    /// </summary>
    public EmployeeRowId UpdatedBy { get; private set; }

    /// <summary>
    /// 監査情報：削除日時
    /// 【型】LocalDateTime（JST）または未設定
    /// 【特徴】論理削除用。削除されていない場合は Unset()
    /// </summary>
    public DeletedAt DeletedAt { get; private set; }

    /// <summary>
    /// 監査情報：削除者（m_persons.row_id）
    /// 【型】EmployeeRowId または未設定
    /// 【特徴】誰が削除したか。未削除時は Unset()
    /// </summary>
    public EmployeeRowId DeletedBy { get; private set; }

    /// <summary>
    /// 楽観ロックタイムスタンプ
    /// 【用途】Repository が UpdateAsync 時に row_version で競合検出
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    private Identity(
        IdentityRowId id,
        EmployeeRowId employeeRowId,
        LoginId loginId,
        AuthMethod authMethod,
        CreatedAt createdAt,
        EmployeeRowId createdBy,
        UpdatedAt updatedAt,
        EmployeeRowId updatedBy,
        DeletedAt deletedAt,
        EmployeeRowId deletedBy)
    {
        RowId = id ?? throw new ArgumentNullException(nameof(id));
        EmployeeRowId = employeeRowId ?? throw new ArgumentNullException(nameof(employeeRowId));
        LoginId = loginId ?? throw new ArgumentNullException(nameof(loginId));
        AuthMethod = authMethod ?? throw new ArgumentNullException(nameof(authMethod));
        CreatedAt = createdAt ?? throw new ArgumentNullException(nameof(createdAt));
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        UpdatedAt = updatedAt ?? throw new ArgumentNullException(nameof(updatedAt));
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
        DeletedAt = deletedAt ?? throw new ArgumentNullException(nameof(deletedAt));
        DeletedBy = deletedBy ?? throw new ArgumentNullException(nameof(deletedBy));
    }

    /// <summary>
    /// Identity を新規作成
    /// </summary>
    public static Identity Create(
        IdentityRowId id,
        EmployeeRowId employeeRowId,
        LoginId loginId,
        AuthMethod authMethod,
        CreatedAt createdAt,
        EmployeeRowId createdBy)
    {
        return new Identity(
            id,
            employeeRowId,
            loginId,
            authMethod,
            createdAt,
            createdBy,
            UpdatedAt.Unset(),
            EmployeeRowId.Unset(),
            DeletedAt.Unset(),
            EmployeeRowId.Unset());
    }

    /// <summary>
    /// DBから復元（全フィールド指定）
    /// 【用途】Repository が DbModel から Domain Entity を構築時に使用
    /// </summary>
    public static Identity Reconstruct(
        IdentityRowId id,
        EmployeeRowId employeeRowId,
        LoginId loginId,
        AuthMethod authMethod,
        CreatedAt createdAt,
        EmployeeRowId createdBy,
        UpdatedAt updatedAt,
        EmployeeRowId updatedBy,
        DeletedAt deletedAt,
        EmployeeRowId deletedBy)
    {
        return new Identity(
            id,
            employeeRowId,
            loginId,
            authMethod,
            createdAt,
            createdBy,
            updatedAt,
            updatedBy,
            deletedAt,
            deletedBy);
    }

    /// <summary>
    /// Identity が有効（削除されていない）か判定
    /// </summary>
    public bool IsActive() => !DeletedAt.IsDeleted;
}
