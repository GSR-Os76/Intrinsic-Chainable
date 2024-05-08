using System.Text.RegularExpressions;
using System.Threading.Channels;

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

        private const string MEMBER_NAME_REGEX = @"^[_a-zA-Z][_0-9a-zA-Z]*";
        private const string NUMERIC_START_CHAR_REGEX = @"[-0-9]";
        private const string NUMERIC_REGEX = @"^-?[0-9]+([sil]|((\.[0-9]+)?[fdm]))";
        private const string ILLEGAL_NUMERIC_REGEX = @"^-?[0-9]+\.[0-9]+[sil]";
        private const string ARGUMENT_REGEX = @"^([^,\(\)]|(\(.*?\)))+";

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



        public ICommand Evaluate(string input)
        {
            string parse = input.Trim();
            if (parse.Length == 0)
                throw new InvalidSyntaxException("Command was empty.");

            return _Evaluate(parse);
        } // end Evaluate()


        private ICommand _Evaluate(string input, ChainType chainedType = ChainType.NONE, object? chainedOn = null)
        {
            string parse = input;
            if (Regex.IsMatch(parse[..1], NUMERIC_START_CHAR_REGEX))
            {
                if (chainedType != ChainType.NONE)
                    throw new InvalidOperationException("Can't chain to numeric literal.");

                return ReadNumericLiteral(input);
            }
            else if (parse[0].Equals('"'))
            {
                if (chainedType != ChainType.NONE)
                    throw new InvalidOperationException("Can't chain to string literal.");

                return ReadStringLiteral(input);
            }
            else if (parse[0].Equals('$'))
                return ReadVariable(input, chainedType, chainedOn);
            else if (parse[0].Equals('~') || Regex.IsMatch(parse[..1], MEMBER_NAME_REGEX))
            {
                bool isMeta = false;
                if (parse[0].Equals('~')) 
                {
                    parse = parse[1..];
                    isMeta = true;
                }

                string name = Regex.Match(parse, MEMBER_NAME_REGEX).Value;
                parse = Regex.Replace(parse, MEMBER_NAME_REGEX, string.Empty);

                int paramCount = GetArgumentCount(parse) + chainedType != ChainType.NONE ? 1 : 0;
                ICommand c = (isMeta ? s_metaCommands : m_commands).GetCommand(name, paramCount);
                return ReadCommand(c, parse, chainedType, chainedOn);
            }
            else
                throw new InvalidSyntaxException($"Couldn't read command: {input}");
        } // end ReadCommand()



        private ICommand ReadNumericLiteral(string input)
        {
            string parse = input;
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

            ICommand c = CommandFor(NUMERIC_LITERAL_TYPE, value.GetType(), () => value);
            if (parse.Equals(string.Empty))
                return c;
            else if (IsChain(parse))
                return Chain(parse, value, c);
            else
                throw new InvalidSyntaxException($"Couldn't interpret value: \"{input}\"");
        } // end ReadNumericLiteral

        private ICommand ReadStringLiteral(string input)
        {
            string parse = input[1..];
            
            string value = Regex.Match(parse, UNTIL_END_QUOTE_REGEX).Value;
            value = ESCAPE_REPLACEMENTS.Aggregate(value, (x, y) => x.Replace(y.Item1, y.Item2));

            parse = Regex.Replace(parse, UNTIL_END_QUOTE_REGEX, string.Empty);
            if (parse.Length == 0 || !parse[0].Equals('"'))
                throw new InvalidSyntaxException("Invalid string literal, didn't find expectedend quote ");

            parse = parse[1..].TrimStart();

            ICommand c = CommandFor(STRING_LITERAL_TYPE, typeof(string), () => value);
            if (parse.Equals(string.Empty))
                return c;
            else if (IsChain(parse))
                return Chain(parse, value, c);
            else
                throw new InvalidSyntaxException($"Couldn't interpret value: \"{input}\"");
        } // end ReadStringLiteral

        private ICommand ReadVariable(string input, ChainType chainedType = ChainType.NONE, object? chainedOn = null)
        {
            string parse = input[1..];
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
                object? value = m_sessionContext.GetValue(varName, typeof(object));

                ICommand c = CommandFor(VARIABLE_UNWRAP_TYPE, value?.GetType() ?? typeof(object), () => value);
                if (parse.Equals(string.Empty))
                    return c;
                else if (IsChain(parse))
                    return Chain(parse, value, c);
                else if (parse[0].Equals('('))
                {
                    if (!typeof(ICommand).IsAssignableFrom(value?.GetType()))
                        throw new InvalidOperationException("Can't invoke non-command varaible.");

                    return ReadCommand((ICommand)value, parse, chainedType, chainedOn);
                }
                else
                    throw new InvalidSyntaxException($"Unexpected character: \"{parse[0]}\", after variable: \"${varName}\"");
            }
        } // end ReadVariable()

        private ICommand ReadCommand(ICommand c, string argsInput, ChainType chainedType = ChainType.NONE, object? chainedOn = null) 
        {
#warning implement
            string parse = argsInput[1..];

            if (parse[0].Equals(')'))
            {
                parse = parse[1..].TrimStart();
                if (chainedType != ChainType.NONE)
                {
                    if (!(c.ParameterTypes.Length >= 1))
                        throw new InvalidOperationException("Can't chain to function without a parameter");

                    if (chainedOn != null && !c.ParameterTypes[0].IsAssignableFrom(chainedOn.GetType()))
                        throw new TypeMismatchException($"Chained type mismatched. Expected {c.ParameterTypes[0]} or subtype,  got {chainedOn?.GetType()}");
                }

                if (parse.Equals(string.Empty))
                {
                    if (chainedType != ChainType.NONE)
                        return CommandFor(VARIABLE_INVOKE_TYPE, c.ReturnType, chainedOn, () => c.Execute());
                    else
                        return CommandFor(VARIABLE_INVOKE_TYPE, c.ReturnType, () => c.Execute());
                }
                else if (parse[0].Equals('.'))
                {
                    parse = parse[1..].TrimStart();
                    if (chainedType != ChainType.NONE)
                        return _Evaluate(parse, ChainType.CHAIN, c.Execute());
                    else
                        return _Evaluate(parse, ChainType.CHAIN, c.Execute());
                }
                else
                    throw new InvalidSyntaxException($"Unexpected character: \"{parse[0]}\", after variable invoke for: \"{c}\"");
            }
            else
            {
                throw new NotImplementedException();
                // capture arguments etc
                //ARGUMENT_REGEX
                // if command try to invoke
            }
        } // end ReadCommand()



        private bool IsChain(string input) => Regex.IsMatch(input, @"^\.>?");

        private ICommand Chain(string input, object? ifChain, ICommand ifFunctionChain) 
        {
            string parse = input;
            if (parse.Length >= 2 && parse[..2].Equals(".>"))
            {
                parse = parse[2..].TrimStart();
                return _Evaluate(parse, ChainType.FUNCTION_CHAIN, ifFunctionChain);
            }
            else if (parse[0].Equals('.'))
            {
                parse = parse[1..].TrimStart();
                return _Evaluate(parse, ChainType.CHAIN, ifChain);
            }
            else
                throw new InvalidSyntaxException($"Expected chain operator, but got {input}");
        } // end Chain()

        public static int GetArgumentCount(string input)
        {
            string parse = input[1..]; // remove espected parenthesis
            int count = 0;
            while (Regex.IsMatch(parse, ARGUMENT_REGEX))
            {
                parse = Regex.Replace(parse, ARGUMENT_REGEX, string.Empty);
                ++count;
                if (parse.Length == 0)
                    throw new InvalidSyntaxException("Command not ended");
                else if (parse[0].Equals(','))
                    parse = parse[1..].TrimStart();
                else if (parse[0].Equals(')'))
                    return count;
                else
                    throw new InvalidSyntaxException($"Unexpected character after command argument: \"{parse[0]}\""); // theoretically unreachable route
            }
            if (parse[0].Equals(')'))
                return count;
            else
                throw new InvalidSyntaxException($"Unexpected character while reading arguments: \"{parse[0]}\"");
        } // end GetArgumentCount()

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