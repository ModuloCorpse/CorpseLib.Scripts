using CorpseLib.Scripts.Parser;
using System.Text;

namespace CorpseLib.Scripts.Type.Primitive
{
    public class ArrayType : ARawPrimitive
    {
        private readonly ATypeInstance m_ElementType;

        public ATypeInstance ElementType => m_ElementType;

        internal ArrayType(ATypeInstance arrayType) : base() => m_ElementType = arrayType;

        public override object[]? Convert(object[] value)
        {
            if (value.Length == 0)
                return [];
            if (value.Length != 1)
                return null;
            if (value[0] is not List<object[]> elements)
                return null;
            List<Variable> variables = [];
            foreach (object[] element in elements)
                variables.Add(new(m_ElementType, m_ElementType.InternalConvert(element), false));
            return variables.ToArray();
        }

        public override bool IsOfType(object[]? value)
        {
            if (value == null)
                return false;
            foreach (object element in value)
            {
                if (!m_ElementType.IsOfType([element]))
                    return false;
            }
            return true;
        }
    }
}
