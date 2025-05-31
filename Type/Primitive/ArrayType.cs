using System.Text;

namespace CorpseLib.Scripts.Type.Primitive
{
    public class ArrayType : ARawPrimitive
    {
        private readonly ATypeInstance m_ElementType;

        public ATypeInstance ElementType => m_ElementType;

        internal ArrayType(ATypeInstance arrayType) : base() => m_ElementType = arrayType;

        private static string[] SplitArray(string str)
        {
            List<string> ret = [];
            while (!string.IsNullOrEmpty(str))
            {
                Tuple<string, string> split = ScriptParser.NextElement(str, '[', ']');
                ret.Add(split.Item1);
                str = split.Item2;
            }
            return [.. ret];
        }

        private static string[] SplitObjects(string str)
        {
            List<string> ret = [];
            while (!string.IsNullOrEmpty(str))
            {
                Tuple<string, string> split = ScriptParser.NextElement(str, '{', '}');
                ret.Add(split.Item1);
                str = split.Item2;
            }
            return [.. ret];
        }

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
