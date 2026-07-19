using System;
using System.Collections.Generic;
using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.SharedKernel.ValueObjects.Audit;

/// <summary>
/// エンティティの論理削除日時を表すValueObject（null許容、IsSet で削除/未削除状態を表現）
/// </summary>
public sealed class DeletedAt : PrimitiveValueObject<DateTime?>, IEquatable<DeletedAt>
{
    /// <summary>
    /// 指定された日時からDeletedAtのインスタンスを生成する
    /// 【責務】指定された日時を持つDeletedAtを表現する
    /// </summary>
    /// <param name="value">日時値</param>
    /// <param name="isSet">IsSet フラグ（デフォルト: true）</param>
    /// <returns>指定された日時を持つDeletedAtのインスタンス</returns>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private DeletedAt(DateTime? value, bool isSet = true) : base(value, isSet)
    {
    }

    /// <summary>
    /// 指定された日時からDeletedAtのインスタンスを生成する（推奨: LocalDateTime で取得）
    /// 【責務】指定された日時を持つDeletedAtを表現する
    /// </summary>
    /// <param name="value">LocalDateTime値（IClock.JstNow から取得）</param>
    /// <returns>指定された日時を持つDeletedAtのインスタンス</returns>
    public static DeletedAt From(LocalDateTime value) => new(value.Value, true);

    /// <summary>
    /// 未削除状態のDeletedAtのインスタンスを生成する
    /// 【責務】未削除状態を表現する
    /// </summary>
    /// <returns>未削除状態のDeletedAtのインスタンス</returns>
    public static DeletedAt Unset() => new(null, isSet: false);

    /// <summary>
    /// 指定された日時からDeletedAtのインスタンスを生成する（層間の型変換用）
    /// null が来た場合は Unset() で変換（成功）
    /// 【責務】null安全に DeletedAt を生成する
    /// </summary>
    /// <param name="input">LocalDateTime? 値（DB からの読み込み値）</param>
    /// <param name="result">生成されたDeletedAtのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(LocalDateTime? input, out DeletedAt result)
    {
        if (input == null || !input.HasValue)
        {
            result = Unset();  // ← null → Unset() で成功
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            result = null!;
            return false;
        }
    }

    /// <summary>
    /// 保持する値を取得する
    /// 【責務】保持する値を取得する（IsSet = true の時のみ有効）
    /// </summary>
    /// <returns>保持する日時値（null 許容）</returns>
    public DateTime? Value => ValueField;

    /// <summary>
    /// 削除済み状態を判定する（IsSet の別名）
    /// 【責務】削除済みか未削除かを判定する
    /// </summary>
    /// <returns>削除済みの場合は true、未削除の場合は false</returns>
    public bool IsDeleted => IsSet;

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// 【責務】指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as DeletedAt);

    /// <summary>
    /// 指定されたDeletedAtと等価かどうかを判定する
    /// 【責務】指定されたDeletedAtと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のDeletedAt</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(DeletedAt? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ValueField == other.ValueField && IsSet == other.IsSet;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// 【責務】オブジェクトのハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => HashCode.Combine(ValueField, IsSet);

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す（IsSet を除く）
    /// IsSet は ValueObject.GetEqualityComponents で自動的に先頭に付加される
    /// </summary>
    /// <returns>ValueField を含むコンポーネント列</returns>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return ValueField;  // DateTime? を返す
    }

    /// <summary>
    /// 正規化済み値の検証を行う
    /// 【責務】業務ルールに基づく値の妥当性のチェック
    /// </summary>
    /// <param name="normalized">正規化済みの値（null 許容）</param>
    /// <exception cref="ArgumentException">値が有効な日時でない場合にスローされる</exception>
    public override void Validate(DateTime? normalized)
    {
        base.Validate(normalized);

        // null は許容（未削除状態を表現）
        if (normalized == null)
            return;

        // DateTime.MinValue や DateTime.MaxValue は除外
        if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
        {
            throw new ArgumentException(
                $"DeletedAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}.");
        }
    }
}
