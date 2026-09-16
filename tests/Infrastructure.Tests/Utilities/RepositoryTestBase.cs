using System.Data;
using SupportAdvance.Infrastructure.Persistence;

namespace SupportAdvance.Infrastructure.Tests.Utilities;

/// <summary>
/// リポジトリテストの共通ベースクラス
///
/// 【責務】
/// - IAsyncLifetime パターンの実装
/// - テストデータ ID の追跡（_createdRowIds）
/// - DisposeAsync での確実なクリーンアップ
/// - 各テストメソッドで try/finally による確実なクリーンアップ
///
/// 【使用方法】
/// 1. RepositoryTestBase を継承
/// 2. InitializeAsync で _connectionFactory を初期化
/// 3. CleanupAsync をオーバーライドしてクリーンアップロジック実装
/// 4. テストメソッドで try/finally { await DisposeAsync(); }
/// </summary>
public abstract class RepositoryTestBase : IAsyncLifetime
{
    protected IDbConnectionFactory _connectionFactory = null!;
    protected readonly List<long> _createdRowIds = new();

    public abstract Task InitializeAsync();

    /// <summary>
    /// テスト終了時のクリーンアップ
    /// 【重要】各テストメソッドで try/finally { await DisposeAsync(); } で確実に実行
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_createdRowIds.Count == 0)
        {
            return;
        }

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            foreach (var rowId in _createdRowIds)
            {
                await CleanupAsync(connection, rowId);
            }

            _createdRowIds.Clear();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Cleanup error: {ex.Message}");
            // クリーンアップ失敗時も進行（テストランナーのエラーを避けるため）
        }
    }

    /// <summary>
    /// 各テストクラスが Override してクリーンアップロジックを実装
    /// 【例】EmployeeRepositoryTests では Employee/Person/DepartmentMembership を削除
    /// </summary>
    protected abstract Task CleanupAsync(IDbConnection connection, long rowId);

    protected static void ExecuteNonQuery(IDbConnection connection, string sql, long rowId)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        AddParam(cmd, "@rowId", rowId);
        cmd.ExecuteNonQuery();
    }

    protected static void AddParam(IDbCommand cmd, string name, object value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        cmd.Parameters.Add(param);
    }
}
