using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PlutoScarab
{
    public static class LaTeX
    {
        public static string From<T, U>(Expression<Func<T, U>> expr)
        {
            var s = new StringBuilder();
            Build(expr.Body, s);
            return s.ToString();
        }

        private static void Build(Expression expr, StringBuilder s)
        {
            switch (expr)
            {
                case ConstantExpression c:
                    if (c.Value is null)
                    {
                        s.Append("\\bot");
                        return;
                    }

                    switch (c.Value)
                    {
                        case int:
                        case long:
                        case short:
                        case sbyte:
                        case uint:
                        case ulong:
                        case ushort:
                        case byte:
                        case float:
                            s.Append(c.Value);
                            return;

                        case bool:
                            s.Append((bool)c.Value ? 'T' : 'F');
                            return;

                        case double:
                            var dbl = (double)c.Value;
                            if (dbl == Math.PI)
                                s.Append("\\pi");
                            else if (dbl == Math.E)
                                s.Append("e");
                            else if (dbl == Math.Tau)
                                s.Append("2\\pi");
                            else
                                s.Append(c.Value);
                            return;

                        default:
                            throw new NotImplementedException();
                    }

                case ParameterExpression p:
                    s.Append(p.Name);
                    return;

                case UnaryExpression u:
                    switch (expr.NodeType)
                    {
                        case ExpressionType.UnaryPlus:
                            Build(u.Operand, s);
                            return;

                        case ExpressionType.Negate:
                        case ExpressionType.NegateChecked:
                            s.Append('-');
                            BuildParens(u.Operand, s);
                            return;

                        case ExpressionType.Not:
                            s.Append("\\neg");

                            if (u.Operand is ConstantExpression || u.Operand is ParameterExpression)
                            {
                                s.Append(' ');
                                Build(u.Operand, s);
                            }
                            else
                            {
                                s.Append('(');
                                Build(u.Operand, s);
                                s.Append(')');
                            }

                            return;

                        default:
                            throw new NotImplementedException();
                    }

                case BinaryExpression b:
                    switch (expr.NodeType)
                    {
                        case ExpressionType.Add:
                        case ExpressionType.AddChecked:
                            BuildBinary("+", b, s);
                            return;

                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractChecked:
                            BuildBinary("-", b, s);
                            return;

                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyChecked:
                            if ((b.Left is ConstantExpression || b.Left is ParameterExpression) && (b.Right is ConstantExpression || b.Right is ParameterExpression))
                            {
                                Build(b.Left, s);
                                s.Append("\\cdot");
                                Build(b.Right, s);
                            }
                            else
                            {
                                BuildParens(b.Left, s);
                                BuildParens(b.Right, s);
                            }
                            return;

                        case ExpressionType.Divide:
                            s.Append("\\frac{");
                            Build(b.Left, s);
                            s.Append("}{");
                            Build(b.Right, s);
                            s.Append("}");
                            return;

                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                            BuildBinary("\\wedge", b, s);
                            return;

                        case ExpressionType.Or:
                        case ExpressionType.OrElse:
                            BuildBinary("\\vee", b, s);
                            return;

                        case ExpressionType.Modulo:
                            BuildBinary("\\bmod", b, s);
                            return;

                        case ExpressionType.Power:
                            BuildBinary("^", b, s);
                            return;

                        default:
                            throw new NotImplementedException();
                    }

                case MethodCallExpression m:
                    if (!(m.Object is null))
                    {
                        throw new NotImplementedException();
                    }
                    if (m.Method.DeclaringType != typeof(Math))
                    {
                        throw new NotImplementedException();
                    }

                    switch (m.Method.Name)
                    {
                        case "Log":
                            s.Append("\\ln");
                            break;

                        case "Log2":
                            s.Append("\\log_2");
                            break;

                        case "Log10":
                            s.Append("\\log_{10}");
                            break;

                        case "Acos":
                        case "Acosh":
                        case "Asin":
                        case "Asinh":
                        case "Atan":
                        case "Atanh":
                            s.Append(m.Method.Name.Substring(1));
                            s.Append("^{-1}");
                            break;

                        case "Atan2":
                            s.Append("tan^{-1}(\\frac{");
                            Build(m.Arguments[0], s);
                            s.Append("}{");
                            Build(m.Arguments[1], s);
                            s.Append('}');
                            return;

                        case "FusedMultiplyAdd":
                            s.Append('{');
                            Build(m.Arguments[0], s);
                            s.Append("}{");
                            Build(m.Arguments[1], s);
                            s.Append("}+{");
                            Build(m.Arguments[2], s);
                            s.Append('}');
                            return;

                        case "Abs":
                            s.Append("\\left|");
                            Build(m.Arguments[0], s);
                            s.Append("\\right|");
                            return;

                        case "BigMul":
                            s.Append('{');
                            Build(m.Arguments[0], s);
                            s.Append("}{");
                            Build(m.Arguments[1], s);
                            s.Append('}');
                            return;

                        case "ILogB":
                            s.Append("\\left\\lfloor\\log_2(");
                            Build(m.Arguments[0], s);
                            s.Append(")\\right\\rfloor");
                            return;

                        case "Sqrt":
                            s.Append("\\sqrt{");
                            Build(m.Arguments[0], s);
                            s.Append('}');
                            return;

                        case "Cbrt":
                            s.Append("\\sqrt[3]{");
                            Build(m.Arguments[0], s);
                            s.Append('}');
                            return;

                        case "Exp":
                            s.Append("e^{");
                            Build(m.Arguments[0], s);
                            s.Append("}");
                            return;

                        case "Floor":
                            s.Append("\\left\\lfloor ");
                            Build(m.Arguments[0], s);
                            s.Append("\\right\\rfloor");
                            return;

                        case "MaxMagnitude":
                            s.Append("max(\\left\\lfloor ");
                            Build(m.Arguments[0], s);
                            s.Append("\\right\\rfloor,\\left\\lfloor ");
                            Build(m.Arguments[1], s);
                            s.Append("\\right\\rfloor)");
                            return;

                        case "MinMagnitude":
                            s.Append("min(\\left\\lfloor ");
                            Build(m.Arguments[0], s);
                            s.Append("\\right\\rfloor,\\left\\lfloor ");
                            Build(m.Arguments[1], s);
                            s.Append("\\right\\rfloor)");
                            return;

                        case "Pow":
                            s.Append('{');
                            Build(m.Arguments[0], s);
                            s.Append("}^{");
                            Build(m.Arguments[1], s);
                            s.Append("}");
                            return;

                        case "ScaleB":
                            s.Append("2^{");
                            Build(m.Arguments[1], s);
                            s.Append("}{");
                            Build(m.Arguments[0], s);
                            s.Append('}');
                            return;

                        default:
                            s.Append(m.Method.Name);
                            break;
                    }

                    s.Append('(');
                    var sep = "";

                    foreach (var arg in m.Arguments)
                    {
                        s.Append(sep);
                        sep = ",";
                        Build(arg, s);
                    }

                    s.Append(')');
                    return;

                default:
                    throw new NotImplementedException();
            }
        }

        private static bool BuildParens(Expression expr, StringBuilder s)
        {
            if (expr is BinaryExpression b)
            {
                switch (b.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        s.Append('(');
                        Build(expr, s);
                        s.Append(')');
                        return true;
                }
            }
            else if (expr is UnaryExpression u)
            {
                switch (u.NodeType)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        s.Append('(');
                        Build(expr, s);
                        s.Append(')');
                        return true;
                }
            }

            Build(expr, s);
            return false;
        }

        private static void BuildBinary(string op, BinaryExpression b, StringBuilder s)
        {
            BuildParens(b.Left, s);
            s.Append(op);

            if (op.StartsWith('\\'))
            {
                s.Append(' ');
            }

            BuildParens(b.Right, s);
        }

        private static void BuildUnary(string op, UnaryExpression expr, StringBuilder s)
        {
            s.Append(op);
            s.Append('{');
            Build(expr.Operand, s);
            s.Append('}');
        }
    }
}