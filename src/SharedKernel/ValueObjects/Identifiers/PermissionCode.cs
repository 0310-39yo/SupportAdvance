namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// 権限コードを表す ValueObject
///
/// 責務：
/// - 権限コードを型安全に保持
/// - 値の検証（1-100文字、英数字と一部特殊文字）
/// - 等価性判定とハッシュコード計算
/// </summary>
public sealed class PermissionCode : PrimitiveValueObject<string>, IEquatable<PermissionCode>
{
    /// <summary>
    /// 権限コードの値を取得する
    /// </summary>
    public string Value => ValueField;

    /// <summary>
    /// 指定された権限コードから PermissionCode を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">権限コード</param>
    private PermissionCode(string value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された権限コードから PermissionCode を生成する
    /// </summary>
    /// <param name="value">権限コード（1-100文字、英数字、_、.のみ）</param>
    /// <returns>生成された PermissionCode インスタンス</returns>
    /// <exception cref="ArgumentException">値が不正な場合</exception>
    public static PermissionCode From(string value) => new(value);

    /// <summary>
    /// 指定された権限コードから PermissionCode の生成を試みる（型安全版）
    /// </summary>
    /// <param name="input">権限コード（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、失敗時 false（例外なし）</returns>
    public static bool TryFrom(string? input, out PermissionCode result)
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
    /// <param name="input">DB から読み込んだ権限コード（null許容）</param>
    /// <param name="result">生成された PermissionCode インスタンス</param>
    /// <returns>成功時 true、失敗時 false</returns>
    public static bool TryFromDbValue(string? input, out PermissionCode result)
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
    public override bool Equals(object? obj) => Equals(obj as PermissionCode);

    /// <summary>
    /// PermissionCode 間の等価性を判定する
    /// </summary>
    public bool Equals(PermissionCode? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;  // 大文字小文字区別
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

        // null/空文字列チェック
        if (string.IsNullOrEmpty(normalized))
        {
            throw new ArgumentException("PermissionCode must not be null or empty.", nameof(normalized));
        }

        // 長さチェック（1-100文字）
        if (normalized.Length > 100)
        {
            throw new ArgumentException("PermissionCode must be 100 characters or less.", nameof(normalized));
        }

        // 文字種チェック（英数字、_、.のみ）
        if (!Regex.IsMatch(normalized, @"^[A-Za-z0-9_.]+$"))
        {
            throw new ArgumentException("PermissionCode must contain only alphanumeric characters, underscores, and dots.", nameof(normalized));
        }
    }
}
