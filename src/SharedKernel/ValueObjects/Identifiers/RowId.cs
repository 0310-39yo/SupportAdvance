using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// データベース行を一意に識別する主キー値を表す抽象基底クラス
///
/// 【責務】
/// - long 型の行ID を型安全に管理
/// - 派生クラスごとに MinValue（最小有効値）を定義
/// - 派生クラスごとに Validate（検証ルール）を実装
///
/// 【継承パターン】
/// - 必須型（MinValue 大于等于 1）: PersonRowId, DepartmentRowId, EmployeeRowId
/// - オプション型（IsSetフラグで未設定表現）: ManagerEmployeeRowId, ParentDepartmentRowId
///
/// 【使用例】
/// public sealed class PersonRowId : RowId
/// {
///     public const long MinValue = 1L;
///
///     private PersonRowId(long value) : base(value, true) { }
///
///     public static PersonRowId From(long value) => new(value);
///
///     public override void Validate(long normalized)
///     {
///         if (normalized &lt; MinValue)
///             throw new ArgumentOutOfRangeException(...);
///     }
/// }
/// </summary>
public abstract class RowId : PrimitiveValueObject<long>, IEquatable<RowId>
{
    /// <summary>
    /// コンストラクタ（派生クラスからのみ呼び出し可能）
    /// </summary>
    /// <param name="value">行ID値</param>
    /// <param name="isSet">IsSet の値。通常は true</param>
    protected RowId(long value, bool isSet) : base(value, isSet)
    {
    }

    /// <summary>
    /// 未設定状態を表すコンストラクタ（オプション型用）
    /// </summary>
    /// <param name="isSet">false</param>
    protected RowId(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 保持する値を取得する
    /// </summary>
    public long Value => ValueField;

    /// <summary>
    /// 行ID値の検証を行う（派生クラスで実装）
    ///
    /// 【実装例】
    /// public override void Validate(long normalized)
    /// {
    ///     if (normalized < MinValue)
    ///         throw new ArgumentOutOfRangeException(...);
    /// }
    /// </summary>
    /// <param name="normalized">検証対象の値</param>
    public abstract override void Validate(long normalized);

    /// <summary>
    /// 派生クラスで From() static メソッドを実装してください
    /// 【実装例】
    /// public static PersonRowId From(long value) => new PersonRowId(value);
    /// </summary>

    /// <summary>
    /// 文字列表現を返す
    /// </summary>
    public override string ToString() => ValueField.ToString();

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as RowId);

    /// <summary>
    /// 指定された RowId と等価かどうかを判定する
    /// </summary>
    public bool Equals(RowId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() && Value == other.Value;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    public override int GetHashCode() => ValueField.GetHashCode();
}
