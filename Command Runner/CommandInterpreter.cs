﻿using System.Text.RegularExpressions;

namespace GSR.CommandRunner
{
    public class CommandInterpreter : ICommandInterpreter
    {
        private const string FUNCTION_ASSIGN_TYPE = "fa";
        private const string ASSIGN_TYPE = "a";
        private const string STRING_LITERAL_TYPE = "sl";
        private const string NUMERIC_LITERAL_TYPE = "nl";
        private const string VARIABLE_UNWRAP_TYPE = "v";

        private const string META_COMMAND_START_REGEX = @"^~\s*.\s*";
        private const string MEMBER_NAME_REGEX = @"^[_a-zA-Z][_0-9a-zA-Z]*";
        private const string NUMERIC_START_CHAR_REGEX = @"[-0-9]";
        private const string NUMERIC_REGEX = @"^-?[0-9]+([sil]|((.[0-9]+)?[fdm]))";
#warning, add more escapes;
        private static readonly IList<Tuple<string, string>> ESCAPE_REPLACEMENTS = new List<Tuple<string, string>>() { Tuple.Create(@"\\", @"\"), Tuple.Create(@"\""", @"""") };
        private static readonly IEnumerable<Tuple<string, string>> ESCAPE_REPLACEMENTS_R = ESCAPE_REPLACEMENTS.Select((x) => Tuple.Create(x.Item1.Replace(@"\", @"\\"), x.Item2.Replace(@"\", @"\\")));

        private static readonly string UNTIL_END_QUOTE_REGEX = @"^([^\\""]" + ESCAPE_REPLACEMENTS_R.Select((x) => x.Item1).Aggregate("|", (x, y) => $"{x}({y})|")[..^1] + @")*";
        
        private static readonly ICommandSet s_metaCommands = new CommandSet(typeof(CommandInterpreter));
        private readonly ICommandSet m_commands;
        private readonly  ISessionContext m_sessionContext;
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

            if (parse[0].Equals('$')) 
            {
                parse = parse[1..];
                string varName = Regex.Match(parse, MEMBER_NAME_REGEX).Value;
                parse = Regex.Replace(parse, MEMBER_NAME_REGEX, string.Empty).TrimStart();

                if (parse.Length >= 2 && parse[..2].Equals("=>"))
                {
                    parse = parse[2..].TrimStart();
                    ICommand val = ReadCommand(parse);
                    return CommandFor(FUNCTION_ASSIGN_TYPE, () => m_sessionContext.SetValue(varName, val)); 
                }
                else if (parse.Length >= 1 && parse[0].Equals('='))
                {
                    parse = parse[1..].TrimStart();
                    ICommand val = ReadCommand(parse);
                    return CommandFor(ASSIGN_TYPE, () => m_sessionContext.SetValue(varName, val.Execute(Array.Empty<object>())));
                }
                parse = $"${varName}{parse}";
            }
            
            return ReadCommand(parse);
        } // end Evaluate()


        private ICommand ReadCommand(string input, Type? chainedType = null, object? chainedOn = null) 
        {
            string parse = input.Trim();
            if (Regex.IsMatch(parse[..1], NUMERIC_START_CHAR_REGEX))
            {
                if (chainedType != null)
                    throw new InvalidOperationException("Can't chain to numeric literal.");

                return ReadNumericLiteral(input);
            }
            else if (parse[0].Equals('"'))
            {
                if (chainedType != null)
                    throw new InvalidOperationException("Can't chain to string literal.");

                return ReadStringLiteral(input);
            }
            else if (parse[0].Equals('$'))
            {
                parse = parse[1..];
                string varName = Regex.Match(parse, MEMBER_NAME_REGEX).Value;
                object? val = m_sessionContext.GetValue(varName, typeof(object));
                parse = Regex.Replace(parse, MEMBER_NAME_REGEX, string.Empty).TrimStart();


                // try return value, or if invoked holding command execute command.

                if (parse.Equals(string.Empty)) 
                    return CommandFor(VARIABLE_UNWRAP_TYPE, val?.GetType() ?? typeof(object),  () => val);
                else if (parse[0].Equals('('))
                {
                    // if command try to invoke
                }
                else if (parse[0].Equals('.'))
                {
                    parse = parse[1..].TrimStart();
                    return ReadCommand(parse, typeof(object), val);
                }
                else
                    throw new InvalidSyntaxException($"Unexpected character: \"{parse[0]}\", after variable: \"${varName}\"");
            }

            /*else if (Regex.IsMatch(parse, META_COMMAND_START_REGEX))
                {
                    parse = Regex.Replace(input, META_COMMAND_START_REGEX, "");
                    return ReadMetaCommand(parse);
                    string cName = Regex.Match(input, MEMBER_NAME_REGEX).Groups[0].Value;

                }*/
            return null;
        } // end ReadCommand()



        private ICommand ReadNumericLiteral(string input)
        {
            string parse = input.Trim();

            if (!Regex.IsMatch(parse[..1], NUMERIC_START_CHAR_REGEX))
                throw new InvalidOperationException($"{nameof(ReadStringLiteral)} should not be called with a value that's not starting with \'\"\'");

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
            
            if (parse.Equals(string.Empty)) 
                return CommandFor(NUMERIC_LITERAL_TYPE, value.GetType(), () => value);
            else if (parse[0].Equals('.'))
            {
                parse = parse[1..].TrimStart();
                return ReadCommand(parse, value.GetType(), value);
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
                return ReadCommand(parse, typeof(string), value);
            }
            else
                throw new InvalidSyntaxException($"Couldn't interpret value: \"{input}\"");
        } // end ReadStringLiteral



        private ICommand CommandFor(string type, Action value) => new Command($"{type}_{++m_uniqueNumber}", typeof(void), Array.Empty<Type>(), (x) => { value(); return null; });

        private ICommand CommandFor(string type, Type returnType, Func<object?> value) => new Command($"{type}_{++m_uniqueNumber}", returnType, Array.Empty<Type>(), (x) => value());

    } // end class
} // end namespace