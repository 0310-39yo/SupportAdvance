namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Identifiers;

using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;



/// <summary>
/// 部署の階層レベルを表す ValueObject
///
/// 責務：
/// - 階層レベル（0-4）を型安全に保持
/// - 値の検証（0-4 の範囲）
/// - ビジネス意味を名称で表現（Company, Division, Department など）
/// - 等価性判定とハッシュコード計算
/// </summary>
public sealed class HierarchyLevel : EnumValueObject<int>, IEquatable<HierarchyLevel>
{
    /// <summary>
    /// 階層レベルの値を取得する
    /// </summary>
    public int Value => ValueField;

    /// <summary>
    /// 指定された階層レベルから HierarchyLevel を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">階層レベル（0-4）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private HierarchyLevel(int value) : base(value)
    {
    }

    /// <summary>
    /// 指定された階層レベルから HierarchyLevel を生成する
    /// </summary>
    /// <param name="value">階層レベル（0-4）</param>
    /// <returns>生成された HierarchyLevel インスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">0-4 範囲外</exception>
    public static HierarchyLevel From(int value) => new(value);

    /// <summary>
    /// 指定された階層レベルから HierarchyLevel の生成を試みる（型安全版）
    /// </summary>
    /// <param name="value">階層レベル</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、失敗時 false（例外なし）</returns>
    public static bool TryFrom(int value, out HierarchyLevel result)
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
    /// DB値からの変換
    /// </summary>
    /// <param name="value">DB から読み込んだ階層レベル</param>
    /// <param name="result">生成された HierarchyLevel インスタンス</param>
    /// <returns>成功時 true、失敗時 false</returns>
    public static bool TryFromDbValue(int value, out HierarchyLevel result)
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
    /// 会社レベル（0）を生成する
    /// </summary>
    public static HierarchyLevel Company() => From(0);

    /// <summary>
    /// 本部レベル（1）を生成する
    /// </summary>
    public static HierarchyLevel Division() => From(1);

    /// <summary>
    /// 部レベル（2）を生成する
    /// </summary>
    public static HierarchyLevel Department() => From(2);

    /// <summary>
    /// グループレベル（3）を生成する
    /// </summary>
    public static HierarchyLevel Group() => From(3);

    /// <summary>
    /// チームレベル（4）を生成する
    /// </summary>
    public static HierarchyLevel Team() => From(4);

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as HierarchyLevel);

    /// <summary>
    /// 指定された HierarchyLevel と等価かどうかを判定する
    /// </summary>
    public bool Equals(HierarchyLevel? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IsSet == other.IsSet && Value == other.Value;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(IsSet, Value);

    /// <summary>
    /// 階層レベルが有効か検証する
    /// </summary>
    /// <param name="value">検証対象の値</param>
    /// <exception cref="ArgumentOutOfRangeException">0-4 範囲外</exception>
    public override void Validate(int value)
    {
        if (value < 0 || value > 4)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "HierarchyLevel must be between 0 and 4.");
        }
    }

    /// <summary>
    /// 階層レベルの業務名称を取得する
    /// </summary>
    /// <returns>業務名称（Company, Division, Department, Group, Team）</returns>
    protected override string GetDisplayName()
    {
        return Value switch
        {
            0 => "Company",
            1 => "Division",
            2 => "Department",
            3 => "Group",
            4 => "Team",
            _ => "Unknown"
        };
    }
}




