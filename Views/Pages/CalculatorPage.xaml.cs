using Calculator.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calculator.Views.Pages
{
    public partial class CalculatorPage : Page
    {
        public CalculatorPage(CalculatorViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            Root.Focus(); // 포커스가 있어야 키 입력이 Root로 옵니다
        }

        private void Root_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (DataContext is not CalculatorViewModel vm) return;

            var t = e.Text; // 예: "+", "-", "*", "/", "1", ".", "="
            if (t.Length != 1) return;

            // 숫자 / 소수점
            if (char.IsDigit(t[0]) || t == ".")
            {
                vm.InputCommand.Execute(t);
                e.Handled = true;
                return;
            }

            // 메인 키보드 연산자 (Shift 조합 포함)
            if (t is "+" or "-" or "*" or "/")
            {
                vm.OperateCommand.Execute(t);
                e.Handled = true;
                return;
            }

            // 메인 키보드 '=' (Shift 안 누른 = 키)
            if (t == "=")
            {
                vm.EqualsCommand.Execute(null);
                e.Handled = true;
                return;
            }
        }

        private void Root_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not CalculatorViewModel vm) return;

            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                vm.EqualsCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                vm.InputCommand.Execute("⌫");
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                vm.ClearCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}