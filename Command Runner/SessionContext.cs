using System.Collections.Immutable;

namespace GSR.CommandRunner
{
    /// <summary>
    /// Basic ISessionContext implementation.
    /// </summary>
    public class SessionContext : ISessionContext
    {
        /// <inheritdoc/>
        public IList<Variable> Variables => m_variables.ToImmutableList();
        private readonly IList<Variable> m_variables = new List<Variable>();



        /// <inheritdoc/>
        public object? GetValue(string name)
        {
            IEnumerable<Variable> q = m_variables.Where((x) => x.Name == name);
            if (!q.Any())
                throw new UndefinedMemberException($"No variable found: {name}");

            return q.First().Value;
        } // end GetValue()

        /// <inheritdoc/>
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
