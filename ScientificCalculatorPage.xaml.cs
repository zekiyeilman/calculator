using System.Data;
using System.Globalization;

namespace MauiApp1;

public partial class ScientificCalculatorPage : ContentPage
{
    private string _currentExpression = "";
    private bool _isResultShown = false;
    private double _memory;
    private bool _isDegreeMode = true;
    private bool _isSecond = false;
    private bool _isHyperbolic = false;

    public ScientificCalculatorPage()
    {
        InitializeComponent();
        OnClearClicked(this, EventArgs.Empty);
    }

    private void OnNumberClicked(object sender, EventArgs e)
    {
        if (_isResultShown)
        {
            _currentExpression = "";
            _isResultShown = false;
        }
        var button = sender as Button;
        _currentExpression += button.Text;
        UpdateDisplay();
    }

    void OnConstantClicked(object sender, EventArgs e)
    {
        if (_isResultShown)
        {
            _currentExpression = "";
            _isResultShown = false;
        }
        var button = sender as Button;
        string constantValue = "";
        if (button.Text == "π") constantValue = Math.PI.ToString(CultureInfo.InvariantCulture);
        else if (button.Text == "e") constantValue = Math.E.ToString(CultureInfo.InvariantCulture);

        _currentExpression += constantValue;
        UpdateDisplay();
    }

    private void OnCharacterClicked(object sender, EventArgs e)
    {
        _isResultShown = false;
        var button = sender as Button;
        _currentExpression += $" {button.Text} ";
        UpdateDisplay();
    }

    private void OnOperatorClicked(object sender, EventArgs e)
    {
        _isResultShown = false;
        var button = sender as Button;
        _currentExpression += $" {button.Text} ";
        UpdateDisplay();
    }

    private void OnEqualsClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentExpression)) return;

        try
        {
            HistoryLabel.Text = _currentExpression.Replace(" ", "") + "=";
            string expressionToCompute = PrepareExpressionForCalculation(_currentExpression);

            var result = new DataTable().Compute(expressionToCompute, null);
            Display.Text = Convert.ToDouble(result).ToString("G", CultureInfo.CurrentCulture);
            _currentExpression = Display.Text.Replace(',', '.');
            _isResultShown = true;
        }
        catch (Exception)
        {
            Display.Text = "Hata";
            _currentExpression = "";
            _isResultShown = true;
        }
    }

    private string PrepareExpressionForCalculation(string expression)
    {
        string calcExpression = expression.Replace(",", ".").Replace("×", "*").Replace("÷", "/").Replace("mod", "%");

        var parts = new List<string>(calcExpression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

        while (parts.Contains("xʸ"))
        {
            int index = parts.IndexOf("xʸ");
            if (index > 0 && index < parts.Count - 1)
            {
                double left = double.Parse(parts[index - 1], CultureInfo.InvariantCulture);
                double right = double.Parse(parts[index + 1], CultureInfo.InvariantCulture);
                double result = Math.Pow(left, right);

                parts[index - 1] = result.ToString(CultureInfo.InvariantCulture);
                parts.RemoveAt(index + 1);
                parts.RemoveAt(index);
            }
            else
            {
                throw new InvalidExpressionException("Üs alma işlemi için geçersiz ifade.");
            }
        }

        return string.Join(" ", parts);
    }

    private void OnBackspaceClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentExpression)) return;

        if (_isResultShown)
        {
            _isResultShown = false;
        }

        _currentExpression = _currentExpression.TrimEnd();
        if (!string.IsNullOrEmpty(_currentExpression))
        {
            int lastSpace = _currentExpression.LastIndexOf(' ');
            if (lastSpace != -1)
            {
                _currentExpression = _currentExpression.Substring(0, lastSpace).TrimEnd();
            }
            else
            {
                _currentExpression = "";
            }
        }
        UpdateDisplay();
    }

    private void OnFunctionClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null || string.IsNullOrWhiteSpace(_currentExpression)) return;

        string func = button.Text;
        var (lastNumber, index) = FindLastNumberInExpression(_currentExpression);

        if (lastNumber == null) return;

        double currentValue = double.Parse(lastNumber.Replace(',', '.'), CultureInfo.InvariantCulture);
        double result = 0;

        try
        {
            switch (func)
            {
                case "x²": result = Math.Pow(currentValue, 2); break;
                case "²√x": result = Math.Sqrt(currentValue); break;
                case "1/x":
                    if (currentValue == 0) throw new DivideByZeroException();
                    result = 1 / currentValue; break;
                case "10ˣ": result = Math.Pow(10, currentValue); break;
                case "log": result = Math.Log10(currentValue); break;
                case "ln": result = Math.Log(currentValue); break;
                case "exp": result = Math.Exp(currentValue); break;
                case "+/-": result = -currentValue; break;
                case "n!":
                    if (currentValue < 0 || currentValue != Math.Floor(currentValue)) throw new Exception("Geçersiz giriş");
                    result = 1;
                    for (int i = 2; i <= currentValue; i++) result *= i;
                    break;
                default: return;
            }

            _currentExpression = _currentExpression.Remove(index, lastNumber.Length).Insert(index, result.ToString(CultureInfo.InvariantCulture));

            UpdateDisplay();
            HistoryLabel.Text = $"{func}({currentValue.ToString(CultureInfo.CurrentCulture)})";
            _isResultShown = true;
        }
        catch { Display.Text = "Hata"; _currentExpression = ""; _isResultShown = true; }
    }

    private void OnSpecialFunctionClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        if (button.Text == "rand")
        {
            _currentExpression = new Random().NextDouble().ToString(CultureInfo.InvariantCulture);
            UpdateDisplay();
            _isResultShown = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(_currentExpression)) return;

        string func = button.Text;
        var (lastNumber, index) = FindLastNumberInExpression(_currentExpression);

        if (lastNumber == null) return;

        double currentValue = double.Parse(lastNumber.Replace(',', '.'), CultureInfo.InvariantCulture);

        try
        {
            string resultString = "";
            switch (func)
            {
                case "|x|": resultString = Math.Abs(currentValue).ToString(CultureInfo.InvariantCulture); break;
                case "⌊x⌋": resultString = Math.Floor(currentValue).ToString(CultureInfo.InvariantCulture); break;
                case "⌈x⌉": resultString = Math.Ceiling(currentValue).ToString(CultureInfo.InvariantCulture); break;
                case "→dms":
                    int deg = (int)currentValue;
                    double minutes = (currentValue - deg) * 60;
                    int min = (int)minutes;
                    double seconds = (minutes - min) * 60;
                    Display.Text = $"{deg}° {min}' {seconds:F2}\"";
                    HistoryLabel.Text = $"dms({currentValue})";
                    _isResultShown = true;
                    return;
                case "→deg":
                    int d = (int)currentValue;
                    double fractional = currentValue - d;
                    int m = (int)(fractional * 100);
                    double s = (fractional * 10000) - (m * 100);
                    resultString = (d + (m / 60.0) + (s / 3600.0)).ToString(CultureInfo.InvariantCulture);
                    break;
                default: return;
            }

            _currentExpression = _currentExpression.Remove(index, lastNumber.Length).Insert(index, resultString);
            UpdateDisplay();
            HistoryLabel.Text = $"{func}({currentValue})";
            _isResultShown = true;
        }
        catch { Display.Text = "Hata"; _currentExpression = ""; _isResultShown = true; }
    }

    private (string, int) FindLastNumberInExpression(string expression)
    {
        var parts = expression.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return (null, -1);

        string lastPart = parts[parts.Length - 1];
        if (double.TryParse(lastPart.Replace(',', '.'), CultureInfo.InvariantCulture, out _))
        {
            int index = expression.LastIndexOf(lastPart);
            return (lastPart, index);
        }

        return (null, -1);
    }

    private void UpdateDisplay()
    {
        string textToShow = _currentExpression.Replace(" ", "").Replace(".", ",");
        Display.Text = string.IsNullOrWhiteSpace(textToShow) ? "0" : textToShow;
    }

    void OnMemoryClicked(object sender, EventArgs e)
    {
        if (!double.TryParse(Display.Text.Replace(',', '.'), CultureInfo.InvariantCulture, out double currentValue)) return;

        switch (((Button)sender).Text)
        {
            case "MC": _memory = 0; break;
            case "MR":
                _currentExpression = _memory.ToString(CultureInfo.InvariantCulture);
                UpdateDisplay();
                break;
            case "M+": _memory += currentValue; break;
            case "M-": _memory -= currentValue; break;
            case "MS": _memory = currentValue; break;
        }
    }

    private void Memory_View_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Hafıza", $"Hafızada kayıtlı değer: {_memory}", "Tamam");
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        _currentExpression = "";
        HistoryLabel.Text = "";
        _isResultShown = false;
        UpdateDisplay();
    }

    void OnFeClicked(object sender, EventArgs e)
    {
        if (Display.Text == "Hata" || Display.Text == "0") return;
        try
        {
            double currentValue = double.Parse(Display.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
            Display.Text = currentValue.ToString("E2", CultureInfo.CurrentCulture);
            _currentExpression = currentValue.ToString("E", CultureInfo.InvariantCulture);
            _isResultShown = true;
        }
        catch
        {
            Display.Text = "Hata";
            _currentExpression = "";
            _isResultShown = true;
        }
    }

    #region Trigonometri Kodları
    private void OnTrigonometryToggleClicked(object sender, EventArgs e)
    {
        TrigonometryGrid.IsVisible = !TrigonometryGrid.IsVisible;
        if (TrigonometryGrid.IsVisible) FunctionGrid.IsVisible = false;
    }

    private void OnFunctionToggleClicked(object sender, EventArgs e)
    {
        FunctionGrid.IsVisible = !FunctionGrid.IsVisible;
        if (FunctionGrid.IsVisible) TrigonometryGrid.IsVisible = false;
    }

    void OnSecondClicked(object sender, EventArgs e)
    {
        _isSecond = !_isSecond;
        var activeColor = Color.FromArgb("#A46CDA");
        var inactiveColor = Color.FromArgb("#3B3B3B");

        SecondButton.BackgroundColor = _isSecond ? activeColor : inactiveColor;
        MainSecondButton.BackgroundColor = _isSecond ? activeColor : inactiveColor;

        SinButton.Text = _isSecond ? "sin⁻¹" : "sin";
        CosButton.Text = _isSecond ? "cos⁻¹" : "cos";
        TanButton.Text = _isSecond ? "tan⁻¹" : "tan";
        SecButton.Text = _isSecond ? "sec⁻¹" : "sec";
        CscButton.Text = _isSecond ? "csc⁻¹" : "csc";
        CotButton.Text = _isSecond ? "cot⁻¹" : "cot";
    }

    void OnHypClicked(object sender, EventArgs e)
    {
        _isHyperbolic = !_isHyperbolic;
        HypButton.BackgroundColor = _isHyperbolic ? Color.FromArgb("#A46CDA") : Color.FromArgb("#3B3B3B");
    }

    void OnTrigFunctionClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;
        string func = button.Text;
        ProcessTrigonometryFunction(func);
    }

    void OnDegRadClicked(object sender, EventArgs e)
    {
        _isDegreeMode = !_isDegreeMode;
        DegRadButton.Text = _isDegreeMode ? "DEG" : "RAD";
    }

    private void ProcessTrigonometryFunction(string func)
    {
        var (lastNumber, index) = FindLastNumberInExpression(_currentExpression);
        if (lastNumber == null) return;

        double currentValue = double.Parse(lastNumber.Replace(',', '.'), CultureInfo.InvariantCulture);
        double result = 0;
        double angleInRadians = _isDegreeMode ? (currentValue * Math.PI) / 180 : currentValue;
        try
        {
            if (!_isHyperbolic)
            {
                if (!_isSecond)
                {
                    switch (func)
                    {
                        case "sin": result = Math.Sin(angleInRadians); break;
                        case "cos": result = Math.Cos(angleInRadians); break;
                        case "tan": result = Math.Tan(angleInRadians); break;
                        case "sec": result = 1 / Math.Cos(angleInRadians); break;
                        case "csc": result = 1 / Math.Sin(angleInRadians); break;
                        case "cot": result = 1 / Math.Tan(angleInRadians); break;
                    }
                }
                else
                {
                    double arcResult = 0;
                    switch (func)
                    {
                        case "sin⁻¹": arcResult = Math.Asin(currentValue); break;
                        case "cos⁻¹": arcResult = Math.Acos(currentValue); break;
                        case "tan⁻¹": arcResult = Math.Atan(currentValue); break;
                        case "sec⁻¹": arcResult = Math.Acos(1 / currentValue); break;
                        case "csc⁻¹": arcResult = Math.Asin(1 / currentValue); break;
                        case "cot⁻¹": arcResult = Math.Atan(1 / currentValue); break;
                    }
                    result = _isDegreeMode ? (arcResult * 180) / Math.PI : arcResult;
                }
            }
            else
            {
                if (!_isSecond)
                {
                    switch (func)
                    {
                        case "sin": result = Math.Sinh(currentValue); break;
                        case "cos": result = Math.Cosh(currentValue); break;
                        case "tan": result = Math.Tanh(currentValue); break;
                        case "sec": result = 1 / Math.Cosh(currentValue); break;
                        case "csc": result = 1 / Math.Sinh(currentValue); break;
                        case "cot": result = 1 / Math.Tanh(currentValue); break;
                    }
                }
                else
                {
                    switch (func)
                    {
                        case "sin⁻¹": result = Math.Asinh(currentValue); break;
                        case "cos⁻¹": result = Math.Acosh(currentValue); break;
                        case "tan⁻¹": result = Math.Atanh(currentValue); break;
                        case "sec⁻¹": result = Math.Acosh(1 / currentValue); break;
                        case "csc⁻¹": result = Math.Asinh(1 / currentValue); break;
                        case "cot⁻¹": result = Math.Atanh(1 / currentValue); break;
                    }
                }
            }

            result = Math.Round(result, 12);

            _currentExpression = _currentExpression.Remove(index, lastNumber.Length).Insert(index, result.ToString(CultureInfo.InvariantCulture));

            UpdateDisplay();
            HistoryLabel.Text = $"{func}({currentValue})";
            _isResultShown = true;
        }
        catch { Display.Text = "Hata"; _currentExpression = ""; _isResultShown = true; }
        finally
        {
            if (_isSecond) OnSecondClicked(SecondButton, EventArgs.Empty);
            if (_isHyperbolic) OnHypClicked(HypButton, EventArgs.Empty);
        }
    }
    #endregion
}