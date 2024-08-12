using AddWaterMark.Config;
using AddWaterMark.Utils;
using System;
using System.Diagnostics;
using System.Windows;

namespace AddWaterMark {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {

        public App() {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e) {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] processArray = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (Process process in processArray) {
                if (currentProcess.Id != process.Id) {
                    var iniData = IniParserUtils.GetIniData(Constants.SET_FILE);
                    var language = iniData[Constants.INI_SECTION_WINDOW][Constants.INI_KEY_LANGUAGE];
                    var errorMsg = "已存在运行程序！";
                    if ("zh_tw".Equals(language)) {
                        errorMsg = "已存在運行程式！";
                    } else if ("en".Equals(language)) {
                        errorMsg = "The Application Already Running!";
                    }
                    MessageBox.Show(errorMsg, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    DllUtils.SwitchToThisWindow(process.MainWindowHandle, true);
                    DllUtils.ShowWindow(process.MainWindowHandle, 1);
                    Environment.Exit(0);
                }
            }
        }

        private void Lnk_Click(object sender, RoutedEventArgs e) {
            Process.Start(((System.Windows.Documents.Hyperlink)sender).NavigateUri.ToString());
        }
    }
}
