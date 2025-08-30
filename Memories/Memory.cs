using CorpseLib.Scripts.Parameters;
using System.Collections;

namespace CorpseLib.Scripts.Memories
{
    public class Memory : IEnumerable<Heap.MemorySlot>, IEnumerable<StackVariable>
    {
        private readonly Heap m_Heap = new();
        private readonly Stack m_Stack = new();

        internal Heap Heap => m_Heap;

        public void AddVariable(int id, ParameterType type, AMemoryValue value)
        {
            if (type.IsRef || value.Address == -1)
                m_Stack.AddVariable(id, new(type, m_Heap.Allocate(value), false));
            else
                m_Stack.AddVariable(id, new(type, m_Heap.Duplicate(value.Address), false));
        }

        public AMemoryValue? GetMemoryValue(int address) => m_Heap.Get(address);

        public AMemoryValue? GetVariable(int id)
        {
            StackVariable? stackVariable = m_Stack.GetVariable(id);
            if (stackVariable == null)
                return null;
            return GetMemoryValue(stackVariable.MemoryAddress);
        }

        public void SetVariable(int id, AMemoryValue value)
        {
            StackVariable? stackVariable = m_Stack.GetVariable(id);
            if (stackVariable == null)
                return;
            int address = m_Heap.Allocate(value);
            if (stackVariable.IsRef)
                stackVariable.MemoryAddress = address;
            else
            {
                int newAddress = m_Heap.Duplicate(address);
                stackVariable.MemoryAddress = newAddress;
            }
        }

        public void OpenScope() => m_Stack.OpenScope();

        public void CloseScope()
        {
            List<StackVariable> toFree = m_Stack.CloseScope();
            foreach (StackVariable variable in toFree)
                m_Heap.Free(variable.MemoryAddress);
        }

        public void PushFunction() => m_Stack.PushFunction();

        public void PopFunction()
        {
            List<StackVariable> toFree = m_Stack.PopFunction();
            foreach (StackVariable variable in toFree)
                m_Heap.Free(variable.MemoryAddress);
        }

        public AMemoryValue? ReturnValue => m_Stack.ReturnValue;
        public bool HasReturn => m_Stack.HasReturn;

        public void Return() => m_Stack.Return();
        public void Return(AMemoryValue value) => m_Stack.Return(value);

        IEnumerator<Heap.MemorySlot> IEnumerable<Heap.MemorySlot>.GetEnumerator() => ((IEnumerable<Heap.MemorySlot>)m_Heap).GetEnumerator();
        IEnumerator<StackVariable> IEnumerable<StackVariable>.GetEnumerator() => ((IEnumerable<StackVariable>)m_Stack).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Heap).GetEnumerator();
    }
}
