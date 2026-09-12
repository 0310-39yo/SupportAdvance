using System.Text.RegularExpressions;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Department.Domain.ValueObjects;

/// <summary>
/// 部署コードを表す ValueObject
///
/// 責務：
/// - 部署コード（4文字固定）を型安全に保持
/// - 文字種検証（英数字のみ）と長さ検証
/// - 等価性判定とハッシュコード計算
/// </summary>
public sealed class DepartmentCode : PrimitiveValueObject<string>, IEquatable<DepartmentCode>
{
    /// <summary>
    /// 部署コード（4文字）の値を取得する
    /// </summary>
    public string Value => ValueField;

    /// <summary>
    /// 指定された部署コードから DepartmentCode を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">部署コード（4文字）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private DepartmentCode(string value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された部署コードから DepartmentCode を生成する
    /// </summary>
    /// <param name="value">部署コード（4文字、英数字のみ）</param>
    /// <returns>生成された DepartmentCode インスタンス</returns>
    /// <exception cref="ArgumentException">値が不正な場合</exception>
    public static DepartmentCode From(string value) => new(value);

    /// <summary>
    /// 指定された部署コードから DepartmentCode の生成を試みる（型安全版）
    /// </summary>
    /// <param name="input">部署コード（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、失敗時 false（例外なし）</returns>
    public static bool TryFrom(string? input, out DepartmentCode result)
    {
        result = null!;

        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        try
        {
            result = From(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// DB値からの変換
    /// </summary>
    /// <param name="input">DB から読み込んだ部署コード（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、失敗時 false</returns>
    public static bool TryFromDbValue(string? input, out DepartmentCode result)
    {
        result = null!;

        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        try
        {
            result = From(input);
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
    public override bool Equals(object? obj) => Equals(obj as DepartmentCode);

    /// <summary>
    /// DepartmentCode 間の等価性を判定する
    /// </summary>
    public bool Equals(DepartmentCode? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value; // 大文字小文字区別
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString() => Value;

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
    public override void Validate(string normalized)
    {
        base.Validate(normalized);

        // null チェック
        if (string.IsNullOrEmpty(normalized))
        {
            throw new ArgumentException("DepartmentCode must not be null or empty.", nameof(normalized));
        }

        // 長さチェック（固定4文字）
        if (normalized.Length != 4)
        {
            throw new ArgumentException("DepartmentCode must be exactly 4 characters.", nameof(normalized));
        }

        // 文字種チェック（英数字のみ）
        if (!Regex.IsMatch(normalized, @"^[A-Za-z0-9]{4}$"))
        {
            throw new ArgumentException("DepartmentCode must contain only alphanumeric characters.",
                nameof(normalized));
        }
    }
}
