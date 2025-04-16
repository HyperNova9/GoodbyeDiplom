// MathExpressionParser.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoodbyeDiplom
{
    public class MathExpressionParser
    {
        private Dictionary<string, Func<double, double, double>> _variables = new Dictionary<string, Func<double, double, double>>()
        {
            { "x", (x, y) => x },
            { "y", (x, y) => y },
            { "e", (x, y) => Math.E },
            { "pi", (x, y) => Math.PI }
        };

        private Dictionary<string, Func<double, double>> _unaryFunctions = new Dictionary<string, Func<double, double>>(StringComparer.OrdinalIgnoreCase)
        {
            { "sin", Math.Sin },
            { "cos", Math.Cos },
            { "tan", Math.Tan },
            { "exp", Math.Exp },
            { "log", Math.Log },
            { "sqrt", Math.Sqrt },
            { "abs", Math.Abs }
        };

        private Dictionary<string, Func<double, double, double>> _binaryFunctions = new Dictionary<string, Func<double, double, double>>(StringComparer.OrdinalIgnoreCase)
        {
            { "pow", Math.Pow }
        };

        public Func<double, double, double> Parse(string expression)
        {
            var tokens = Tokenize(expression);
            var rpn = ConvertToRPN(tokens);
            return Compile(rpn);
        }

        private List<string> Tokenize(string expression)
        {
            var tokens = new List<string>();
            int i = 0;
            while (i < expression.Length)
            {
                if (char.IsWhiteSpace(expression[i]))
                {
                    i++;
                    continue;
                }

                if (char.IsLetter(expression[i]))
                {
                    int start = i;
                    while (i < expression.Length && char.IsLetter(expression[i]))
                        i++;
                    tokens.Add(expression.Substring(start, i - start));
                }
                else if (char.IsDigit(expression[i]) || expression[i] == '.')
                {
                    int start = i;
                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                        i++;
                    tokens.Add(expression.Substring(start, i - start));
                }
                else
                {
                    tokens.Add(expression[i].ToString());
                    i++;
                }
            }
            return tokens;
        }

        private List<string> ConvertToRPN(List<string> tokens)
        {
            var output = new List<string>();
            var stack = new Stack<string>();

            foreach (var token in tokens)
            {
                if (IsNumber(token) || IsVariable(token))
                {
                    output.Add(token);
                }
                else if (IsFunction(token))
                {
                    stack.Push(token);
                }
                else if (token == ",")
                {
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        output.Add(stack.Pop());
                    }
                }
                else if (IsOperator(token))
                {
                    while (stack.Count > 0 && IsOperator(stack.Peek()) && GetPrecedence(token) <= GetPrecedence(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
                else if (token == "(")
                {
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Pop(); // Удаляем "("
                    if (stack.Count > 0 && IsFunction(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                }
            }

            while (stack.Count > 0)
            {
                output.Add(stack.Pop());
            }

            return output;
        }

        private Func<double, double, double> Compile(List<string> rpn)
        {
            var stack = new Stack<Func<double, double, double>>();

            foreach (var token in rpn)
            {
                if (IsNumber(token))
                {
                    double num = double.Parse(token);
                    stack.Push((x, y) => num);
                }
                else if (IsVariable(token))
                {
                    if (_variables.TryGetValue(token.ToLower(), out var variable))
                    {
                        stack.Push(variable);
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown variable: {token}");
                    }
                }
                else if (IsOperator(token))
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    stack.Push(CombineOperators(left, right, token));
                }
                else if (IsFunction(token))
                {
                    if (_unaryFunctions.TryGetValue(token.ToLower(), out var unaryFunc))
                    {
                        var arg = stack.Pop();
                        stack.Push((x, y) => unaryFunc(arg(x, y)));
                    }
                    else if (_binaryFunctions.TryGetValue(token.ToLower(), out var binaryFunc))
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push((x, y) => binaryFunc(left(x, y), right(x, y)));
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown function: {token}");
                    }
                }
            }

            if (stack.Count != 1)
                throw new InvalidOperationException("Invalid expression");

            return stack.Pop();
        }

        private Func<double, double, double> CombineOperators(
            Func<double, double, double> left,
            Func<double, double, double> right,
            string op)
        {
            return op switch
            {
                "+" => (x, y) => left(x, y) + right(x, y),
                "-" => (x, y) => left(x, y) - right(x, y),
                "*" => (x, y) => left(x, y) * right(x, y),
                "/" => (x, y) => left(x, y) / right(x, y),
                "^" => (x, y) => Math.Pow(left(x, y), right(x, y)),
                _ => throw new ArgumentException($"Unknown operator: {op}")
            };
        }

        private bool IsNumber(string token) => double.TryParse(token, out _);
        private bool IsVariable(string token) => _variables.ContainsKey(token.ToLower());
        private bool IsFunction(string token) => _unaryFunctions.ContainsKey(token.ToLower()) || _binaryFunctions.ContainsKey(token.ToLower());
        private bool IsOperator(string token) => "+-*/^".Contains(token);
        private int GetPrecedence(string op) => op switch
        {
            "^" => 4,
            "*" or "/" => 3,
            "+" or "-" => 2,
            _ => 0
        };
    }
}