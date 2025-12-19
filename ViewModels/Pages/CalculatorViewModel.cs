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

        private decimal CurrentValue => decimal.TryParse(DisplayText, NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
        CultureInfo.InvariantCulture, out var v) ? v : 0m;

        #endregion

        #region Commands
        [RelayCommand]
        private void Input(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            string raw = StripCommas(DisplayText);

            if (DisplayText == "Error")
            {
                if (key is "00" or "." || (key.Length == 1 && char.IsDigit(key[0])))
                {
                    Clear();
                    raw = "0";
                }
                else
                {
                    return;
                }
            }

            if (key == "00")
            {
                if (_isNewInput || raw == "0")
                {
                    raw = "0";
                }
                else
                {
                    raw += "00";
                }

                SetDisplayFromRaw(raw);
                _isNewInput = false;
                return;
            }

            if (key.Length == 1 && char.IsDigit(key[0]))
            {
                if (_isNewInput || raw == "0")
                    raw = key;
                else
                    raw += key;

                SetDisplayFromRaw(raw);
                _isNewInput = false;
                return;
            }

            if (key == ".")
            {
                if (_isNewInput)
                    raw = "0.";

                if (!raw.Contains(".", StringComparison.Ordinal))
                    raw += ".";

                SetDisplayFromRaw(raw);
                _isNewInput = false;
                return;
            }

            if (key == "±")
            {
                if (raw == "0" || raw == "0." || raw == "")
                    return;

                raw = raw.StartsWith("-", StringComparison.Ordinal)
                    ? raw[1..]
                    : "-" + raw;

                SetDisplayFromRaw(raw);
                return;
            }

            if (key == "⌫")
            {
                if (_isNewInput)
                    return;

                if (raw.Length <= 1 || (raw.Length == 2 && raw.StartsWith("-", StringComparison.Ordinal)))
                    raw = "0";
                else
                    raw = raw[..^1];

                if (raw == "" || raw == "-")
                    raw = "0";

                SetDisplayFromRaw(raw);
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
            DisplayText = FormatWithCommas(_storedValue.ToString(CultureInfo.InvariantCulture));

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

            DisplayText = FormatWithCommas(result.ToString(CultureInfo.InvariantCulture));

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

        private static string StripCommas(string s) => s.Replace(",", "");

        private static string FormatWithCommas(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "0";

            if (raw == "-" || raw == "Error")
                return raw;

            bool negative = raw.StartsWith("-", StringComparison.Ordinal);
            string body = negative ? raw[1..] : raw;

            string intPart;
            string fracPart = "";
            bool hasDot = body.Contains('.', StringComparison.Ordinal);

            if (hasDot)
            {
                var parts = body.Split('.', 2);
                intPart = parts[0].Length == 0 ? "0" : parts[0];
                fracPart = parts.Length > 1 ? parts[1] : "";
            }
            else
            {
                intPart = body.Length == 0 ? "0" : body;
            }

            if (!long.TryParse(intPart, NumberStyles.None, CultureInfo.InvariantCulture, out var n))
                return raw;

            string formattedInt = n.ToString("N0", CultureInfo.InvariantCulture);

            string sign = negative ? "-" : "";
            return hasDot ? $"{sign}{formattedInt}.{fracPart}" : $"{sign}{formattedInt}";
        }

        private void SetDisplayFromRaw(string raw)
        {
            DisplayText = FormatWithCommas(raw);
        }


        #endregion
    }
}
