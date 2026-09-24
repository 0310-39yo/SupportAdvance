using System.Text.Json;

namespace SupportAdvance.Common.Clocks;

/// <summary>
/// BusinessDayClockの状態ファイル（JSON）の読み書き
/// </summary>
/// <param name="filePath">状態ファイルのパス</param>
/// <remarks>
/// <para>【注意】複数のプロセスでの同じファイルの共有は非対応（排他制御なし）</para>
/// </remarks>
internal sealed class BusinessDayClockStateFile(string filePath)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// 状態ファイルのパス
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>
    /// 状態ファイルの読み込み
    /// </summary>
    /// <returns>読み込んだ状態。ファイルがない場合、または読み込めない・JSON として不正な場合は <see langword="null"/>（例外の送出なし）</returns>
    public BusinessDayClockState? Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            return JsonSerializer.Deserialize<BusinessDayClockState>(File.ReadAllText(FilePath), JsonOptions);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    /// <summary>
    /// 状態ファイルへの書き込み
    /// </summary>
    /// <param name="state">書き込む状態</param>
    /// <exception cref="IOException">書き込みに失敗した場合</exception>
    /// <exception cref="UnauthorizedAccessException">書き込み権限がない場合</exception>
    /// <remarks>
    /// <para>【設計】一時ファイルに書いてから置き換えることによる、書き込み途中の異常終了でのファイル破損の防止</para>
    /// </remarks>
    public void Save(BusinessDayClockState state)
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = FilePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(state, JsonOptions));
        File.Move(temporaryPath, FilePath, overwrite: true);
    }

    /// <summary>
    /// 状態ファイルの削除
    /// </summary>
    /// <exception cref="IOException">削除に失敗した場合</exception>
    /// <exception cref="UnauthorizedAccessException">削除権限がない場合</exception>
    /// <remarks>
    /// <para>【注意】ファイルがない場合は何もしない</para>
    /// </remarks>
    public void Delete()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}
