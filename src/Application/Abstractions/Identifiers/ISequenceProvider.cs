namespace SupportAdvance.Application.Abstractions.Identifiers;

/// <summary>
/// DB シーケンスから連続した RowId を採番するインターフェース
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>dbo.s_row_id_sequence から次の RowId を取得</description></item>
/// <item><description>単一値および複数値の採番をサポート</description></item>
/// <item><description>全 Bounded Context で共用可能な汎用インターフェース</description></item>
/// </list>
/// <para>【実装】</para>
/// <list type="bullet">
/// <item><description>Infrastructure層 で SqlConnection を使用した SQL実行</description></item>
/// <item><description>スレッドセーフな実装を保証</description></item>
/// </list>
/// <para>【使用箇所】</para>
/// <list type="bullet">
/// <item><description>Employee Context: CreateEmployeeUseCase 他</description></item>
/// <item><description>Authentication Context: 将来実装時に使用予定</description></item>
/// <item><description>Master Context: 将来実装時に使用予定</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var rowId = await sequenceProvider.GetNextValueAsync();
/// var rowIds = await sequenceProvider.GetNextValuesAsync(count: 5);
/// </code>
/// </example>
public interface ISequenceProvider
{
    /// <summary>
    /// 次の RowId 単一値の取得
    /// </summary>
    /// <returns>RowId の値（long）。DB シーケンスから新規採番</returns>
    /// <exception cref="InvalidOperationException">
    /// SQL Server へのアクセス失敗または値取得失敗時
    /// </exception>
    Task<long> GetNextValueAsync();

    /// <summary>
    /// 複数個の連続した RowId の取得
    /// </summary>
    /// <param name="count">取得個数。デフォルト 1。1以上である必要あり</param>
    /// <returns>採番された RowId のリスト（昇順、重複なし）</returns>
    /// <exception cref="ArgumentException">count が 0 以下の場合</exception>
    /// <exception cref="InvalidOperationException">
    /// SQL Server へのアクセス失敗または値取得失敗時
    /// </exception>
    Task<IReadOnlyList<long>> GetNextValuesAsync(int count = 1);
}
