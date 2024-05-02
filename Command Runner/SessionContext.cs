using System;

namespace GSR.CommandRunner
{
    public class SessionContext : ISessionContext
    {
        private IList<Variable> m_variables = new List<Variable>();



        public object GetValue(string name, Type type)
        {
            IEnumerable<Variable> q = m_variables.Where((x) => x.Name == name);
            if (!q.Any())
                throw new InvalidOperationException($"No varible found: {name}");

            Variable v = q.First();
            if (!(v.Type.IsSubclassOf(type) || v.Type == type))
                throw new InvalidOperationException($"Variable type mismatch, got: {v.Type}, expected: {type}");

            return v.Value;
        } // end GetValue()

        public void SetValue(string name, object value)
        {
            IEnumerable<Variable> q = m_variables.Where((x) => x.Name == name);
            if (q.Any())
                q.First().Update(value.GetType(), value);
            else
                m_variables.Add(new(name, value.GetType(), value));
        } // end SetValue()

    } // end class
} // end namespace
