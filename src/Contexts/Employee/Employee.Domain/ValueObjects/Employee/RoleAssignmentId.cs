namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

using System.Collections.Generic;

/// <summary>
/// ロール割り当て識別子を表す ValueObject（GUID ベース）
///
/// 責務：
/// - ロール割り当てを一意に識別
/// - GUID ベースの識別子を型安全に保持
/// - 等価性判定とハッシュコード計算
/// </summary>
public sealed class RoleAssignmentId : PrimitiveValueObject<Guid>, IEquatable<RoleAssignmentId>
{
    /// <summary>
    /// ロール割り当て識別子の値を取得する
    /// </summary>
    public Guid Value => ValueField;

    /// <summary>
    /// 指定された GUID から RoleAssignmentId を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">GUID 値</param>
    private RoleAssignmentId(Guid value) : base(value, true)
    {
    }

    /// <summary>
    /// 新しいロール割り当て識別子を生成する（ファクトリメソッド）
    /// </summary>
    /// <returns>新規生成された RoleAssignmentId インスタンス</returns>
    public static RoleAssignmentId NewId() => new(Guid.NewGuid());

    /// <summary>
    /// 指定された GUID から RoleAssignmentId を生成する
    /// </summary>
    /// <param name="value">GUID 値</param>
    /// <returns>生成された RoleAssignmentId インスタンス</returns>
    /// <exception cref="ArgumentException">GUID が Empty の場合</exception>
    public static RoleAssignmentId From(Guid value) => new(value);

    /// <summary>
    /// 指定された GUID から RoleAssignmentId の生成を試みる（型安全版）
    /// </summary>
    /// <param name="input">GUID 値（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、失敗時 false（例外なし）</returns>
    public static bool TryFrom(Guid? input, out RoleAssignmentId result)
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
    public override bool Equals(object? obj) => Equals(obj as RoleAssignmentId);

    /// <summary>
    /// RoleAssignmentId 間の等価性を判定する
    /// </summary>
    public bool Equals(RoleAssignmentId? other)
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
            throw new ArgumentException("RoleAssignmentId must not be empty.", nameof(normalized));
        }
    }
}
