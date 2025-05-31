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

        private bool CheckObject(object[] value)
        {
            foreach (object obj in value)
            {
                if (obj is not object[])
                    return false;
            }
            return true;
        }

        public override object[]? Convert(object[] str)
        {
            if (str.Length == 0)
                return [];
            if (!CheckObject(str))
                return null;
            int i = 0;
            List<Variable> variables = [];
            foreach (Parameter attribute in m_Attributes)
            {
                if (i < str.Length)
                    variables.Add(m_Attributes[i].Convert((object[])str[i]) ?? throw new ArgumentException("Invalid object"));
                else
                    variables.Add(m_Attributes[i].Instantiate() ?? throw new ArgumentException("Invalid object"));
                ++i;
            }
            return [..variables];
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
