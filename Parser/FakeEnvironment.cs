using CorpseLib.Scripts.Type;

namespace CorpseLib.Scripts.Parser
{
    public class FakeEnvironment
    {
        private readonly FakeEnvironment? m_Parent = null;
        private readonly Dictionary<string, ATypeInstance> m_Variables = [];

        public FakeEnvironment() { }
        public FakeEnvironment(FakeEnvironment parent) => m_Parent = parent;

        public void AddVariableType(string name, ATypeInstance value) => m_Variables[name] = value;

        public ATypeInstance? GetVariableType(string name, ConversionTable conversionTable)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            string[] param = name.Split('.');
            if (m_Variables.TryGetValue(param[0], out var type))
            {
                for (int i = 1; i != param.Length; ++i)
                {
                    if (type is ObjectType objectType)
                    {
                        bool hasFoundAttribute = false;
                        foreach (Parameter parameter in objectType.Attributes)
                        {
                            if (parameter.ID == conversionTable.PushName(name))
                            {
                                type = parameter.Type;
                                hasFoundAttribute = true;
                            }
                        }
                        if (!hasFoundAttribute)
                            return null;
                    }
                    else
                        return null;
                }
                return type;
            }
            return m_Parent?.GetVariableType(name, conversionTable);
        }
    }
}
