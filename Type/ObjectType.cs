using System.Text;

namespace CorpseLib.Scripts.Type
{
    public class ObjectType(TypeInfo typeInfo) : ATypeInstance(typeInfo)
    {
        private readonly List<Parameter> m_Attributes = [];

        public Parameter[] Attributes => [..m_Attributes];

        internal bool AddAttribute(Parameter attributeToAdd)
        {
            foreach (Parameter attribute in m_Attributes)
            {
                if (attribute.ID == attributeToAdd.ID)
                    return false;
            }
            m_Attributes.Add(attributeToAdd);
            return true;
        }

        private static Tuple<string, string> IsolateFirstElem(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == ',')
                {
                    string elem = str[..i];
                    string ret = str[(i + 1)..];
                    if (ret.Length > 0 && ret[0] == ' ')
                        ret = ret[1..];
                    return new(elem, ret);
                }
            }
            return new(str, string.Empty);
        }

        private static string[] SplitObject(string str)
        {
            if (str.Length <= 2 || str[0] != '{' || str[^1] != '}')
                throw new ArgumentException(string.Format("Invalid object string : \"{0}\"", str));
            str = str[1..^1];
            if (str.Length > 0 && str[^1] == ' ')
                str = str[..^1];
            if (str.Length > 0 && str[0] == ' ')
                str = str[1..];
            List<string> ret = [];
            while (!string.IsNullOrEmpty(str))
            {
                if (str[0] == '[')
                {
                    Tuple<string, string> split = ScriptParser.NextElement(str, '[', ']');
                    ret.Add(split.Item1);
                    str = split.Item2;
                }
                else if (str[0] == '{')
                {
                    Tuple<string, string> split = ScriptParser.NextElement(str, '{', '}');
                    ret.Add(split.Item1);
                    str = split.Item2;
                }
                else if (str[0] == '"')
                {
                    Tuple<string, string> split = ScriptParser.NextString(str);
                    ret.Add(split.Item1);
                    str = split.Item2;
                }
                else
                {
                    Tuple<string, string> split = IsolateFirstElem(str);
                    ret.Add(split.Item1);
                    str = split.Item2;
                }
            }
            return [.. ret];
        }

        public override object[]? Parse(string str)
        {
            if (str == "null")
                return [];
            string[] split = SplitObject(str);
            int i = 0;
            List<Variable> variables = [];
            foreach (Parameter attribute in m_Attributes)
            {
                if (i < split.Length)
                    variables.Add(m_Attributes[i].Instantiate(split[i]) ?? throw new ArgumentException("Invalid object"));
                else
                    variables.Add(m_Attributes[i].Instantiate() ?? throw new ArgumentException("Invalid object"));
                ++i;
            }
            return [..variables];
        }

        public override string ToString(object[]? value)
        {
            if (value == null)
                throw new ArgumentException("Object has no value");
            if (value.Length == 0)
                return "null";
            StringBuilder builder = new("{");
            for (int i = 0; i != value.Length; ++i)
            {
                if (value[i] is Variable var)
                {
                    if (!var.IsDefault())
                    {
                        if (i != 0)
                            builder.Append(',');
                        builder.Append(' ');
                        builder.Append(var.Type.ToString(var.Values));
                    }
                }
                else
                    throw new ArgumentException("Object is not valid");
            }
            if (value.Length != 0)
                builder.Append(' ');
            builder.Append('}');
            return builder.ToString();
        }

        public override bool IsOfType(object[]? value)
        {
            if (value == null)
                return false;
            int i = 0;
            foreach (Parameter attribute in m_Attributes)
            {
                if (value.Length < i)
                {
                    if (!attribute.Type.IsOfType([value[i]]))
                        return false;
                }
                else
                {
                    if (!attribute.HaveDefaultValue)
                        return false;
                }
                ++i;
            }
            return true;
        }
    }
}
