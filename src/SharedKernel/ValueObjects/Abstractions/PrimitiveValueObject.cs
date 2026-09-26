namespace SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// スカラ値を保持する値オブジェクトの抽象基底クラス
/// 値の正規化・検証・等価性・文字列化の基盤の提供
/// 派生クラスは Normalize / Validate / Format のオーバーライドのみで業務固有ロジックの表現
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
    /// <param name="isSet">設定済みかどうかを示す値。未設定（Unset）の場合は <see langword="false"/></param>
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
    /// 保持する値の取得
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
    /// 入力値の正規化（変換）
    /// </summary>
    /// <param name="input">入力値（IsSet=true の場合のみ呼び出される）</param>
    /// <returns>正規化済み値</returns>
    /// <remarks>
    /// <para>【責務】値のビジネスルールに適合した形への変換</para>
    /// <list type="bullet">
    /// <item><description>文字列: トリム、大文字小文字統一、複数空白の単一化など</description></item>
    /// <item><description>数値: 丸め処理、単位変換など</description></item>
    /// <item><description>日時: タイムゾーン統一など</description></item>
    /// </list>
    /// <para>【重要】値の変換のみを担当。検証（有効性チェック）は Validate メソッドで別途実施</para>
    /// <para>【実行タイミング】コンストラクター内での自動実行。isSet=true の場合のみ呼び出し</para>
    /// <para>【実装ガイド】</para>
    /// <list type="bullet">
    /// <item><description>派生クラスで override は optional（不要な場合は除外して OK）</description></item>
    /// <item><description>副作用のない純粋な値変換のみ</description></item>
    /// <item><description>null を返すべきではない（isSet=true の場合を想定）</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// RespondentName（氏名）の場合:
    ///   protected override string Normalize(string input)
    ///       => input.Trim().Replace("　", " ");
    /// </code>
    /// </example>
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
    /// 値の文字列へのフォーマット
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
    /// IsSet は ValueObject.GetEqualityComponents による自動的な先頭への付加
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
    /// 保持する値の文字列化
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
