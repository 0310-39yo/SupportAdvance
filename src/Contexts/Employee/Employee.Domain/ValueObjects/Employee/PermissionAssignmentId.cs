using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 権限割り当て識別子を表す ValueObject（GUID ベース）
///
/// 責務：
/// - 権限割り当てを一意に識別
/// - GUID ベースの識別子を型安全に保持
/// - 等価性判定とハッシュコード計算
/// </summary>
public sealed class PermissionAssignmentId : PrimitiveValueObject<Guid>, IEquatable<PermissionAssignmentId>
{
    /// <summary>
    /// 権限割り当て識別子の値を取得する
    /// </summary>
    public Guid Value => ValueField;

    /// <summary>
    /// 指定された GUID から PermissionAssignmentId を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">GUID 値</param>
    private PermissionAssignmentId(Guid value) : base(value, true)
    {
    }

    /// <summary>
    /// 新しい権限割り当て識別子を生成する（ファクトリメソッド）
    /// </summary>
    /// <returns>新規生成された PermissionAssignmentId インスタンス</returns>
    public static PermissionAssignmentId NewId() => new(Guid.NewGuid());

    /// <summary>
    /// 指定された GUID から PermissionAssignmentId を生成する
    /// </summary>
    /// <param name="value">GUID 値</param>
    /// <returns>生成された PermissionAssignmentId インスタンス</returns>
    /// <exception cref="ArgumentException">GUID が Empty の場合</exception>
    public static PermissionAssignmentId From(Guid value) => new(value);

    /// <summary>
    /// 指定された GUID から PermissionAssignmentId の生成を試みる（型安全版）
    /// </summary>
    /// <param name="input">GUID 値（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、失敗時 false（例外なし）</returns>
    public static bool TryFrom(Guid? input, out PermissionAssignmentId result)
    {
        result = null!;

        if (!input.HasValue || input.Value == Guid.Empty)
        {
            return false;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// オブジェクト等価性を判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as PermissionAssignmentId);

    /// <summary>
    /// PermissionAssignmentId 間の等価性を判定する
    /// </summary>
    public bool Equals(PermissionAssignmentId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString() => Value.ToString("D");

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す
    /// </summary>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// 値を検証する
    /// </summary>
    /// <param name="normalized">検証対象の値</param>
    /// <exception cref="ArgumentException">不正な値</exception>
    public override void Validate(Guid normalized)
    {
        base.Validate(normalized);

        // Empty チェック
        if (normalized == Guid.Empty)
        {
            throw new ArgumentException("PermissionAssignmentId must not be empty.", nameof(normalized));
        }
    }
}
