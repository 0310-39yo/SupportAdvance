namespace SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// スカラ値を保持する値オブジェクトの抽象基底クラス
/// 値の正規化・検証・等価性・文字列化の基盤を提供する
/// 派生クラスは Normalize / Validate / Format のオーバーライドのみで業務固有ロジックを表現する
/// </summary>
/// <typeparam name="TValue">保持する値の型</typeparam>
public abstract class PrimitiveValueObject<TValue> : ValueObject
{
    /// <summary>
    /// 保持する値 IsSet = trueの場合のみ有効
    /// </summary>
    protected readonly TValue ValueField;

    /// <summary>
    /// UnSet インスタンス用のコンストラクタ
    /// </summary>
    /// <param name="isSet"></param>
    protected PrimitiveValueObject(bool isSet)
    {
        IsSet = isSet;
        ValueField = default!;
    }

    /// <summary>
    /// 値を保持するインスタンス用のコンストラクタ
    /// </summary>
    /// <param name="value">初期値</param>
    /// <param name="isSet">IsSet の値。通常は true</param>
    protected PrimitiveValueObject(TValue value, bool isSet)
    {
        IsSet = isSet;
        ValueField = isSet ? Normalize(value) : default!;
        if (isSet)
        {
            Validate(ValueField);
        }
    }

    /// <summary>
    /// 保持する値を取得する
    /// </summary>
    /// <param name="value">保持する値。IsSet = false の場合は default</param>
    /// <returns>IsSet = true の場合は true、そうでない場合は false</returns>
    public bool TryGetValue(out TValue value)
    {
        if (!IsSet)
        {
            value = default!;
            return false;
        }

        value = ValueField;
        return true;
    }

    /// <summary>
    /// 入力値を正規化（変換）します。
    ///
    /// 【責務】
    /// 値をビジネスルールに適合した形に変換します。
    ///   - 文字列: トリム、大文字小文字統一、複数空白の単一化など
    ///   - 数値: 丸め処理、単位変換など
    ///   - 日時: タイムゾーン統一など
    ///
    /// 【重要】値の変換のみを行います
    /// 検証（有効性チェック）は Validate メソッドで別途実施されます。
    ///
    /// 【実行タイミング】
    /// コンストラクタ内で自動実行されます。
    /// isSet=true の場合のみ呼び出されます。
    ///
    /// 【実装ガイド】
    /// - 派生クラスで override は optional（不要な場合は除外して OK）
    /// - 副作用のない純粋な値変換のみ
    /// - null を返すべきではない（isSet=true の場合を想定）
    ///
    /// 【実装例】
    /// RespondentName（氏名）の場合:
    ///   protected override string Normalize(string input)
    ///       => input.Trim().Replace("　", " ");
    /// </summary>
    /// <param name="input">入力値（IsSet=true の場合のみ呼び出される）</param>
    /// <returns>正規化済み値</returns>
    protected virtual TValue Normalize(TValue input) => input;

    /// <summary>
    /// 正規化済み値の検証を行う
    /// </summary>
    /// <param name="normalized">正規化済み値</param>
    /// <remarks>
    /// デフォルト実装は何も行っていない
    /// 派生クラスはビジネスルールに従って例外をスローさせる
    /// </remarks>
    public virtual void Validate(TValue normalized)
    {
    }

    /// <summary>
    /// 値を文字列にフォーマットする
    /// </summary>
    /// <param name="value">対象値</param>
    /// <returns>フォーマット済み文字列</returns>
    /// <remarks>
    /// デフォルト実装は value?.ToString() ?? string.Empty を返す
    /// ToString() から呼び出される拡張
    /// </remarks>
    protected virtual string Format(TValue value) => value?.ToString() ?? string.Empty;

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す（IsSet を除く）
    /// IsSet は ValueObject.GetEqualityComponents で自動的に先頭に付加される
    /// </summary>
    /// <returns>ValueField（IsSet = true の場合）を含むコンポーネント列</returns>
    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet)
        {
            yield return ValueField;
        }
    }

    /// <summary>
    /// 保持する値を文字列化する
    /// </summary>
    /// <returns>IsSet = false の場合は "Unset"、IsSet = true の場合は Format(ValueField) の結果</returns>
    public override string ToString()
    {
        if (!IsSet)
        {
            return "Unset";
        }

        return Format(ValueField);
    }
}
