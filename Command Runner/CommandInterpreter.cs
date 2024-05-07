using System.Text.RegularExpressions;

namespace GSR.CommandRunner
{
    public class CommandInterpreter : ICommandInterpreter
    {
        private const string FUNCTION_ASSIGN_TYPE = "fa";
        private const string ASSIGN_TYPE = "a";
        private const string STRING_LITERAL_TYPE = "sl";
        private const string NUMERIC_LITERAL_TYPE = "nl";
        private const string VARIABLE_UNWRAP_TYPE = "vu";
        private const string VARIABLE_INVOKE_TYPE = "ve";

        private const string META_COMMAND_START_REGEX = @"^~\s*\.\s*";
        private const string MEMBER_NAME_REGEX = @"^[_a-zA-Z][_0-9a-zA-Z]*";
        private const string NUMERIC_START_CHAR_REGEX = @"[-0-9]";
        private const string NUMERIC_REGEX = @"^-?[0-9]+([sil]|((\.[0-9]+)?[fdm]))";
        private const string ILLEGAL_NUMERIC_REGEX = @"^-?[0-9]+\.[0-9]+[sil]";
        private const string ARGUMENT_REGEX = @"^([^,\(\)]|(\(.*?\)))*";

#warning, add more escapes;
        private static readonly IList<Tuple<string, string>> ESCAPE_REPLACEMENTS = new List<Tuple<string, string>>() { Tuple.Create(@"\\", @"\"), Tuple.Create(@"\""", @"""") };
        private static readonly IEnumerable<Tuple<string, string>> ESCAPE_REPLACEMENTS_R = ESCAPE_REPLACEMENTS.Select((x) => Tuple.Create(x.Item1.Replace(@"\", @"\\"), x.Item2.Replace(@"\", @"\\")));

        private static readonly string UNTIL_END_QUOTE_REGEX = @"^([^\\""]" + ESCAPE_REPLACEMENTS_R.Select((x) => x.Item1).Aggregate("|", (x, y) => $"{x}({y})|")[..^1] + @")*";

        private static readonly ICommandSet s_metaCommands = new CommandSet(typeof(CommandInterpreter));
        private readonly ICommandSet m_commands;
        private readonly ISessionContext m_sessionContext;
        private int m_uniqueNumber = 0;



        public CommandInterpreter(ICommandSet defaultCommands) : this(defaultCommands, new SessionContext()) { } // end constructor

        public CommandInterpreter(ICommandSet defaultCommands, ISessionContext sessionContext)
        {
            m_commands = defaultCommands;
            m_sessionContext = sessionContext;
        } // end constructor



        public ICommand Evaluate(string input) => _Evaluate(input);


        private ICommand _Evaluate(string input, bool chainedType = false, object? chainedOn = null)
        {
            string parse = input.Trim();
            if (parse.Length == 0)
                throw new InvalidSyntaxException("Command was empty.");

            if (Regex.IsMatch(parse[..1], NUMERIC_START_CHAR_REGEX))
            {
                if (chainedType)
                    throw new InvalidOperationException("Can't chain to numeric literal.");

                return ReadNumericLiteral(input);
            }
            else if (parse[0].Equals('"'))
            {
                if (chainedType)
                    throw new InvalidOperationException("Can't chain to string literal.");

                return ReadStringLiteral(input);
            }
            else if (parse[0].Equals('$'))
                return ReadVariable(input, chainedType, chainedOn);

            /*else if (Regex.IsMatch(parse, META_COMMAND_START_REGEX))
                {
                    parse = Regex.Replace(input, META_COMMAND_START_REGEX, "");
                    return ReadMetaCommand(parse);
                    string cName = Regex.Match(input, MEMBER_NAME_REGEX).Groups[0].Value;

                }*/
            throw new NotImplementedException();
        } // end ReadCommand()



        private ICommand ReadNumericLiteral(string input)
        {
            string parse = input.Trim();

            if (!Regex.IsMatch(parse[..1], NUMERIC_START_CHAR_REGEX))
                throw new InvalidOperationException($"{nameof(ReadStringLiteral)} should not be called with a value that's not starting with \'\"\'");

            if (Regex.IsMatch(parse, ILLEGAL_NUMERIC_REGEX))
                throw new InvalidSyntaxException("Integral numerics may not contain decimal places.");

            string rVal = Regex.Match(parse, NUMERIC_REGEX).Value;
            parse = Regex.Replace(parse, NUMERIC_REGEX, string.Empty);

            object value = rVal[^1] switch
            {
                's' => short.Parse(rVal[..^1]),
                'i' => int.Parse(rVal[..^1]),
                'l' => long.Parse(rVal[..^1]),
                'f' => float.Parse(rVal[..^1]),
                'd' => double.Parse(rVal[..^1]),
                'm' => decimal.Parse(rVal[..^1]),
                _ => throw new InvalidStateException("numeric literal type indentification failed, this shouldn't happen"),
            };

            if (value.Equals(float.PositiveInfinity))
                throw new OverflowException($"\"{rVal}\" is too small or too large.");

            if (parse.Equals(string.Empty))
                return CommandFor(NUMERIC_LITERAL_TYPE, value.GetType(), () => value);
            else if (parse[0].Equals('.'))
            {
                parse = parse[1..].TrimStart();
                return _Evaluate(parse, true, value);
            }
            else
                throw new InvalidSyntaxException($"Couldn't interpret value: \"{input}\"");
        } // end ReadNumericLiteral

        private ICommand ReadStringLiteral(string input)
        {
            string parse = input.Trim();

            if (!parse[0].Equals('"'))
                throw new InvalidOperationException($"{nameof(ReadStringLiteral)} should not be called with a value that's not starting with \'\"\'");

            parse = parse[1..];
            string value = Regex.Match(parse, UNTIL_END_QUOTE_REGEX).Value;
            value = ESCAPE_REPLACEMENTS.Aggregate(value, (x, y) => x.Replace(y.Item1, y.Item2));

            parse = Regex.Replace(parse, UNTIL_END_QUOTE_REGEX, string.Empty);
            if (parse.Length == 0 || !parse[0].Equals('"'))
                throw new InvalidSyntaxException("Invalid string literal, didn't find expectedend quote ");

            parse = parse[1..].TrimStart();

            if (parse.Equals(string.Empty))
                return CommandFor(STRING_LITERAL_TYPE, typeof(string), () => value);
            else if (parse[0].Equals('.'))
            {
                parse = parse[1..].TrimStart();
                return _Evaluate(parse, true, value);
            }
            else
                throw new InvalidSyntaxException($"Couldn't interpret value: \"{input}\"");
        } // end ReadStringLiteral

        private ICommand ReadVariable(string input, bool chainedType = false, object? chainedOn = null)
        {
            string parse = input.Trim();
            if (!parse[0].Equals('$'))
                throw new InvalidOperationException($"{nameof(ReadVariable)} should not be called with a value that's not starting with \'$\'");

            parse = parse[1..];
            string varName = Regex.Match(parse, MEMBER_NAME_REGEX).Value;
            parse = Regex.Replace(parse, MEMBER_NAME_REGEX, string.Empty).TrimStart();

            if (parse.Length >= 2 && parse[..2].Equals("=>"))
            {
                parse = parse[2..].TrimStart();
                ICommand val = _Evaluate(parse);
                return CommandFor(FUNCTION_ASSIGN_TYPE, () => m_sessionContext.SetValue(varName, val));
            }
            else if (parse.Length >= 1 && parse[0].Equals('='))
            {
                parse = parse[1..].TrimStart();
                ICommand val = _Evaluate(parse);
                return CommandFor(ASSIGN_TYPE, () => m_sessionContext.SetValue(varName, val.Execute(Array.Empty<object>())));
            }
            else 
            {
                object? val = m_sessionContext.GetValue(varName, typeof(object));

                if (parse.Equals(string.Empty))
                    return CommandFor(VARIABLE_UNWRAP_TYPE, val?.GetType() ?? typeof(object), () => val);
                else if (parse[0].Equals('.'))
                {
                    parse = parse[1..].TrimStart();
                    return _Evaluate(parse, true, val);
                }
                else if (parse[0].Equals('('))
                {
                    if (!typeof(ICommand).IsAssignableFrom(val?.GetType()))
                        throw new InvalidOperationException("Can't invoke non-command varaible.");

                    parse = parse[1..].TrimStart();
                    ICommand varV = (ICommand)val;

                    if (parse[0].Equals(')'))
                    {
                        parse = parse[1..].TrimStart();
                        if (chainedType)
                        {
                            // refactor out probably.
                            if (!(varV.ParameterTypes.Length >= 1))
                                throw new InvalidOperationException("Can't chain to function without a parameter");

                            if (chainedOn != null && !varV.ParameterTypes[0].IsAssignableFrom(chainedOn.GetType()))
                                throw new TypeMismatchException($"Chained type mismatched. Expected {varV.ParameterTypes[0]} or subtype,  got {chainedOn?.GetType()}");
                        }

                        if (parse.Equals(string.Empty))
                        {
                            if (chainedType)
                                return CommandFor(VARIABLE_INVOKE_TYPE, varV.ReturnType, chainedOn, () => varV.Execute());
                            else
                                return CommandFor(VARIABLE_INVOKE_TYPE, varV.ReturnType, () => varV.Execute());
                        }
                        else if (parse[0].Equals('.'))
                        {
                            parse = parse[1..].TrimStart();
                            if (chainedType)
                                return _Evaluate(parse, true, varV.Execute());
                            else
                                return _Evaluate(parse, true, varV.Execute());
                        }
                        else
                            throw new InvalidSyntaxException($"Unexpected character: \"{parse[0]}\", after variable invoke for: \"${varName}\"");
                    }
                    else
                    {
#warning
                        throw new NotImplementedException();
                        // capture arguments etc
                        //ARGUMENT_REGEX
                        // if command try to invoke
                    }
                }
                else
                    throw new InvalidSyntaxException($"Unexpected character: \"{parse[0]}\", after variable: \"${varName}\"");
            }
        } // end ReadVariable



#warning .> vs . for chaining. first passes function, parameterize or not; second pass value of function, or if has ? parameters creates a command that will execute when given args to match them
#warning > before argument as well.



        private ICommand CommandFor(string type, Action value) => new Command($"{type}_{++m_uniqueNumber}", typeof(void), Array.Empty<Type>(), (x) => { value(); return null; });

        private ICommand CommandFor(string type, Type returnType, Func<object?> value) => new Command($"{type}_{++m_uniqueNumber}", returnType, Array.Empty<Type>(), (x) => value());

        private ICommand CommandFor(string type, Type returnType, object? chainedOn, Func<object?> value)
        {
            throw new NotImplementedException();
            // included chained on ? parameters in results
            // Command($"{type}_{++m_uniqueNumber}", returnType, Array.Empty<Type>(), (x) => value());
        } // end CommandForChained()

        private ICommand CommandForFunctionChained(string type, Type returnType, object? chainedOn, object?[] args, Func<object?> value) => throw new NotImplementedException();

    } // end class
} // end namespace