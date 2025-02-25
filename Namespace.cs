using CorpseLib.Scripts.Type.Primitive;
using CorpseLib.Scripts.Type;
using System.Text;

namespace CorpseLib.Scripts
{
    public class Namespace(int id)
    {
        private readonly Namespace? m_Parent = null;
        //TODO Change to hashes to reduce memory usage
        private readonly Dictionary<string, TemplateDefinition> m_TemplateDefinitions = [];
        private readonly Dictionary<int, ATypeInstance> m_TemplateTypeInstances = [];
        private readonly Dictionary<int, ATypeInstance> m_TypeInstances = [];
        private readonly Dictionary<int, AFunction> m_Functions = [];
        private readonly Dictionary<int, Parameter> m_Globals = [];
        private readonly Dictionary<int, Namespace> m_Namespaces = [];
        private readonly int m_ID = id;

        public int ID => m_ID;

        public string GetName(ConversionTable conversionTable)
        {
            string parentName = string.Empty;
            if (m_Parent != null)
                parentName = m_Parent.GetName(conversionTable);
            if (!string.IsNullOrEmpty(parentName))
                return string.Format("{0}.{1}", parentName, conversionTable.GetName(m_ID));
            return conversionTable.GetName(m_ID);
        }

        public TemplateDefinition[] Definitions => [.. m_TemplateDefinitions.Values];
        public ATypeInstance[] TemplateTypeInstances => [.. m_TemplateTypeInstances.Values];
        public ATypeInstance[] Instances => [.. m_TypeInstances.Values];
        public AFunction[] Functions => [.. m_Functions.Values];
        public Parameter[] Globals => [.. m_Globals.Values];
        public Namespace[] Namespaces => [.. m_Namespaces.Values];

        public Namespace(int id, Namespace parent) : this(id) => m_Parent = parent;

        public void AddTemplateDefinition(TemplateDefinition type) => m_TemplateDefinitions[type.Name] = type;

        public void AddType(ATypeInstance type) => m_TypeInstances[type.ID] = type;

        internal void AddTemplateTypeInstance(int id, ATypeInstance type) => m_TemplateTypeInstances[id] = type;
        internal void RemoveTemplateTypeInstance(int id) => m_TemplateTypeInstances.Remove(id);

        public bool AddFunction(AFunction function)
        {
            if (m_Functions.ContainsKey(function.Signature.ID))
                return false;
            m_Functions[function.Signature.ID] = function;
            return true;
        }

        public bool AddGlobal(Parameter parameter)
        {
            if (m_Globals.ContainsKey(parameter.ID))
                return false;
            m_Globals[parameter.ID] = parameter;
            return true;
        }

        public bool AddNamespace(Namespace @namespace)
        {
            if (m_Namespaces.ContainsKey(@namespace.ID))
                return false;
            m_Namespaces[@namespace.ID] = @namespace;
            return true;
        }

        private Namespace? SearchNamespace(int namespaceID)
        {
            if (m_ID == namespaceID)
                return this;
            if (m_Namespaces.TryGetValue(namespaceID, out Namespace? ns))
                return ns;
            if (m_Parent != null)
            {
                if (m_Parent.ID == namespaceID)
                    return m_Parent;
                return m_Parent.SearchNamespace(namespaceID);
            }
            return null;
        }

        private ATypeInstance? Intantiate(ParsedType parsedType)
        {
            Namespace @namespace = this;
            if (parsedType.NamespacesID.Length > 0)
            {
                Namespace? searchedNamespace = SearchNamespace(parsedType.NamespacesID[0]);
                if (searchedNamespace == null)
                    return null;
            }
            return null;
        }


        private ATypeInstance? InternalInstantiate(string typeName, bool isConst)
        {
            //TODO Rework to use a ParsedType class that will store all information of the type
            if (typeName.Contains('.'))
            {
                int idx = typeName.IndexOf('.');
                int namespaceToSearch = typeName[..idx].GetHashCode();
                string typeToSearch = typeName[(idx + 1)..];
                if (m_Namespaces.TryGetValue(namespaceToSearch, out Namespace? @namespace))
                    return @namespace.InternalInstantiate(typeToSearch, isConst);
                return m_Parent?.InternalInstantiate(typeName, isConst);
            }
            if (typeName.EndsWith("[]"))
            {
                ATypeInstance? arrayType = InternalInstantiate(typeName[..^2], isConst);
                if (arrayType != null)
                    return new ArrayType(arrayType);
                return m_Parent?.InternalInstantiate(typeName, isConst);
            }
            else if (typeName.Contains('<'))
            {
                OperationResult<Tuple<string, string[]>> tupleResult = TemplateDefinition.ExtractTypeName(typeName);
                if (!tupleResult)
                    return null;
                string typeToSearch = tupleResult.Result!.Item1;
                string[] templates = tupleResult.Result!.Item2;
                if (m_TemplateTypeInstances.TryGetValue(typeName.Replace(" ", "").GetHashCode(), out ATypeInstance? templateObjectInstance))
                    return templateObjectInstance;
                else if (m_TypeInstances.TryGetValue(typeName.Replace(" ", "").GetHashCode(), out ATypeInstance? objectInstance))
                    return objectInstance;
                else if (m_TemplateDefinitions.TryGetValue(typeToSearch, out TemplateDefinition? ret))
                    return ret.Instantiate(isConst, templates, this);
                return m_Parent?.InternalInstantiate(typeName, isConst);
            }
            else if (Types.TryGet(typeName, out ATypeInstance? primitiveInstance))
                return primitiveInstance;
            else if (m_TypeInstances.TryGetValue(typeName.GetHashCode(), out ATypeInstance? objectInstance))
                return objectInstance;
            return m_Parent?.InternalInstantiate(typeName, isConst);
        }

        public ATypeInstance? Instantiate(string type)
        {
            if (type.StartsWith("const "))
                return InternalInstantiate(type[6..], true);
            return InternalInstantiate(type, false);
        }

        public string ToScriptString(ConversionTable conversionTable)
        {
            StringBuilder stringBuilder = new("namespace ");
            stringBuilder.Append(conversionTable.GetName(m_ID));
            stringBuilder.Append(" { ");
            //TODO
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }
    }
}
