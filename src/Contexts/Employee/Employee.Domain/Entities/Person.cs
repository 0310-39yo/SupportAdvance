using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Employee.Domain.Entities;

/// <summary>
/// 個人情報エンティティ（従業員の個人基本情報）
/// </summary>
/// <remarks>
/// <para>【ID型】PersonRowId（DB行ID）</para>
/// <para>【責務】個人の氏名・カナ氏名を管理</para>
/// <para>【内包関係】Employee 集約の子 Entity として内包</para>
/// </remarks>
public sealed class Person : Entity<PersonRowId>
{
    /// <summary>
    /// 楽観ロックタイムスタンプ（concurrency control 用）
    /// </summary>
    /// <remarks>
    /// <para>【責務】DB更新時の競合検出</para>
    /// <para>【管理】更新時の Repository による新しい値での上書き</para>
    /// </remarks>
    public byte[] RowVersion { get; internal set; } = [];

    /// <summary>
    /// 姓
    /// </summary>
    public LastName LastName { get; private set; }

    /// <summary>
    /// 名
    /// </summary>
    public FirstName FirstName { get; private set; }

    /// <summary>
    /// 姓（カナ）
    /// </summary>
    public LastNameKana LastNameKana { get; private set; }

    /// <summary>
    /// 名（カナ）
    /// </summary>
    public FirstNameKana FirstNameKana { get; private set; }

    /// <summary>
    /// 指定されたプロパティから Person を生成する（プライベートコンストラクタ）
    /// </summary>
    private Person(
        PersonRowId personRowId,
        LastName lastName,
        FirstName firstName,
        LastNameKana lastNameKana,
        FirstNameKana firstNameKana)
    {
        RowId = personRowId;
        LastName = lastName;
        FirstName = firstName;
        LastNameKana = lastNameKana;
        FirstNameKana = firstNameKana;
    }

    /// <summary>
    /// 新しい Person を生成する（ファクトリメソッド）
    /// </summary>
    /// <param name="personRowId">採番済みの人物の行ID</param>
    /// <param name="lastName">姓</param>
    /// <param name="firstName">名</param>
    /// <param name="lastNameKana">姓（カナ）</param>
    /// <param name="firstNameKana">名（カナ）</param>
    /// <returns>生成した人物（<c>RowVersion</c> は空）</returns>
    public static Person Create(
        PersonRowId personRowId,
        LastName lastName,
        FirstName firstName,
        LastNameKana lastNameKana,
        FirstNameKana firstNameKana) =>
        new(personRowId, lastName, firstName, lastNameKana, firstNameKana);

    /// <summary>
    /// DB から読み込んだ値から Person を復元する（ファクトリメソッド）
    /// </summary>
    /// <param name="personRowId">人物の行ID</param>
    /// <param name="lastName">姓</param>
    /// <param name="firstName">名</param>
    /// <param name="lastNameKana">姓（カナ）</param>
    /// <param name="firstNameKana">名（カナ）</param>
    /// <param name="rowVersion">楽観ロック用の値。<see langword="null"/> の場合は設定なし</param>
    /// <returns>復元した人物</returns>
    /// <remarks>
    /// <para>【責務】DB の プリミティブ型 → Domain Entity に変換</para>
    /// <para>【パラメータ】rowVersion は楽観ロック用（更新時に競合検出）</para>
    /// </remarks>
    public static Person Reconstruct(
        PersonRowId personRowId,
        LastName lastName,
        FirstName firstName,
        LastNameKana lastNameKana,
        FirstNameKana firstNameKana,
        byte[]? rowVersion = null)
    {
        var person = new Person(personRowId, lastName, firstName, lastNameKana, firstNameKana);
        if (rowVersion != null)
        {
            person.RowVersion = rowVersion;
        }

        return person;
    }

    /// <summary>
    /// 氏名の完全な表記を取得する（姓 名）
    /// </summary>
    /// <returns>姓と名を半角スペースでつないだ文字列（例: <c>山田 太郎</c>）</returns>
    public string GetFullName() => $"{LastName.Value} {FirstName.Value}";

    /// <summary>
    /// 氏名のカナ表記を取得する（姓カナ 名カナ）
    /// </summary>
    /// <returns>姓（カナ）と名（カナ）を半角スペースでつないだ文字列</returns>
    public string GetFullNameKana() => $"{LastNameKana.Value} {FirstNameKana.Value}";

    /// <summary>
    /// 姓の更新
    /// </summary>
    /// <param name="lastName">新しい姓</param>
    /// <remarks>
    /// <para>【責務】従業員の姓変更を記録（改名等）</para>
    /// <para>【呼び出し元】Application層の Use Case（例：UpdateEmployeeNameUseCase）</para>
    /// <para>【DB永続化】Employee.Repository.SaveAsync() で集約全体を保存時に反映</para>
    /// </remarks>
    public void UpdateLastName(LastName lastName)
    {
        LastName = lastName;
    }

    /// <summary>
    /// 名の更新
    /// </summary>
    /// <param name="firstName">新しい名</param>
    /// <remarks>
    /// <para>【責務】従業員の名変更を記録（改名等）</para>
    /// <para>【呼び出し元】Application層の Use Case</para>
    /// <para>【DB永続化】Employee.Repository.SaveAsync() で集約全体を保存時に反映</para>
    /// </remarks>
    public void UpdateFirstName(FirstName firstName)
    {
        FirstName = firstName;
    }

    /// <summary>
    /// 姓（カナ）の更新
    /// </summary>
    /// <param name="lastNameKana">新しい姓（カナ）</param>
    /// <remarks>
    /// <para>【責務】従業員の姓カナ変更を記録</para>
    /// <para>【呼び出し元】Application層の Use Case</para>
    /// <para>【DB永続化】Employee.Repository.SaveAsync() で集約全体を保存時に反映</para>
    /// </remarks>
    public void UpdateLastNameKana(LastNameKana lastNameKana)
    {
        LastNameKana = lastNameKana;
    }

    /// <summary>
    /// 名（カナ）の更新
    /// </summary>
    /// <param name="firstNameKana">新しい名（カナ）</param>
    /// <remarks>
    /// <para>【責務】従業員の名カナ変更を記録</para>
    /// <para>【呼び出し元】Application層の Use Case</para>
    /// <para>【DB永続化】Employee.Repository.SaveAsync() で集約全体を保存時に反映</para>
    /// </remarks>
    public void UpdateFirstNameKana(FirstNameKana firstNameKana)
    {
        FirstNameKana = firstNameKana;
    }

    /// <summary>
    /// Person の文字列表現の取得
    /// </summary>
    /// <returns><c>Person(RowId=…, Name=…)</c> 形式のデバッグ用文字列</returns>
    public override string ToString()
        => $"Person(RowId={RowId.Value}, Name={GetFullName()})";
}
