using System.Text.RegularExpressions;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;

/// <summary>
/// ロールコードを表す ValueObject
///
/// 責務：
/// - ロールコードを型安全に保持
/// - 値の検証（1-50文字、英数字と一部特殊文字）
/// - 等価性判定とハッシュコード計算
/// </summary>
public sealed class RoleCode : PrimitiveValueObject<string>, IEquatable<RoleCode>
{
    /// <summary>
    /// ロールコードの値の取得
    /// </summary>
    public string Value => ValueField;

    /// <summary>
    /// 指定されたロールコードから RoleCode を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">ロールコード</param>
    private RoleCode(string value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定されたロールコードからの RoleCode の生成
    /// </summary>
    /// <param name="value">ロールコード（1-50文字、英数字、_、.のみ）</param>
    /// <returns>生成された RoleCode インスタンス</returns>
    /// <exception cref="ArgumentException">値が不正な場合</exception>
    public static RoleCode From(string value) => new(value);

    /// <summary>
    /// 指定されたロールコードから RoleCode の生成を試みる（型安全版）
    /// </summary>
    /// <param name="input">ロールコード（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、失敗時 false（例外なし）</returns>
    public static bool TryFrom(string? input, out RoleCode result)
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
    /// <param name="input">DB から読み込んだロールコード（null許容）</param>
    /// <param name="result">生成された RoleCode インスタンス</param>
    /// <returns>成功時 true、失敗時 false</returns>
    public static bool TryFromDbValue(string? input, out RoleCode result)
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

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RoleCode);

    /// <inheritdoc/>
    public bool Equals(RoleCode? other)
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

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 文字列表現の取得
    /// </summary>
    /// <returns>ロールコードの値そのもの</returns>
    public override string ToString() => Value;

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// 値の検証
    /// </summary>
    /// <param name="normalized">検証対象の値</param>
    /// <exception cref="ArgumentException">不正な値</exception>
    public override void Validate(string normalized)
    {
        base.Validate(normalized);

        // null/空文字列チェック
        if (string.IsNullOrEmpty(normalized))
        {
            throw new ArgumentException("RoleCode must not be null or empty.", nameof(normalized));
        }

        // 長さチェック（1-50文字）
        if (normalized.Length > 50)
        {
            throw new ArgumentException("RoleCode must be 50 characters or less.", nameof(normalized));
        }

        // 文字種チェック（英数字、_、.のみ）
        if (!Regex.IsMatch(normalized, @"^[A-Za-z0-9_.]+$"))
        {
            throw new ArgumentException("RoleCode must contain only alphanumeric characters, underscores, and dots.",
                nameof(normalized));
        }
    }
}
