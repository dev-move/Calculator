using System.Globalization;

namespace Calculator.ViewModels.Pages
{
    public partial class CalculatorViewModel : ObservableObject
    {
        #region Filds
        private decimal _storedValue = 0;
        private CalcOp _pendingOp = CalcOp.None;
        private bool _isNewInput = true;
        #endregion

        #region Properties
        [ObservableProperty]
        private string displayText = "0";

        [ObservableProperty]
        private string expressionText = string.Empty;

        private decimal CurrentValue => decimal.TryParse(DisplayText, 
            NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var v)
    ? v : 0m;

        #endregion

        #region Commands
        [RelayCommand]
        private void Input(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (key == "00")
            {
                if (DisplayText == "Error")
                {
                    Clear();
                    DisplayText = "0";
                    _isNewInput = false;
                    return;
                }

                if (_isNewInput || DisplayText == "0")
                {
                    DisplayText = "0";
                    _isNewInput = false;
                    return;
                }

                DisplayText += "00";
                _isNewInput = false;
                return;
            }

            if (DisplayText == "Error")
            {
                if (key.Length == 1 && char.IsDigit(key[0]))
                {
                    Clear();
                    DisplayText = key;
                    _isNewInput = false;
                }
                return;
            }

            if (key.Length == 1 && char.IsDigit(key[0]))
            {
                if (_isNewInput || DisplayText == "0")
                    DisplayText = key;
                else
                    DisplayText += key;

                _isNewInput = false;
                return;
            }

            if (key == ".")
            {
                if (_isNewInput)
                {
                    DisplayText = "0.";
                    _isNewInput = false;
                    return;
                }

                if (!DisplayText.Contains("."))
                    DisplayText += ".";
                return;
            }

            if (key == "±")
            {
                if (DisplayText == "0" || DisplayText == "Error")
                    return;

                if (DisplayText.StartsWith("-", StringComparison.Ordinal))
                    DisplayText = DisplayText[1..];
                else
                    DisplayText = "-" + DisplayText;

                return;
            }

            if (key == "⌫")
            {
                if (_isNewInput)
                    return;

                if (DisplayText.Length <= 1 || (DisplayText.Length == 2 && DisplayText.StartsWith("-")))
                    DisplayText = "0";
                else
                    DisplayText = DisplayText[..^1];
                return;
            }
        }



        [RelayCommand]
        private void Operate(string op)
        {
            if (string.IsNullOrWhiteSpace(op))
                return;
            if (DisplayText == "Error")
            {
                Clear();
                return;
            }

            if (_pendingOp != CalcOp.None && !_isNewInput)
            {
                _storedValue = Apply(_storedValue, CurrentValue, _pendingOp);
            }
            else if (_pendingOp == CalcOp.None)
            {
                _storedValue = CurrentValue;
            }

            _pendingOp = op switch
            {
                "+" => CalcOp.Add,
                "-" => CalcOp.Sub,
                "*" => CalcOp.Mul,
                "/" => CalcOp.Div,
                _ => CalcOp.None

            };


            ExpressionText = $"{_storedValue.ToString(CultureInfo.InvariantCulture)} {op}";
            DisplayText = _storedValue.ToString(CultureInfo.InvariantCulture);

            _isNewInput = true;

        }

        [RelayCommand]
        private void Equals()
        {
            if (_pendingOp == CalcOp.None || DisplayText == "Error")
                return;

            var b = CurrentValue;

            if (_pendingOp == CalcOp.Div && b == 0m)
            {
                DisplayText = "Error";
                ExpressionText = string.Empty;
                _storedValue = 0m;
                _pendingOp = CalcOp.None;
                _isNewInput = true;
                return;
            }

            var result = Apply(_storedValue, b, _pendingOp);

            ExpressionText =
                $"{_storedValue.ToString(CultureInfo.InvariantCulture)} {OpToSymbol(_pendingOp)} {b.ToString(CultureInfo.InvariantCulture)} =";

            DisplayText = result.ToString(CultureInfo.InvariantCulture);

            _storedValue = result;
            _pendingOp = CalcOp.None;
            _isNewInput = true;
        }

        [RelayCommand]
        private void Clear()
        {
            DisplayText = "0";
            ExpressionText = string.Empty;

            _storedValue = 0m;
            _pendingOp = CalcOp.None;
            _isNewInput = true;
        }
        #endregion

        #region Constructors
        
        #endregion

        #region Methods
        private static decimal Apply(decimal a, decimal b, CalcOp op) => op switch
        {
            CalcOp.Add => a + b,
            CalcOp.Sub => a - b,
            CalcOp.Mul => a * b,
            CalcOp.Div => a / b,
            _ => b
        };

        private static string OpToSymbol(CalcOp op) => op switch
        {
            CalcOp.Add => "+",
            CalcOp.Sub => "-",
            CalcOp.Mul => "*",
            CalcOp.Div => "/",
            _ => ""
        };
        
        

        #endregion
    }
}
