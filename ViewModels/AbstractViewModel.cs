using AddWaterMark.Commands;
using PropertyChanged;
using System.Windows;

namespace AddWaterMark.ViewModels {
    [AddINotifyPropertyChangedInterface]
    public class AbstractViewModel {
        public int FontSize { get; set; }
        // 取消按钮的命令，需要指定窗口为命令参数
        public RelayCommand CancelCommand { get; set; } = new RelayCommand(Cancel);

        public static void Cancel(object obj) {
            var window = (Window)obj;
            if (null != window) {
                window.DialogResult = false;
            }
        }
    }
}
