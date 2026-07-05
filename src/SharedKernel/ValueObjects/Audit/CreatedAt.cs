using System;
using System.Collections.Generic;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.SharedKernel.ValueObjects.Audit;

/// <summary>
/// エンティティの作成日時を表すValueObject
/// </summary>
public sealed class CreatedAt : PrimitiveValueObject<DateTime>, IEquatable<CreatedAt>
{
    /// <summary>
    /// 指定された日時からCreatedAtのインスタンスを生成する
    /// 【責務】指定された日時を持つCreatedAtを表現する
    /// </summary>
    /// <param name="value">日時値</param>
    /// <returnss>指定された日時を持つCreatedAtのインスタンス</returns>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private CreatedAt(DateTime value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された日時からCreatedAtのインスタンスを生成する
    /// 【責務】指定された日時を持つCreatedAtを表現する
    /// </summary>
    /// <param name="value">日時値</param>
    /// <returns>指定された日時を持つCreatedAtのインスタンス</returns>
    public static CreatedAt From(DateTime value) => new(value);

    /// <summary>
    /// 指定された日時からCreatedAtのインスタンスを生成する
    /// 【責務】指定された日時を持つCreatedAtを表現する
    /// </summary>
    /// <param name="input">日時値</param>
    /// <param name="result">生成されたCreatedAtのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(DateTime? input, out CreatedAt result)
    {
        // null の場合は失敗
        if (!input.HasValue)
        {
            result = null!;
            return false;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            // 入力値が不正な場合は失敗
            result = null!;
            return false;
        }
    }

    /// <summary>
    /// 指定された日時からCreatedAtのインスタンスを生成する
    /// 【責務】指定された日時を持つCreatedAtを表現する
    /// </summary>
    /// <param name="input">日時値</param>
    /// <param name="result">生成されたCreatedAtのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(DateTime input, out CreatedAt result) => TryFrom((DateTime?)input, out result);

    /// <summary>
    /// 保持する値を取得する
    /// 【責務】保持する値を取得する
    /// </summary>
    /// <returns>保持する日時値</returns>
    public DateTime Value => ValueField;

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// 【責務】指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as CreatedAt);

    /// <summary>
    /// 指定されたCreatedAtと等価かどうかを判定する
    /// 【責務】指定されたCreatedAtと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のCreatedAt</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(CreatedAt? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ValueField == other.ValueField;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// 【責務】オブジェクトのハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => ValueField.GetHashCode();

    /// <summary>
    /// 等価性の比較に使用するコンポーネントを取得する
    /// 【責務】ValueFieldを返すことで、等価性の比較に使用するコンポーネントを提供する
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ValueField;
    }

    /// <summary>
    /// 正規化済み値の検証を行う
    /// 【責務】業務ルールに基づく値の妥当性のチェック
    /// </summary>
    /// <param name="normalized">正規化済みの値</param>
    /// <exception cref="ArgumentException">値が有効な日時でない場合にスローされる</exception>
    protected override void Validate(DateTime normalized)
    {
        base.Validate(normalized);

        // DateTime.MinValue や DateTime.MaxValue は除外
        if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
        {
            throw new ArgumentException($"CreatedAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}.");
        }
    }
}
