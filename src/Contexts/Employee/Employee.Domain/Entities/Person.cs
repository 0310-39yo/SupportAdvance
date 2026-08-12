namespace SupportAdvance.Contexts.Employee.Domain.Entities;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;

/// <summary>
/// 個人情報エンティティ（従業員の個人基本情報）
///
/// 【ID型】PersonRowId（DB行ID）
/// 【責務】個人の氏名・カナ氏名を管理
/// 【内包関係】Employee 集約の子 Entity として内包
/// </summary>
public sealed class Person : Entity<PersonRowId>
{
    /// <summary>
    /// 姓
    /// </summary>
    public PersonLastName LastName { get; private set; }

    /// <summary>
    /// 名
    /// </summary>
    public PersonFirstName FirstName { get; private set; }

    /// <summary>
    /// 姓（カナ）
    /// </summary>
    public PersonLastNameKana LastNameKana { get; private set; }

    /// <summary>
    /// 名（カナ）
    /// </summary>
    public PersonFirstNameKana FirstNameKana { get; private set; }

    /// <summary>
    /// 指定されたプロパティから Person を生成する（プライベートコンストラクタ）
    /// </summary>
    private Person(
        PersonRowId personRowId,
        PersonLastName lastName,
        PersonFirstName firstName,
        PersonLastNameKana lastNameKana,
        PersonFirstNameKana firstNameKana)
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
    public static Person Create(
        PersonRowId personRowId,
        PersonLastName lastName,
        PersonFirstName firstName,
        PersonLastNameKana lastNameKana,
        PersonFirstNameKana firstNameKana)
    {
        return new(personRowId, lastName, firstName, lastNameKana, firstNameKana);
    }

    /// <summary>
    /// DB から読み込んだ値から Person を復元する（ファクトリメソッド）
    /// </summary>
    public static Person Reconstruct(
        PersonRowId personRowId,
        PersonLastName lastName,
        PersonFirstName firstName,
        PersonLastNameKana lastNameKana,
        PersonFirstNameKana firstNameKana)
    {
        return new(personRowId, lastName, firstName, lastNameKana, firstNameKana);
    }

    /// <summary>
    /// 氏名の完全な表記を取得する（姓 名）
    /// </summary>
    public string GetFullName() => $"{LastName.Value} {FirstName.Value}";

    /// <summary>
    /// 氏名のカナ表記を取得する（姓カナ 名カナ）
    /// </summary>
    public string GetFullNameKana() => $"{LastNameKana.Value} {FirstNameKana.Value}";

    /// <summary>
    /// 姓を更新する
    /// 【責務】従業員の姓変更を記録（改名等）
    /// 【呼び出し元】Application層の Use Case（例：UpdateEmployeeNameUseCase）
    /// 【DB永続化】Employee.Repository.SaveAsync() で集約全体を保存時に反映
    /// </summary>
    public void UpdateLastName(PersonLastName lastName)
    {
        LastName = lastName;
    }

    /// <summary>
    /// 名を更新する
    /// 【責務】従業員の名変更を記録（改名等）
    /// 【呼び出し元】Application層の Use Case
    /// 【DB永続化】Employee.Repository.SaveAsync() で集約全体を保存時に反映
    /// </summary>
    public void UpdateFirstName(PersonFirstName firstName)
    {
        FirstName = firstName;
    }

    /// <summary>
    /// 姓（カナ）を更新する
    /// 【責務】従業員の姓カナ変更を記録
    /// 【呼び出し元】Application層の Use Case
    /// 【DB永続化】Employee.Repository.SaveAsync() で集約全体を保存時に反映
    /// </summary>
    public void UpdateLastNameKana(PersonLastNameKana lastNameKana)
    {
        LastNameKana = lastNameKana;
    }

    /// <summary>
    /// 名（カナ）を更新する
    /// 【責務】従業員の名カナ変更を記録
    /// 【呼び出し元】Application層の Use Case
    /// 【DB永続化】Employee.Repository.SaveAsync() で集約全体を保存時に反映
    /// </summary>
    public void UpdateFirstNameKana(PersonFirstNameKana firstNameKana)
    {
        FirstNameKana = firstNameKana;
    }

    /// <summary>
    /// Person の文字列表現を取得する
    /// </summary>
    public override string ToString()
        => $"Person(RowId={RowId.Value}, Name={GetFullName()})";
}
