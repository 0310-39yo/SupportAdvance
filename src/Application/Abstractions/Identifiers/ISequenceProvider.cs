namespace SupportAdvance.Application.Abstractions.Identifiers;

/// <summary>
/// DB シーケンスから連続した RowId を採番するインターフェース
///
/// 【責務】
/// - dbo.s_row_id_sequence から次の RowId を取得
/// - 単一値および複数値の採番をサポート
/// - 全 Bounded Context で共用可能な汎用インターフェース
///
/// 【実装】
/// - Infrastructure層 で SqlConnection を使用した SQL実行
/// - スレッドセーフな実装を保証
///
/// 【使用例】
/// var rowId = await sequenceProvider.GetNextValueAsync();
/// var rowIds = await sequenceProvider.GetNextValuesAsync(count: 5);
///
/// 【使用箇所】
/// - Employee Context: CreateEmployeeUseCase 他
/// - Identity Context: 将来実装時に使用予定
/// - Master Context: 将来実装時に使用予定
/// </summary>
public interface ISequenceProvider
{
    /// <summary>
    /// 次の RowId 単一値を取得する
    /// </summary>
    /// <returns>RowId の値（long）。DB シーケンスから新規採番</returns>
    /// <exception cref="InvalidOperationException">
    /// SQL Server へのアクセス失敗または値取得失敗時
    /// </exception>
    Task<long> GetNextValueAsync();

    /// <summary>
    /// 複数個の連続した RowId を取得する
    /// </summary>
    /// <param name="count">取得個数。デフォルト 1。1以上である必要あり</param>
    /// <returns>採番された RowId のリスト（昇順、重複なし）</returns>
    /// <exception cref="ArgumentException">count が 0 以下の場合</exception>
    /// <exception cref="InvalidOperationException">
    /// SQL Server へのアクセス失敗または値取得失敗時
    /// </exception>
    Task<IReadOnlyList<long>> GetNextValuesAsync(int count = 1);
}
