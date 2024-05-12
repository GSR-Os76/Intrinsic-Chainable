using System.Collections.Immutable;
using System.Text;
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
        private const string COMMAND_INVOKE_TYPE = "ci";

        private const string MEMBER_NAME_REGEX = @"^[_a-zA-Z][_0-9a-zA-Z]*";
        private const string NUMERIC_START_CHAR_REGEX = @"[-0-9]";
        private const string NUMERIC_REGEX = @"^-?[0-9]+([sil]|((\.[0-9]+)?[fdm]))";
        private const string ILLEGAL_NUMERIC_REGEX = @"^-?[0-9]+\.[0-9]+[sil]";
        //private const string ARGUMENT_REGEX = @"^([^,\(\)]|(\(.*?\)))+";

// #warning, add more escapes;
        private static readonly IList<Tuple<string, string>> ESCAPE_REPLACEMENTS = new List<Tuple<string, string>>() { Tuple.Create(@"\\", @"\"), Tuple.Create(@"\""", @"""") };
        private static readonly IEnumerable<Tuple<string, string>> ESCAPE_REPLACEMENTS_R = ESCAPE_REPLACEMENTS.Select((x) => Tuple.Create(x.Item1.Replace(@"\", @"\\"), x.Item2.Replace(@"\", @"\\")));

        private static readonly string UNTIL_END_QUOTE_REGEX = @"^([^\\""]" + ESCAPE_REPLACEMENTS_R.Select((x) => x.Item1).Aggregate("|", (x, y) => $"{x}({y})|")[..^1] + @")*";

        private static readonly ICommandSet s_metaCommands = new CommandSet(typeof(MetaCommands));
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
                    throw new InvalidCommandOperationException("Can't chain to numeric literal.");

                return ReadNumericLiteral(input);
            }
            else if (parse[0].Equals('"'))
            {
                if (chainedType != ChainType.NONE)
                    throw new InvalidCommandOperationException("Can't chain to string literal.");

                return ReadStringLiteral(input);
            }
            else if (parse[0].Equals('$'))
                return ReadVariable(input, chainedType, chainedOn);
            else if (parse[0].Equals('~') || Regex.IsMatch(parse[..1], MEMBER_NAME_REGEX))
            {
                bool isMeta = false;
                if (parse[0].Equals('~'))
                {
                    if (chainedType != ChainType.NONE)
                        throw new InvalidCommandOperationException("Meta commands are not able to be chained into");

                    chainedType = ChainType.CHAIN;
                    chainedOn = this;

                    parse = parse[1..];
                    isMeta = true;
                }

                string name = Regex.Match(parse, MEMBER_NAME_REGEX).Value;
                parse = Regex.Replace(parse, MEMBER_NAME_REGEX, string.Empty);

                int paramCount = GetArgumentCount(parse) + (chainedType != ChainType.NONE ? 1 : 0);
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

            object value;
            try
            {
                value = rVal[^1] switch
                {
                    's' => short.Parse(rVal[..^1]),
                    'i' => int.Parse(rVal[..^1]),
                    'l' => long.Parse(rVal[..^1]),
                    'f' => float.Parse(rVal[..^1]),
                    'd' => double.Parse(rVal[..^1]),
                    'm' => decimal.Parse(rVal[..^1]),
                    _ => throw new InvalidStateException("numeric literal type indentification failed, this shouldn't happen"),
                };

            }
            catch (OverflowException e)
            {
                throw new NumericOverflowException(e.Message);
            }

            if (value.Equals(float.PositiveInfinity))
                throw new NumericOverflowException($"\"{rVal}\" is too small or too large.");

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
                        throw new InvalidCommandOperationException("Can't invoke non-command varaible.");

                    return ReadCommand((ICommand)value, parse, chainedType, chainedOn);
                }
                else
                    throw new InvalidSyntaxException($"Unexpected character: \"{parse[0]}\", after variable: \"${varName}\"");
            }
        } // end ReadVariable()

        private ICommand ReadCommand(ICommand c, string argsInput, ChainType chainedType = ChainType.NONE, object? chainedOn = null)
        {
            if (chainedType != ChainType.NONE)
            {
                if (!(c.ParameterTypes.Length >= 1))
                    throw new InvalidCommandOperationException("Can't chain to function without a parameter");

                if (chainedOn is not null)
                {
                    bool asIsMatch = c.ParameterTypes[0].IsAssignableFrom(chainedOn.GetType());
                    bool validTyped = chainedType == ChainType.CHAIN && chainedOn is ICommand co && co.ParameterTypes.Length > 0
                        ? c.ParameterTypes[0].IsAssignableFrom(co.ReturnType)
                        : asIsMatch;
                    if (!validTyped)
                        throw new TypeMismatchException($"Chained type mismatched. Expected {c.ParameterTypes[0]} or subtype,  got {chainedOn?.GetType()}");
                }
            }

            string parse = argsInput[1..].TrimStart();

            IList<Tuple<bool, ICommand>> args = new List<Tuple<bool, ICommand>>();
            while (!parse[0].Equals(')'))
            {
                string s = CaptureArgument(parse);
                bool functional = false;
                if (s[0].Equals('>'))
                {
                    functional = true;
                    s = s[1..].TrimStart();
                    parse = parse[1..].TrimStart();
                }

                if (s[0].Equals('?'))
                {
                    if (functional)
                        throw new InvalidCommandOperationException("Parameterize can't also be functional");

                    parse = parse[1..].TrimStart();
                    args.Add(Tuple.Create(functional, (ICommand)ParameterizedCommand.Parameterizer.Inst));
                }
                else
                {
                    args.Add(Tuple.Create(functional, _Evaluate(s)));
                    parse = parse[(s.Length)..];
                }


                if (parse.Length == 0)
                    throw new InvalidSyntaxException("Argument not ended, expected comma or close parenthesis.");
                if (parse[0].Equals(','))
                    parse = parse[1..].TrimStart();
                else if (!parse[0].Equals(')'))
                    throw new InvalidSyntaxException($"Unexpected character \"{parse[0]}\" after argument.");
            }

            int argCount = args.Count() + (chainedType != ChainType.NONE ? 1 : 0);
            if (argCount != c.ParameterTypes.Length)
                throw new InvalidCommandOperationException($"Parameter count mismatch for command: {c.Name}. Expected {c.ParameterTypes.Length}, got {argCount}.");

#warning validate args s

            if (!parse[0].Equals(')'))
                throw new InvalidSyntaxException($"Unexpected character \"{parse[0]}\"at args list close"); // shouldn't be an attainable state

            parse = parse[1..].TrimStart();

            ICommand command = CommandInvocationFor(COMMAND_INVOKE_TYPE, c, chainedType, chainedOn, args);
            if (parse.Length == 0)
                return command;
            if (IsChain(parse))
                return Chain(parse,
                    command.ParameterTypes.Length == 0 ? command.Execute() : command,
                    command);
            else
                throw new InvalidSyntaxException($"Unexpected character: \"{parse[0]}\", after variable invoke for: \"{c}\"");
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

        public int GetArgumentCount(string input)
        {
            string parse = input[1..]; // remove expected parenthesis
            int count = 0;

            if (parse.Length == 0)
                throw new InvalidSyntaxException("Expected argument but got nothing.");

            while (!parse[0].Equals(')'))
            {
                string arg = CaptureArgument(parse);
                parse = parse[(arg.Length)..];

                ++count;

                if (parse.Length == 0)
                    throw new InvalidSyntaxException("Command  \argument list not ended.");
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
                throw new InvalidStateException($"Unexpected character after reading arguments: \"{parse[0]}\". The branch was anticipated inpossible.");
        } // end GetArgumentCount()

        private string CaptureArgument(string input)
        {
            // assumes start parenthesis is removed
            if (input.Length == 0)
                throw new InvalidSyntaxException("Expected argument but got nothing.");

            int depth = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c.Equals('('))
                    ++depth;
                else if (depth == 0 && (c.Equals(',') || c.Equals(')'))) 
                {
                    string a = input[..i];
                    if (a.Length == 0)
                        throw new InvalidSyntaxException("Arguments may not be empty.");

                    return a;
                }                    
                else if (c.Equals(')'))
                    --depth;
            }
            throw new InvalidSyntaxException("Didn't find end of argument.");
        } // end CaptureArgument()



        private ICommand CommandFor(string type, Action value) => new Command($"{type}_{++m_uniqueNumber}", typeof(void), Array.Empty<Type>(), (x) => { value(); return null; });

        private ICommand CommandFor(string type, Type returnType, Func<object?> value) => new Command($"{type}_{++m_uniqueNumber}", returnType, Array.Empty<Type>(), (x) => value());



        private ICommand CommandInvocationFor(string type, ICommand c, ChainType chainType, object? chainedOn, IList<Tuple<bool, ICommand>> arguments)
        {
#warning perhaps translate arguments that need execution here. This will allow "X.Y(Z)" and "Y(X, Z) to be equivalent where X's a varaible holding a parameterzied command
    

                string name = $"{type}_{++m_uniqueNumber}";
            Tuple<Type[], bool> paramInfo = ParamInfoFor(c, chainType, chainedOn, arguments);
            Func<object?[], object?> func = (x) => FunctionFor(x, c, chainType, chainedOn, arguments);

            return paramInfo.Item2
                ? new ParameterizedCommand(name, c.ReturnType, paramInfo.Item1, func)
                : new Command(name, c.ReturnType, paramInfo.Item1, func);
        } // end CommandInvocationFor()

        private object? FunctionFor(object?[] x, ICommand c, ChainType chainType, object? chainedOn, IList<Tuple<bool, ICommand>> arguments)
        {
            int argNum = 0;
            IList<object?> innerArguments = new List<object?>();
            IList<Tuple<bool, ICommand>> a = arguments.ToList();

            if (chainType != ChainType.NONE)
            {
                if (chainType == ChainType.CHAIN && chainedOn is ParameterizedCommand co)
                    a.Insert(0, Tuple.Create(false, (ICommand)co));
                else
                    innerArguments.Add(chainedOn);
            }

            foreach (Tuple<bool, ICommand> argument in a)
            {
                if (argument.Item1)
                    innerArguments.Add(argument.Item2);
                else
                {
                    if (argument.Item2.GetType() == typeof(ParameterizedCommand.Parameterizer))
                        innerArguments.Add(x[argNum++]);
                    else if (argument.Item2 is ParameterizedCommand)
                        innerArguments.Add(argument.Item2.Execute(argument.Item2
                            .ParameterTypes
                            .Select((y) => x[argNum++])
                            .ToArray()
                            ));
                    else
                        innerArguments.Add(argument.Item2.Execute());
                }
            }

            return c.Execute(innerArguments.ToArray());
        } // end FunctionFor()

        private Tuple<Type[], bool> ParamInfoFor(ICommand c, ChainType chainType, object? chainedOn, IList<Tuple<bool, ICommand>> arguments)
        {
            bool isParameterized = false;
            IList<Type> paramTypes = new List<Type>();

            int argNum = 0;
            IList<Tuple<bool, ICommand>> a = arguments.ToList();
            if (chainType != ChainType.NONE)
            {
                if (chainType == ChainType.CHAIN && chainedOn is ParameterizedCommand co)
                    a.Insert(0, Tuple.Create(false, (ICommand)co));
                else
                    argNum++;
            }

            foreach (Tuple<bool, ICommand> argument in a)
            {
                if (!argument.Item1)
                {
                    if (argument.Item2.GetType() == typeof(ParameterizedCommand.Parameterizer))
                    {
                        isParameterized = true;
                        paramTypes.Add(c.ParameterTypes[argNum]);
                    }
                    else if (argument.Item2 is ParameterizedCommand)
                    {
                        isParameterized = true;
                        argument.Item2.ParameterTypes.ToList().ForEach((x) => paramTypes.Add(x));
                    }
                }
                argNum++;
            }

            return Tuple.Create(paramTypes.ToArray(), isParameterized);
        } // end ParamTypesFor()



        public static class MetaCommands
        {
            [Command]
            public static string Help(CommandInterpreter self)
            {
                return
                    $"Run ~{nameof(Help)}() to get an overview of general syntax, and helper commands. \r\n" +
                    $"Run ~{nameof(Variables)}() to view all current variables, their names/identifiers, their current types, and their values. \r\n" +
                    $"Run ~{nameof(Commands)}() to view all current commands, their names/identifiers, their return types, and their parameter types if such's applicable. \r\n" +
                    $"\r\n" +
                    $"Numeric Literals: \r\n" +
                    $"Six types of numeric literals are provided. \r\n" +
                    $"Three integral types: s(16bit), i(32bit), l(64bit) \r\n" +
                    $"Three floating type: f(32bit), d(64bit), m(128bit) \r\n" +
                    $"All numerics must be followed by the symbol for their type. \r\n" +
                    $"Including a decimal point in an integral will cause an expection, even if it's just \".0\". \r\n" +
                    $"Numerics must always start with a digit. \r\n" +
                    $"Any value exceeding the maximum or minimum value for it's type will trigger an exception. \r\n" +
                    $"\r\n" +
                    $"String Literals: \r\n" +
                    $"String literals are started and ended by a double quotation mark. \r\n" +
                    $"A double quotation mark can be escaped when proceeded by a single backslash. \r\n" +
                    $"A backslash can be escaped when proceeded by a single backslash. \r\n" +
                    $"\r\n" +
                    $"Variables: \r\n" +
                    $"All variables must start with a \"$\". Then an underscore or letter, then any number of underscores and letters and numbers are allowed. \r\n" +
                    $"Running the name of a variable returns it's stored value, \"unwrapping\" the variable, or if it doesn't exist, throws an exception. \r\n" +
                    $"Running the name of a variable that's holding an ICommand, followed by parentheses invokes the stored command. \r\n" +
                    $"A variable name followed by an \"=\" or \"=>\" are used to assign or reassign a variable. \r\n" +
                    $"\"=\" assignment evaluates the right side as a command, and then immediately invokes it without arguments. \r\n" +
                    $"\"=>\" assignment evaluates the right side as a command, and then stores it in the varible. \r\n" +
                    $"\r\n" +
                    $"Commands: \r\n" +
                    $"A default set of commands may be provided to the interpreter. \r\n" +
                    $"All comand must start with an underscore or letter, then any number of underscores and letters and numbers are allowed. \r\n" +
                    $"\r\n" +
                    $"Command Invocation: \r\n" +
                    $"A command including one in a variable may be invoked by following it with a matched pair of parenthesis. \r\n" +
                    $"Any number of arguments may be included within the matched parenthesis, separated by commas. \r\n" +
                    $"An argument proceeded by \">\" will be passed as a command, otherwise it'll be immediately executed eithout arguments, or if it's parameterized it'll be inlined. \r\n" +
                    $"If the considered command is chained into, the value chained into it is the first argument, and arguments with the matched parenthesis must start with the second. \r\n" +
                    $"Multiple command may exist with the same name so long as they all have distinct parameter counts. \r\n" +
                    $"An incorrect number of arguments will result in throwing an exception. \r\n" +
                    $"\r\n" +
                    $"Chaining: \r\n" +
                    $"Any command may be chained into another, if the command chained into doesn't accept an argument, or the type doesn't match, an exception's thrown. \r\n" +
                    $"Chaining using the operator \".\" immediately executes the command with arguments, or if parameterized inlines it. \r\n" +
                    $"Chain using the operator \".>\" passes the command as itself \r\n" +
                    $"\r\n" +
                    $"Paramerization: \r\n" +
                    $"A command invocation can be parameterized be replacing an argument with a \"?\". \r\n" +
                    $"The resulting command will require an argument to be provided which will be substitute in it's place. \r\n" +
                    $"As many parameterization are allowed as arguments are required. \r\n" +
                    $"Chaining from a parameterized invocation does not immediately execute the invocation, but rather the resulting command takes arguments as are needed. \r\n" +
                    $"The same goes for an parameterized invocation used as an argument within the invocation parenthesis. \r\n" +
                    $"Arguments are substited in the order left to right that the parameterizations occur. \r\n" +
                    $"\r\n" +
                    $"General: \r\n" +
                    $"Literally everything is a command: literals, assignments, variable unwraps/invocations, invocations in general. \r\n" +
                    $"The function character \">\" means the command is passed as itself, otherwise it'll be immediately executed, or inlined if it's parameterized. \r\n";
            } // end Help()

            [Command]
            public static string Variables(CommandInterpreter self)
            {
                IEnumerable<Tuple<string, string?, string?>> rows = self.m_sessionContext.Variables.Select((x) => Tuple.Create(
                    $"${x.Name}",
                    x.Value?.GetType()?.ToString(),
                    x.Value?.ToString())).ToImmutableList();

                Tuple<int, int, int> maxLengths = rows.Count() >= 1
                    ? Tuple.Create(
                    rows.Select((x) => x.Item1.Length).Max() + 1,
                    rows.Select((x) => x.Item2?.Length ?? 4).Max(),
                    rows.Select((x) => x.Item3?.Length ?? 4).Max()
                    )
                    : Tuple.Create(0, 0, 0);

                string nameColumnName = "Name";
                string typeColumnName = "Type";
                string valueColumnName = "Value";

                maxLengths = Tuple.Create(
                    Math.Max(maxLengths.Item1, nameColumnName.Length),
                    Math.Max(maxLengths.Item2, typeColumnName.Length),
                    Math.Max(maxLengths.Item3, valueColumnName.Length)
                    );

                string formatter = $" {{0, -{maxLengths.Item1}}} | {{1, -{maxLengths.Item2}}} | {{2, -{maxLengths.Item3}}} ";
                string columnHeader = string.Format(formatter,
                    nameColumnName,
                    typeColumnName,
                    valueColumnName);

                StringBuilder sb = new();
                sb.AppendLine(columnHeader);
                sb.AppendLine(new string('-', columnHeader.Length));

                foreach (Tuple<string, string?, string?> row in rows)
                    sb.AppendLine(string.Format(formatter,
                        row.Item1,
                        row.Item2 ?? "null",
                        row.Item3 ?? "null"));

                return sb.ToString();
            } // end Variables()

            [Command]
            public static string Commands(CommandInterpreter self)
            {
                IEnumerable<Tuple<string, string, string[]>> rows = s_metaCommands.Commands
                    .Select((x) => Tuple.Create(
                    $"~{x.Name}",
                    x.ReturnType.ToString(),
                    x.ParameterTypes.Select((x) => x.ToString()).ToArray()))
                    .OrderBy((x) => x.Item1[1..])
                    .Concat(
                        self.m_commands.Commands
                        .Select((x) => Tuple.Create(
                        x.Name,
                        x.ReturnType.ToString(),
                        x.ParameterTypes.Select((x) => x.ToString()).ToArray()))
                        .OrderBy((x) => x.Item1)
                    ).ToImmutableList();

                int nameColumnMax = rows.Count() < 1 ? 0 : rows.Select((x) => x.Item1.Length).Max();
                int returnTypeColumnMax = rows.Count() < 1 ? 0 : rows.Select((x) => x.Item2.Length).Max();
                int parameterTypesColumnCountMax = rows.Count() < 1 ? 0 : rows.Select((x) => x.Item3.Length).Max();

                string nameColumnName = "Name";
                string returnTypeColumnName = "Return Type";
                string[] parameterTypeColumnNames = new string[parameterTypesColumnCountMax];

                for (int i = 0; i < parameterTypesColumnCountMax; i++)
                    parameterTypeColumnNames[i] = $"Paramater {i + 1} Type";

                nameColumnMax = Math.Max(nameColumnMax, nameColumnName.Length);
                returnTypeColumnMax = Math.Max(returnTypeColumnMax, returnTypeColumnName.Length);
                int[] parameterTypeColumnMaximums = new int[parameterTypesColumnCountMax];

                for (int i = 0; i < parameterTypesColumnCountMax; i++)
                {
                    IEnumerable<int> q = rows
                            .Where((x) => x.Item3.Length >= i + 1)
                            .Select((x) => x.Item3[i].Length);
                    parameterTypeColumnMaximums[i] = Math.Max(parameterTypeColumnNames[i].Length, q.Any() ? q.Max() : 0);
                }

                string formatter = $" {{0, -{nameColumnMax}}} | {{1, -{returnTypeColumnMax}}}";
                StringBuilder formatBuilder = new(formatter);
                for (int i = 0; i < parameterTypesColumnCountMax; i++)
                    formatBuilder.Append($" | {{{i + 2}, -{parameterTypeColumnMaximums[i]}}} ");

                formatter = formatBuilder.ToString();

                string columnHeader = string.Format(formatter,
                    new string[] { nameColumnName, returnTypeColumnName }.Union(parameterTypeColumnNames).ToArray());

                StringBuilder sb = new();
                sb.AppendLine(columnHeader);
                sb.AppendLine(new string('-', columnHeader.Length));

                foreach (Tuple<string, string, string[]> row in rows)
                {
                    string[] parameterTypes = new string[parameterTypesColumnCountMax].Select((x) => string.Empty).ToArray();
                    for (int i = 0; i < row.Item3.Length; i++)
                        parameterTypes[i] = row.Item3[i];

                    sb.AppendLine(string.Format(formatter,
                            new string[] { row.Item1, row.Item2 }.Concat(parameterTypes).ToArray()));
                }


                return sb.ToString();
            } // end Commands()

        } // end inner class

        private class ParameterizedCommand : Command
        {
            public class Parameterizer : ICommand
            {
                public static Parameterizer Inst = new();

                private Parameterizer() { } // end constructor

                public string Name => throw new NotImplementedException();

                public Type ReturnType => throw new NotImplementedException();

                public Type[] ParameterTypes => throw new NotImplementedException();

                public object? Execute(object?[] parameters)
                {
                    throw new NotImplementedException();
                } // end Execute()
            } // end inner class

            public ParameterizedCommand(string name, Type returnType, Type[] parameterTypes, Func<object?[], object?> func) : base(name, returnType, parameterTypes, func) { } // end constructor()
        } // end inner class

    } // end outer class
} // end namespace