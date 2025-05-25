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

        public override object[]? Parse(string str)
        {
            if (str == "null")
                return [];
            if (str.Length <= 2 || str[0] != '[' || str[^1] != ']')
                throw new ArgumentException(string.Format("Invalid array string : \"{0}\"", str));
            str = str[1..^1];
            if (str.Length > 0 && str[^1] == ' ')
                str = str[..^1];
            if (str.Length > 0 && str[0] == ' ')
                str = str[1..];
            string[] elements;
            if (m_ElementType is ArrayType)
                elements = SplitArray(str);
            else if (m_ElementType is ARawPrimitive)
                elements = Shell.Helper.Split(str, ',');
            else
                elements = SplitObjects(str);
            List<Variable> variables = [];
            foreach (string element in elements)
                variables.Add(new(m_ElementType, m_ElementType.InternalParse(element), false));
            return variables.ToArray();
        }

        public override string ToString(object[]? value)
        {
            if (value == null)
                throw new ArgumentException("Array has no value");
            if (value.Length == 0)
                return "null";
            StringBuilder builder = new("[");
            for (int i = 0; i != value.Length; ++i)
            {
                if (i != 0)
                    builder.Append(',');
                builder.Append(' ');
                if (value[i] is Variable var)
                    builder.Append(m_ElementType.ToString(var.Values));
                else
                    throw new ArgumentException("Array is not valid");
            }
            if (value.Length != 0)
                builder.Append(' ');
            builder.Append(']');
            return builder.ToString();
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
