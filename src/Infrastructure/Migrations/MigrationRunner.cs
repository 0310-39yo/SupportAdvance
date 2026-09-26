using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Infrastructure.Migrations;

/// <summary>
/// 統一マイグレーション実行エンジン
/// </summary>
/// <remarks>
/// <para>【責務】複数 BC のマイグレーションを一元管理・実行</para>
/// <para>【用途】アプリケーション起動時のDB初期化</para>
/// <para>【実装】各 BC の埋め込みSQLスクリプトを自動検出して実行</para>
/// <para>【マイグレーション検出ルール】</para>
/// <list type="bullet">
/// <item><description>パス: Contexts/*/Infrastructure/Migrations/*.sql</description></item>
/// <item><description>実行順序: ファイル名の昇順（グローバルに昇順）</description></item>
/// <item><description>例: 000_CreateMigrationHistoryTable.sql → 001_CreateUserPreferencesTable.sql</description></item>
/// </list>
/// </remarks>
public class MigrationRunner
{
    private readonly string _connectionString;
    private readonly IClock _clock;

    /// <summary>
    /// <see cref="MigrationRunner"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="connectionString">マイグレーションを実行する SQL Server への接続文字列</param>
    /// <param name="clock">マイグレーション履歴に記録する実行日時（JST）の取得元</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public MigrationRunner(string connectionString, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(clock);
        _connectionString = connectionString;
        _clock = clock;
    }

    /// <summary>
    /// すべてのマイグレーション（全 BC）を実行
    /// </summary>
    /// <remarks>
    /// <para>【実行順序】ファイル名の昇順（000_*, 001_*, ...）</para>
    /// <para>【履歴管理】__MigrationHistory テーブルで実行済みをトラッキング</para>
    /// <para>【重複防止】既に実行済みのマイグレーションはスキップ</para>
    /// <para>【トランザクション】各マイグレーションをトランザクション内で実行</para>
    /// <para>【例外】SQL実行失敗時は SqlException をスロー＆ロールバック</para>
    /// </remarks>
    public async Task RunMigrationsAsync()
    {
        var migrations = GetAllMigrationScripts();

        if (migrations.Count == 0)
        {
            Console.WriteLine("⚠ No migration scripts found");
            return;
        }

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (var (name, sql) in migrations)
        {
            // 実行済みチェック
            if (await IsMigrationExecutedAsync(connection, name))
            {
                Console.WriteLine($"⊘ Migration already executed: {name}");
                continue;
            }

            using var transaction = connection.BeginTransaction();
            try
            {
                Console.WriteLine($"Executing migration: {name}");
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = sql;
                command.CommandTimeout = 300;
                await command.ExecuteNonQueryAsync();

                // 履歴に記録
                await RecordMigrationAsync(connection, transaction, name, true);

                await transaction.CommitAsync();
                Console.WriteLine($"✓ Migration completed: {name}");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"✗ Migration failed: {name}");
                Console.WriteLine($"Error: {ex.Message}");

                // 失敗を記録
                using var failTx = connection.BeginTransaction();
                try
                {
                    await RecordMigrationAsync(connection, failTx, name, false);
                    await failTx.CommitAsync();
                }
                catch { /* 記録失敗は無視 */ }

                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    /// <summary>
    /// すべての BC からマイグレーションスクリプトを検出
    /// </summary>
    /// <remarks>
    /// <para>【検索対象】すべての リファレンス Assembly</para>
    /// <para>【リソース形式】Contexts/{BCName}/Infrastructure/Migrations/{NNN_*.sql}</para>
    /// <para>【ソート】ファイル名で昇順（BC間でも昇順）</para>
    /// </remarks>
    private static List<(string Name, string Sql)> GetAllMigrationScripts()
    {
        var migrations = new List<(string Name, string Sql)>();
        var processedNames = new HashSet<string>(); // 重複排除用

        // すべての参照 Assembly をスキャン
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("CarPreferences.Infrastructure") ?? false);

        foreach (var assembly in assemblies)
        {
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(rn => rn.Contains("Migrations") && rn.EndsWith(".sql"))
                .ToList();

            foreach (var resourceName in resourceNames)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;

                using var reader = new StreamReader(stream);
                var sql = reader.ReadToEnd();
                var name = ExtractMigrationName(resourceName);

                if (!processedNames.Contains(name))
                {
                    migrations.Add((name, sql));
                    processedNames.Add(name);
                }
            }
        }

        // ファイル名で昇順ソート
        return migrations.OrderBy(m => m.Name).ToList();
    }

    /// <summary>
    /// リソース名からマイグレーション名を抽出
    ///
    /// 例: SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Migrations.001_CreateUserPreferencesTable.sql
    /// → 001_CreateUserPreferencesTable.sql
    /// </summary>
    private static string ExtractMigrationName(string resourceName)
    {
        var parts = resourceName.Split('.');
        var lastParts = parts.TakeLast(2); // *.sql の前の部分
        return string.Join(".", lastParts);
    }

    /// <summary>
    /// マイグレーションが既に実行済みか確認
    /// </summary>
    private async Task<bool> IsMigrationExecutedAsync(SqlConnection connection, string migrationName)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM __MigrationHistory WHERE MigrationName = @Name";
            command.Parameters.AddWithValue("@Name", migrationName);
            var result = await command.ExecuteScalarAsync();
            return (int?)result > 0;
        }
        catch
        {
            return false; // 履歴テーブルなしなら実行
        }
    }

    /// <summary>
    /// マイグレーション実行履歴を記録
    /// </summary>
    private async Task RecordMigrationAsync(SqlConnection connection, SqlTransaction transaction, string migrationName, bool success)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO __MigrationHistory (MigrationName, ExecutedAt, Success)
            VALUES (@Name, @ExecutedAt, @Success)";
        command.Parameters.AddWithValue("@Name", migrationName);
        command.Parameters.AddWithValue("@ExecutedAt", _clock.JstNow.Value.ToString("O"));
        command.Parameters.AddWithValue("@Success", success ? 1 : 0);
        await command.ExecuteNonQueryAsync();
    }
}
