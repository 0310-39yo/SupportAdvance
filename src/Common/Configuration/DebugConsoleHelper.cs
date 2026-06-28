using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SupportAdvance.Common.Configuration;

/// <summary>
/// デバッグ用コンソール表示支援ヘルパークラス
/// </summary>
public static class DebugConsoleHelper
{
    /// <summary>
    /// コンソール割り当てAPI呼び出し
    /// </summary>
    /// <returns>割り当て結果</returns>
    [DllImport("kernel32.dll")]
    public static extern bool AllocConsole();

    /// <summary>
    /// デバッグ時のコンソール表示処理
    /// </summary>
    public static void OpenConsoleForDebug()
    {
        if (!Debugger.IsAttached)
        {
            return;
        }

        AllocConsole(); // コンソールを表示
    }
}
