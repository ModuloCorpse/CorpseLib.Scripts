using System.Collections;

namespace CorpseLib.Scripts.Memories
{
    public class Stack : IEnumerable<StackVariable>
    {
        private readonly List<FunctionStack> m_FunctionsStack = [];
        private readonly Dictionary<int, StackVariable> m_Variables = [];

        public AMemoryValue? ReturnValue => (m_FunctionsStack.Count > 0) ? m_FunctionsStack[^1].ReturnValue : null;
        public bool HasReturn => (m_FunctionsStack.Count > 0) && m_FunctionsStack[^1].HasReturn;

        public void Return()
        {
            if (m_FunctionsStack.Count > 0)
                m_FunctionsStack[^1].Return();
        }

        public void Return(AMemoryValue value)
        {
            if (m_FunctionsStack.Count > 0)
                m_FunctionsStack[^1].Return(value);
        }

        public void AddVariable(int id, StackVariable value)
        {
            if (m_FunctionsStack.Count > 0)
                m_FunctionsStack[^1].AddVariable(id, value);
            else
                m_Variables[id] = value;
        }

        public StackVariable? GetPreviousVariable(int id)
        {
            if (m_FunctionsStack.Count > 1)
            {
                StackVariable? stackRet = m_FunctionsStack[^2].GetVariable(id);
                if (stackRet != null)
                    return stackRet;
            }
            if (m_Variables.TryGetValue(id, out var value))
                return value;
            return null;
        }

        public StackVariable? GetVariable(int id)
        {
            if (m_FunctionsStack.Count > 0)
            {
                StackVariable? stackRet = m_FunctionsStack[^1].GetVariable(id);
                if (stackRet != null)
                    return stackRet;
            }
            if (m_Variables.TryGetValue(id, out var value))
                return value;
            return null;
        }

        public void OpenScope()
        {
            if (m_FunctionsStack.Count > 0)
                m_FunctionsStack[^1].OpenScope();
        }

        public List<StackVariable> CloseScope()
        {
            if (m_FunctionsStack.Count > 0)
                return m_FunctionsStack[^1].CloseScope();
            return [];
        }

        public void PushFunction() => m_FunctionsStack.Add(new FunctionStack());

        public List<StackVariable> PopFunction()
        {
            List<StackVariable> stackVariables = [];
            if (m_FunctionsStack.Count > 0)
            {
                stackVariables.AddRange(m_FunctionsStack[^1].Variables);
                m_FunctionsStack.RemoveAt(m_FunctionsStack.Count - 1);
            }
            return stackVariables;
        }

        public IEnumerator<StackVariable> GetEnumerator()
        {
            foreach (var variable in m_Variables.Values)
                yield return variable;
            if (m_FunctionsStack.Count > 0)
            {
                foreach (var variable in m_FunctionsStack[^1].AllVariables)
                    yield return variable;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
