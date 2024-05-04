using System.Reflection;
using System.Text.RegularExpressions;

namespace GSR.CommandRunner
{
    public class MethodInfoCommand : ICommand
    {
        public string Code { get; }

        public Type ReturnType { get; }

        public Type[] Parameters { get; }



        public MethodInfoCommand(MethodInfo from) 
        {
            Code = $"{from.Name}({Regex.Replace(new string('_', from.GetParameters().Length), @"(?<=.)(?!$)", ", ")})";
        } // end constructor



        public object Execute(object[] parameters)
        {
            throw new NotImplementedException();
        }
    } // end class
} // end namespace