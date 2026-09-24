using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Presentation.Shared.ViewModels;

/// <summary>
/// BusinessDayClockを画面のボタンから操作するための、WpfTrial と WinTrial 共通の ViewModel
/// </summary>
/// <remarks>
/// <para>【用途】検証時に、アプリケーションを再起動せずに業務日を開始・終了・やり直し</para>
/// <para>【設計】WPF／WinForms のどちらにも依存しない（各アプリは View だけを持つ）</para>
/// <para>【注意】業務日は ON 中に 0 時を越えると変わるが、表示の自動更新はなし。コマンドの実行時と <see cref="Refresh"/> の呼び出し時に再取得</para>
/// <para>【参照】docs/Common/Clocks/BusinessDayClock_詳細設計書.md §6.4</para>
/// </remarks>
public partial class BusinessDayClockViewModel : ObservableObject
{
    private readonly IBusinessDayClockControl? _control;

    /// <summary>
    /// ON 中かどうかを示す値
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TurnOnCommand))]
    [NotifyCanExecuteChangedFor(nameof(TurnOffCommand))]
    private bool isOn;

    /// <summary>
    /// 現在の業務日の表示用文字列（<c>yyyy/MM/dd</c>）。クロックが BusinessDay 以外の場合は空文字
    /// </summary>
    [ObservableProperty]
    private string currentBusinessDateText = string.Empty;

    /// <summary>
    /// 状態の表示用文字列（<c>ON 中</c>／<c>OFF 中</c>）。クロックが BusinessDay 以外の場合は空文字
    /// </summary>
    [ObservableProperty]
    private string statusText = string.Empty;

    /// <summary>
    /// 操作に失敗した場合のエラーメッセージ。エラーなしの場合は空文字
    /// </summary>
    [ObservableProperty]
    private string errorMessage = string.Empty;

    /// <summary>
    /// <see cref="BusinessDayClockViewModel"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="control">BusinessDayClockの操作用の口。ClockType が BusinessDay 以外で DI に未登録の場合は既定値の <see langword="null"/></param>
    public BusinessDayClockViewModel(IBusinessDayClockControl? control = null)
    {
        _control = control;
        Refresh();
    }

    /// <summary>
    /// BusinessDayClockを操作できるかどうかを示す値
    /// </summary>
    /// <value>ClockType が BusinessDay の場合は <see langword="true"/>。<see langword="false"/> の場合、各画面は操作パネルを非表示</value>
    public bool IsAvailable => _control is not null;

    /// <summary>
    /// 業務日と状態の表示の再取得
    /// </summary>
    public void Refresh()
    {
        if (_control is null)
        {
            IsOn = false;
            CurrentBusinessDateText = string.Empty;
            StatusText = string.Empty;
            return;
        }

        IsOn = _control.IsOn;
        CurrentBusinessDateText = _control.CurrentBusinessDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        StatusText = IsOn ? "ON 中" : "OFF 中";
    }

    /// <summary>
    /// 業務日の開始（前回終了した業務日の翌日、一番最初は基点日から）
    /// </summary>
    /// <remarks>
    /// <para>【副作用】失敗した場合は <see cref="ErrorMessage"/> を設定。例外の送出なし</para>
    /// </remarks>
    [RelayCommand(CanExecute = nameof(CanTurnOn))]
    private void TurnOn() => Execute(control => control.TurnOn());

    /// <summary>
    /// 業務日の終了
    /// </summary>
    /// <remarks>
    /// <para>【副作用】失敗した場合は <see cref="ErrorMessage"/> を設定。例外の送出なし</para>
    /// </remarks>
    [RelayCommand(CanExecute = nameof(CanTurnOff))]
    private void TurnOff() => Execute(control => control.TurnOff());

    /// <summary>
    /// 業務日の進行のやり直し（基点日から再開）
    /// </summary>
    /// <remarks>
    /// <para>【副作用】失敗した場合は <see cref="ErrorMessage"/> を設定。例外の送出なし</para>
    /// </remarks>
    [RelayCommand(CanExecute = nameof(IsAvailable))]
    private void Reset() => Execute(control => control.Reset());

    private bool CanTurnOn() => IsAvailable && !IsOn;

    private bool CanTurnOff() => IsAvailable && IsOn;

    private void Execute(Action<IBusinessDayClockControl> action)
    {
        if (_control is null)
        {
            return;
        }

        try
        {
            action(_control);
            ErrorMessage = string.Empty;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ErrorMessage = $"業務日の状態ファイルの保存に失敗しました: {ex.Message}";
        }

        Refresh();
    }
}
