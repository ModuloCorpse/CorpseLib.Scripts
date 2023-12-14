using CorpseLib.Scripts.Type.Primitive;
using CorpseLib.Scripts.Type;
using System.Text;

namespace CorpseLib.Scripts
{
    public class Namespace(string name)
    {
        private readonly Namespace? m_Parent = null;
        private readonly Dictionary<string, TemplateDefinition> m_TemplateDefinitions = [];
        private readonly Dictionary<string, ATypeInstance> m_TemplateTypeInstances = [];
        private readonly Dictionary<string, ATypeInstance> m_TypeInstances = [];
        private readonly Dictionary<string, AFunction> m_Functions = [];
        private readonly Dictionary<string, Parameter> m_Globals = [];
        private readonly Dictionary<string, Namespace> m_Namespaces = [];
        private readonly string m_Name = name;

        public string GetName()
        {
            string parentName = string.Empty;
            if (m_Parent != null)
                parentName = m_Parent.GetName();
            if (!string.IsNullOrEmpty(parentName))
                return string.Format("{0}.{1}", parentName, m_Name);
            return m_Name;
        }

        public TemplateDefinition[] Definitions => m_TemplateDefinitions.Values.ToArray();
        public ATypeInstance[] TemplateTypeInstances => m_TemplateTypeInstances.Values.ToArray();
        public ATypeInstance[] Instances => m_TypeInstances.Values.ToArray();
        public AFunction[] Functions => m_Functions.Values.ToArray();
        public Parameter[] Globals => m_Globals.Values.ToArray();
        public Namespace[] Namespaces => m_Namespaces.Values.ToArray();

        public Namespace(string name, Namespace parent) : this(name) => m_Parent = parent;

        public void AddTemplateDefinition(TemplateDefinition type) => m_TemplateDefinitions[type.Name] = type;

        public void AddType(ATypeInstance type) => m_TypeInstances[type.Name] = type;

        internal void AddTemplateTypeInstance(string name, ATypeInstance type) => m_TemplateTypeInstances[name] = type;
        internal void RemoveTemplateTypeInstance(string name) => m_TemplateTypeInstances.Remove(name);

        public bool AddFunction(AFunction function)
        {
            if (m_Functions.ContainsKey(function.Signature.Name))
                return false;
            m_Functions[function.Signature.Name] = function;
            return true;
        }

        public bool AddGlobal(Parameter parameter)
        {
            if (m_Globals.ContainsKey(parameter.Name))
                return false;
            m_Globals[parameter.Name] = parameter;
            return true;
        }

        public bool AddNamespace(string namespaceName, Namespace @namespace)
        {
            if (m_Namespaces.ContainsKey(namespaceName))
                return false;
            m_Namespaces[namespaceName] = @namespace;
            return true;
        }

        private ATypeInstance? InternalInstantiate(string type)
        {
            bool isConst = false;
            string typeName = type;
            if (typeName.StartsWith("const "))
            {
                typeName = typeName[6..];
                isConst = true;
            }
            if (typeName.Contains('.'))
            {
                int idx = typeName.LastIndexOf('.');
                string[] namespaces = typeName[..idx].Split('.');
                typeName = typeName[(idx + 1)..];
                string namespaceToSearch = namespaces[0];
                namespaces = namespaces.Skip(1).ToArray();
                if (m_Namespaces.TryGetValue(namespaceToSearch, out Namespace? @namespace))
                {
                    StringBuilder builder = new();
                    int i = 0;
                    foreach (string ns in namespaces)
                    {
                        if (i != 0)
                            builder.Append('.');
                        builder.Append(ns);
                        ++i;
                    }
                    if (i != 0)
                        builder.Append('.');
                    builder.Append(typeName);
                    return @namespace.InternalInstantiate(builder.ToString());
                }
                return null;
            }
            if (typeName.EndsWith("[]"))
            {
                ATypeInstance? arrayType = Instantiate(typeName[..^2]);
                if (arrayType != null)
                    return new ArrayType(arrayType);
                return null;
            }
            else if (typeName.Contains('<'))
            {
                OperationResult<Tuple<string, string[]>> tupleResult = TemplateDefinition.ExtractTypeName(typeName);
                if (!tupleResult)
                {
                    return null;
//                    return new(tupleResult.Error, tupleResult.Description);
                }
                string typeToSearch = tupleResult.Result!.Item1;
                string[] templates = tupleResult.Result!.Item2;
                if (m_TemplateTypeInstances.TryGetValue(typeName.Replace(" ", ""), out ATypeInstance? templateObjectInstance))
                    return templateObjectInstance;
                else if (m_TypeInstances.TryGetValue(typeName.Replace(" ", ""), out ATypeInstance? objectInstance))
                    return objectInstance;
                else if (m_TemplateDefinitions.TryGetValue(typeToSearch, out TemplateDefinition? ret))
                    return ret.Instantiate(isConst, templates, this);
                return null;
            }
            else if (Types.TryGet(typeName, out ATypeInstance? primitiveInstance))
                return primitiveInstance;
            else if (m_TypeInstances.TryGetValue(typeName, out ATypeInstance? objectInstance))
                return objectInstance;
            return null;
        }

        public ATypeInstance? Instantiate(string type)
        {
            ATypeInstance? instance = InternalInstantiate(type);
            if (instance != null)
                return instance;
            return m_Parent?.Instantiate(type);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new("namespace ");
            stringBuilder.Append(m_Name);
            stringBuilder.Append(" { ");
            //TODO
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }
    }
}
