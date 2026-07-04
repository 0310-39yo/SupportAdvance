using System.Configuration;
using System.Data;
using System.Windows;
using SupportAdvance.Presentation.Shared;

namespace SupportAdvance.Presentation.WpfTrial
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        /// アプリケーション起動時にライセンスキーを登録
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Syncfusion ライセンスキーを環境変数から登録
            SyncfusionLicenseHelper.RegisterLicenseFromEnvironment();

            base.OnStartup(e);
        }
    }

}
