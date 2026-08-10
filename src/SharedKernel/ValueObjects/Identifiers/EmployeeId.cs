namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// 従業員を一意に識別するビジネスID（Guid ベース）を表すValueObject
/// 【基底型】Guid
/// 【責務】Employee Entity の集約根ID の管理と検証
/// </summary>
public sealed class EmployeeId : PrimitiveValueObject<Guid>, IEquatable<EmployeeId>
{
    /// <summary>
    /// 従業員IDの値を取得する
    /// </summary>
    public Guid Value => ValueField;

    /// <summary>
    /// 指定された Guid 値からEmployeeIdを生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">従業員ID（Guid.Empty 不許可）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private EmployeeId(Guid value) : base(value, true)
    {
    }

    /// <summary>
    /// 新規の従業員IDを生成する（システム採番）
    /// 【責務】一意の従業員IDを採番して返す
    /// </summary>
    /// <returns>新規生成された EmployeeId</returns>
    public static EmployeeId NewId() => From(Guid.NewGuid());

    /// <summary>
    /// 指定された Guid 値からEmployeeIdのインスタンスを生成する（推奨: Domain層での生成方式）
    /// 【責務】Guid値から従業員IDを表現する
    /// </summary>
    /// <param name="value">従業員ID（Guid.Empty 以外）</param>
    /// <returns>指定されたIDのEmployeeIdのインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">Guid.Empty の場合</exception>
    public static EmployeeId From(Guid value) => new(value);

    /// <summary>
    /// 指定された Guid 値からEmployeeIdのインスタンスの生成を試みる（型安全版）
    /// 【責務】null安全に EmployeeId を生成する（Domain層での生成方式）
    /// </summary>
    /// <param name="value">従業員ID</param>
    /// <param name="result">生成されたEmployeeIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(Guid value, out EmployeeId result)
    {
        result = null!;

        try
        {
            result = From(value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 指定された Guid 値からEmployeeIdのインスタンスの生成を試みる（DB値変換版）
    /// 【責務】DB から読み込んだ Guid から EmployeeId を復元
    /// </summary>
    /// <param name="value">DB の employee.employee_id 値</param>
    /// <param name="result">生成されたEmployeeIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFromDbValue(Guid value, out EmployeeId result)
    {
        result = null!;

        try
        {
            result = From(value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 従業員IDの文字列表現を取得する
    /// 【責務】Guid を文字列に変換
    /// </summary>
    /// <returns>Guid の標準文字列表現（例："12345678-1234-1234-1234-123456789012"）</returns>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as EmployeeId);

    /// <summary>
    /// 指定されたEmployeeIdと等価かどうかを判定する
    /// 【責務】指定されたEmployeeIdと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のEmployeeId</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(EmployeeId? other)
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
    /// 【責務】オブジェクトのハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 従業員IDが有効か検証する
    /// </summary>
    /// <param name="normalized">検証対象の値</param>
    /// <exception cref="ArgumentOutOfRangeException">Guid.Empty の場合</exception>
    public override void Validate(Guid normalized)
    {
        // Guid.Empty は許可しない
        if (normalized == Guid.Empty)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                "EmployeeId must not be Guid.Empty.");
        }
    }
}
