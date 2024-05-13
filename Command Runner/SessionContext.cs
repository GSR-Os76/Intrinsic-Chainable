using System.Collections.Immutable;

namespace GSR.CommandRunner
{
    public class SessionContext : ISessionContext
    {
        public IList<Variable> Variables => m_variables.ToImmutableList();
        private readonly IList<Variable> m_variables = new List<Variable>();



        public object? GetValue(string name)
        {
            IEnumerable<Variable> q = m_variables.Where((x) => x.Name == name);
            if (!q.Any())
                throw new UndefinedMemberException($"No variable found: {name}");

            return q.First().Value;
        } // end GetValue()

        public void SetValue(string name, object? value)
        {
            IEnumerable<Variable> q = m_variables.Where((x) => x.Name == name);
            if (q.Any())
                q.First().Value = value;
            else
                m_variables.Add(new(name, value));
        } // end SetValue()

    } // end class
} // end namespace
