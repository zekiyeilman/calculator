using System.Globalization;

namespace MauiApp1;

public partial class StandardCalculatorPage : ContentPage
{
    double number1 = 0;
    string operatorSymbol = "";
    bool isNewEntry = false;
    private double _memory = 0;

    public StandardCalculatorPage()
    {
        InitializeComponent();
    }

    private void NumClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        string input = button.Text;

        if (cScreen.Text == "0" || isNewEntry)
        {
            cScreen.Text = input;
            isNewEntry = false;
        }
        else
        {
            if (input == "," && cScreen.Text.Contains(",")) return;
            cScreen.Text += input;
        }
    }

    private void OperatorClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        number1 = double.Parse(cScreen.Text.Replace(",", "."), CultureInfo.InvariantCulture);
        operatorSymbol = btn.Text;
        lblHistory.Text = cScreen.Text + " " + operatorSymbol;
        isNewEntry = true;
    }

    private void EqualClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(operatorSymbol)) return;

        double number2 = double.Parse(cScreen.Text.Replace(",", "."), CultureInfo.InvariantCulture);
        double result = 0;

        switch (operatorSymbol)
        {
            case "+": result = number1 + number2; break;
            case "-": result = number1 - number2; break;
            case "*": result = number1 * number2; break;
            case "/":
                if (number1 == 0 && number2 == 0)
                {
                    cScreen.Text = "NaN";
                    lblHistory.Text = "0 / 0 =";
                    operatorSymbol = "";
                    isNewEntry = true;
                    return;
                }
                if (number2 == 0)
                {
                    cScreen.Text = "Sıfıra bölünemez";
                    lblHistory.Text = $"{number1.ToString(CultureInfo.CurrentCulture)} / 0 =";
                    operatorSymbol = "";
                    isNewEntry = true;
                    return;
                }
                result = number1 / number2;
                break;
        }

        lblHistory.Text = $"{number1.ToString(CultureInfo.CurrentCulture)} {operatorSymbol} {number2.ToString(CultureInfo.CurrentCulture)} =";
        cScreen.Text = result.ToString(CultureInfo.CurrentCulture);
        operatorSymbol = "";
        isNewEntry = true;
    }

    private void Clear_Clicked(object sender, EventArgs e)
    {
        cScreen.Text = "0";
        lblHistory.Text = "";
        number1 = 0;
        operatorSymbol = "";
    }

    private void ClearEntry_Clicked(object sender, EventArgs e)
    {
        cScreen.Text = "0";
    }

    private void Function_Clicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        double currentNumber = double.Parse(cScreen.Text.Replace(",", "."), CultureInfo.InvariantCulture);
        double result = 0;

        switch (btn.Text)
        {
            case "x²": result = currentNumber * currentNumber; break;
            case "²√x": result = Math.Sqrt(currentNumber); break;
            case "1/x":
                if (currentNumber == 0) { cScreen.Text = "Sıfıra bölünemez"; return; }
                result = 1 / currentNumber; break;
            case "+/-": result = -currentNumber; break;
        }
        cScreen.Text = result.ToString(CultureInfo.CurrentCulture);
        isNewEntry = true;
    }

    private void Percentage_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(operatorSymbol)) return;

        double number2 = double.Parse(cScreen.Text.Replace(",", "."), CultureInfo.InvariantCulture);
        double result = (number1 * number2) / 100;
        cScreen.Text = result.ToString(CultureInfo.CurrentCulture);
        lblHistory.Text = $"{number1} sayısının %{number2}'si";
    }

    private void Memory_Clicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        if (!double.TryParse(cScreen.Text.Replace(",", "."), CultureInfo.InvariantCulture, out double currentValue)) return;

        switch (button.Text)
        {
            case "MC": _memory = 0; break;
            case "MR": cScreen.Text = _memory.ToString(CultureInfo.CurrentCulture); break;
            case "M+": _memory += currentValue; break;
            case "M-": _memory -= currentValue; break;
            case "MS": _memory = currentValue; break;
        }
        isNewEntry = true;
    }

    private void Memory_View_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Hafıza", $"Hafızada kayıtlı değer: {_memory}", "Tamam");
    }

    private void BackSpaceClicked(object sender, EventArgs e)
    {
        cScreen.Text = cScreen.Text.Length > 1 ? cScreen.Text[..^1] : "0";
    }
}