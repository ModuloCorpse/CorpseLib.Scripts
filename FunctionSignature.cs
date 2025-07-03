namespace CorpseLib.Scripts
{
    public class FunctionSignature(ParameterType returnType, int id, Parameter[] parameters)
    {
        private readonly ParameterType m_ReturnType = returnType;
        private readonly Parameter[] m_Parameters = parameters;
        private readonly int m_ID = id;

        public ParameterType ReturnType => m_ReturnType;
        public int ID => m_ID;
        public Parameter[] Parameters => m_Parameters;

        public override bool Equals(object? obj) => obj is FunctionSignature signature && EqualityComparer<Parameter[]>.Default.Equals(m_Parameters, signature.m_Parameters) && m_ReturnType == signature.m_ReturnType && m_ID == signature.m_ID;

        public override int GetHashCode() => HashCode.Combine(m_Parameters, m_ReturnType, m_ID);
    }
}
