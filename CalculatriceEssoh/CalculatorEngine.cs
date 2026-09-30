using System.Globalization;

namespace CalculatriceEssoh;

/// <summary>Logique de calcul pure (sans UI), basée sur decimal pour éviter les erreurs d'arrondi binaire.</summary>
public class CalculatorEngine
{
    private const int MaxDigits = 15;
    private string _entry = "0";
    private decimal? _left;
    private char? _op;
    private bool _resetEntry;       // la prochaine saisie remplace l'entrée courante
    private bool _awaitingOperand;  // un opérateur vient d'être saisi
    private bool _justEvaluated;    // le dernier appui était "="
    private string _expression = "";

    public bool IsError { get; private set; }
    public string Display => _entry.Replace('.', ',');
    public string Expression => _expression.Replace('.', ',');

    public void InputDigit(char digit)
    {
        if (IsError) Clear();
        if (_justEvaluated) ResetOperation();
        if (_resetEntry || _entry == "0") { _entry = digit.ToString(); _resetEntry = false; _awaitingOperand = false; return; }
        if (_entry.Count(char.IsDigit) >= MaxDigits) return;
        _entry += digit;
    }

    public void InputDecimal()
    {
        if (IsError) Clear();
        if (_justEvaluated) ResetOperation();
        if (_resetEntry) { _entry = "0"; _resetEntry = false; _awaitingOperand = false; }
        if (!_entry.Contains('.')) _entry += ".";
    }

    public void Clear()
    {
        _entry = "0";
        _resetEntry = false;
        IsError = false;
        ResetOperation();
    }

    public void Backspace()
    {
        if (IsError) { Clear(); return; }
        if (_resetEntry) return;
        _entry = _entry.Length > 1 ? _entry[..^1] : "0";
        if (_entry is "-" or "-0") _entry = "0";
    }

    public void ToggleSign()
    {
        if (IsError || _entry == "0") return;
        _entry = _entry.StartsWith('-') ? _entry[1..] : "-" + _entry;
        _awaitingOperand = false;
    }

    public void Percent()
    {
        if (IsError) return;
        var value = Parse();
        if (_left is decimal left && _op is char op)
            value = op is '+' or '−' ? left * value / 100m : value / 100m;
        else
            value /= 100m;
        _entry = Format(value);
        _resetEntry = true;
        _awaitingOperand = false;
    }

    public void SetOperator(char op)
    {
        if (IsError) return;
        if (op == '-') op = '−';
        if (_op is not null && _awaitingOperand)   // changement d'opérateur avant le 2e opérande
        {
            _op = op;
            _expression = $"{Format(_left!.Value)} {op}";
            return;
        }
        var current = Parse();
        if (_op is char pending && _left is decimal left)   // enchaînement : 2 + 3 × ...
        {
            var expr = $"{Format(left)} {pending} {Format(current)} =";
            if (!TryCompute(left, pending, current, expr, out var result)) return;
            current = result;
        }
        _left = current;
        _op = op;
        _entry = Format(current);
        _awaitingOperand = true;
        _resetEntry = true;
        _justEvaluated = false;
        _expression = $"{Format(current)} {op}";
    }

    public void Equals()
    {
        if (IsError || _op is not char op || _left is not decimal left) return;
        var right = Parse();
        var expr = $"{Format(left)} {op} {Format(right)} =";
        if (!TryCompute(left, op, right, expr, out var result)) return;
        _expression = expr;
        _entry = Format(result);
        _left = null;
        _op = null;
        _resetEntry = true;
        _awaitingOperand = false;
        _justEvaluated = true;
    }

    private bool TryCompute(decimal l, char op, decimal r, string expr, out decimal result)
    {
        result = 0;
        try
        {
            result = op switch
            {
                '+' => l + r,
                '−' => l - r,
                '×' => l * r,
                '÷' => l / r,
                _ => r
            };
            return true;
        }
        catch (DivideByZeroException) { SetError("Division par zéro impossible", expr); }
        catch (OverflowException) { SetError("Nombre trop grand", expr); }
        return false;
    }

    private void SetError(string message, string expr)
    {
        ResetOperation();
        IsError = true;
        _entry = message;
        _resetEntry = false;
        _expression = expr;
    }

    private void ResetOperation()
    {
        _left = null;
        _op = null;
        _expression = "";
        _awaitingOperand = false;
        _justEvaluated = false;
    }

    private decimal Parse() => decimal.Parse(_entry, NumberStyles.Float, CultureInfo.InvariantCulture);

    private static string Format(decimal v)
    {
        v = Math.Round(v, 10, MidpointRounding.AwayFromZero);
        if (v == 0) return "0";
        if (Math.Abs(v) >= 1e15m) return ((double)v).ToString("0.######E+0", CultureInfo.InvariantCulture);
        return v.ToString("0.##########", CultureInfo.InvariantCulture);
    }
}

