namespace CorpseLib.Scripts
{
    public class FunctionStack
    {
        private object? m_ReturnValue = null;
        private bool m_HasReturn = false;

        public object? ReturnValue => m_ReturnValue;
        public bool HasReturn => m_HasReturn;

        public void Return() => m_HasReturn = true;

        public void Return(object? value)
        {
            m_HasReturn = true;
            m_ReturnValue = value;
        }
    }
}
