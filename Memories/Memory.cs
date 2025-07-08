using System.Collections;

namespace CorpseLib.Scripts.Memories
{
    public class Memory : IEnumerable<KeyValuePair<int, IMemoryValue>>, IEnumerable<StackVariable>
    {
        private readonly Heap m_Heap = new();
        private readonly Stack m_Stack = new();

        public void AddVariable(int id, ParameterType type, IMemoryValue value)
        {
            int memoryAddress = m_Heap.Allocate(value);
            StackVariable stackVariable = new(type, memoryAddress, false);
            m_Stack.AddVariable(id, stackVariable);
        }

        public bool PassByCopy(int from, int to)
        {
            StackVariable? fromStackVariable = m_Stack.GetPreviousVariable(from);
            if (fromStackVariable == null)
                return false;
            int copyAddress = m_Heap.Duplicate(fromStackVariable.MemoryAddress);
            StackVariable toStackVariable = new(fromStackVariable.Type, copyAddress, false);
            m_Stack.AddVariable(to, toStackVariable);
            return true;
        }

        public bool PassByReference(int from, int to)
        {
            StackVariable? fromStackVariable = m_Stack.GetPreviousVariable(from);
            if (fromStackVariable == null)
                return false;
            StackVariable toStackVariable = new(fromStackVariable.Type, fromStackVariable.MemoryAddress, true);
            m_Stack.AddVariable(to, toStackVariable);
            return true;
        }

        public IMemoryValue? GetVariable(int id)
        {
            StackVariable? stackVariable = m_Stack.GetVariable(id);
            if (stackVariable == null)
                return null;
            return m_Heap.Get(stackVariable.MemoryAddress);
        }

        public void OpenScope() => m_Stack.OpenScope();

        public void CloseScope()
        {
            List<StackVariable> toFree = m_Stack.CloseScope();
            foreach (StackVariable variable in toFree)
            {
                if (!variable.IsRef)
                    m_Heap.Free(variable.MemoryAddress);
            }
        }

        public void PushFunction() => m_Stack.PushFunction();

        public void PopFunction()
        {
            List<StackVariable> toFree = m_Stack.PopFunction();
            foreach (StackVariable variable in toFree)
            {
                if (!variable.IsRef)
                    m_Heap.Free(variable.MemoryAddress);
            }
        }

        public IMemoryValue? ReturnValue => m_Stack.ReturnValue;
        public bool HasReturn => m_Stack.HasReturn;

        public void Return() => m_Stack.Return();
        public void Return(IMemoryValue value) => m_Stack.Return(value);

        IEnumerator<KeyValuePair<int, IMemoryValue>> IEnumerable<KeyValuePair<int, IMemoryValue>>.GetEnumerator() => ((IEnumerable<KeyValuePair<int, IMemoryValue>>)m_Heap).GetEnumerator();
        IEnumerator<StackVariable> IEnumerable<StackVariable>.GetEnumerator() => ((IEnumerable<StackVariable>)m_Stack).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Heap).GetEnumerator();
    }
}
