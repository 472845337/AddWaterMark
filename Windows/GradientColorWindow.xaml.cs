using AddWaterMark.Beans;
using AddWaterMark.Config;
using AddWaterMark.Utils;
using AddWaterMark.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AddWaterMark.Windows {
    /// <summary>
    /// GradientColor.xaml 的交互逻辑
    /// </summary>
    public partial class GradientColorWindow : Window {
        readonly GradientColorViewModel vm = new GradientColorViewModel();
        public string GradientColorResult { get; set; }
        public GradientColorWindow(string gradientColor) {
            InitializeComponent();
            Loaded += Window_Loaded;
            vm.GradientColorList = new ObservableCollection<GradientColor>(GradientColorUtils.GetList(gradientColor));
            vm.ChangeGradient();
        }

        private void Window_Loaded(object sender, EventArgs e) {
            DataContext = vm;
        }

        private void CheckPoint_TextChanged(object sender, RoutedEventArgs e) {
            TextBox pointTextBox = (TextBox)sender;
            string point = pointTextBox.Text;
            if (string.IsNullOrEmpty(point)) {
                return;
            }
            bool isNumeric = NumberUtils.IsNumeric(point, out double pointD);
            if (!isNumeric || pointD < 0 || pointD > 1) {
                MessageBox.Show("请输入0-1之间的数值");
            }

        }
        private void ChangeColor_MouseLeftButtonUp(object sender, RoutedEventArgs e) {
            FrameworkElement ele = sender as FrameworkElement;
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if (System.Windows.Forms.DialogResult.OK == cd.ShowDialog()) {
                GradientColor gradientColor = ele.Tag as GradientColor;
                gradientColor.Color = System.Drawing.ColorTranslator.ToHtml(cd.Color);
                vm.ChangeGradient();
            }
        }

        /// <summary>
        /// 删除渐变点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteGradientColor_Click(object sender, RoutedEventArgs e) {
            if (vm.GradientColorList.Count < 3) {
                MessageBox.Show("渐变色不能小于2种！", Constants.MSG_ERROR);
                return;
            }
            Button deleteButton = sender as Button;
            GradientColor deleteColor = deleteButton.Tag as GradientColor;
            vm.GradientColorList.Remove(deleteColor);
            vm.ChangeGradient();
        }

        /// <summary>
        /// 添加渐变色块
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddGradientColor_Click(object sender, RoutedEventArgs e) {
            vm.GradientColorList.Add(new GradientColor(1, "#FFFFFF"));
            vm.ChangeGradient();
        }

        private void PointSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            vm.ChangeGradient();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            vm.GradientColorList.Clear();
            vm.WaterMarkFontGradientColor = string.Empty;
            DataContext = null;
            DialogResult = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            foreach(GradientColor color in vm.GradientColorList) {
                if (color.Point < 0 || color.Point > 1) {
                    MessageBox.Show("请输入0-1之间的数值");
                    return;
                }
            }
            List<GradientColor> gradientColorList = new List<GradientColor>(vm.GradientColorList);
            gradientColorList.Sort(delegate (GradientColor c1, GradientColor c2) {
                return c1.Point > c2.Point ? 1 : -1;
            });
            GradientColorResult = GradientColorUtils.GetString(gradientColorList);
            vm.GradientColorList.Clear();
            vm.WaterMarkFontGradientColor = string.Empty;
            DataContext = null;
            DialogResult = true;
        }
    }
}
