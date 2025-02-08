global using StswExpress;
using System.Windows;

namespace ExcelVbaSearch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : StswApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
       
            StswTranslator.AvailableLanguages = new() { "pl" };
            StswSettings.Default.Language = "pl";
           
        }
    }
}
