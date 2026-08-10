namespace SupportAdvance.SharedKernel.ValueObjects;

/// <summary>
/// 選択肢型値オブジェクトの抽象基底クラス
/// 選択肢に対応する内部値を保持し、業務名称（表示名）を管理する。
/// 派生クラスは Validate と GetDisplayName をオーバーライドして
/// 選択肢ごとの検証と名称変換を実装する。
/// IsSet の初期値は protected コンストラクタで制御可能
/// GetEqualityComponents では IsSet と ValueField に基づき等価性を判定
/// </summary>
/// <typeparam name="TValue">選択肢の内部値型（struct 制約）</typeparam>
public abstract class EnumValueObject<TValue> : ValueObject
    where TValue : struct
{
    /// <summary>
    /// 選択肢に対応する内部値
    /// protected readonly で派生クラスからの読み取りを許可し、外部からの変更は禁止
    /// </summary>
    protected readonly TValue ValueField;

    /// <summary>
    /// Unset インスタンス用のコンストラクタ
    /// </summary>
    protected EnumValueObject() : this(default, false)
    {
    }

    /// <summary>
    /// 派生クラスから呼び出すコンストラクタ
    /// 値を設定して IsSet を true で初期化し、Validate を実行する
    /// </summary>
    /// <param name="value">設定する選択肢の内部値</param>
    protected EnumValueObject(TValue value) : this(value, true)
    {
    }

    /// <summary>
    /// 派生クラスから呼び出すコンストラクタ（protected）
    /// 値と IsSet フラグを指定可能
    /// </summary>
    /// <param name="value">設定する選択肢の内部値</param>
    /// <param name="isSet">設定状態フラグ</param>
    protected EnumValueObject(TValue value, bool isSet)
    {
        IsSet = isSet;
        ValueField = isSet ? value : default;
        if (isSet)
        {
            Validate(value);
        }
    }

    /// <summary>
    /// 選択肢の内部値を安全に取得する
    /// IsSet が false の場合は false を返す
    /// </summary>
    /// <param name="value">取得した内部値（out パラメータ）</param>
    /// <returns>取得に成功した場合 true、失敗した場合 false</returns>
    public bool TryGetValue(out TValue value)
    {
        if (!IsSet)
        {
            value = default;
            return false;
        }

        value = ValueField;
        return true;
    }

    /// <summary>
    /// 派生クラスが実装し、選択肢の妥当性を検証する
    /// 無効な value の場合、ArgumentOutOfRangeException をスロー
    /// </summary>
    /// <param name="value">検証対象の選択肢の内部値</param>
    public abstract void Validate(TValue value);

    /// <summary>
    /// 派生クラスが実装し、ValueField に対応する業務名称を返す
    /// switch 式で ValueField → 日本語名を変換
    /// </summary>
    /// <returns>業務名称（表示名）</returns>
    protected abstract string GetDisplayName();

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す（IsSet を除く）
    /// IsSet は ValueObject.GetEqualityComponents で自動的に先頭に付加される
    /// </summary>
    /// <returns>ValueField（IsSet = true の場合）を含むコンポーネント列挙</returns>
    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet)
        {
            yield return ValueField;
        }
    }

    /// <summary>
    /// オブジェクトを文字列で表現（ログ出力、UI 表示）
    /// IsSet が true なら GetDisplayName() の結果を返す
    /// IsSet が false なら "Unset" を返す
    /// </summary>
    /// <returns>文字列表現</returns>
    public override string ToString() => IsSet ? GetDisplayName() : "Unset";
}

