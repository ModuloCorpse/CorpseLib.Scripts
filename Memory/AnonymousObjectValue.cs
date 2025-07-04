using CorpseLib.Scripts.Type;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Memory
{
    public class AnonymousObjectValue(IMemoryValue[] value) : IMemoryObject
    {
        private readonly IMemoryValue[] m_Value = value;

        public IMemoryValue[] Properties => m_Value;

        public bool Equals(IMemoryValue? other)
        {
            if (other is AnonymousObjectValue anonymousObjectValue)
                return EqualityComparer<IMemoryValue[]>.Default.Equals(m_Value, anonymousObjectValue.m_Value);
            return false;
        }

        public IMemoryValue Clone()
        {
            IMemoryValue[] clonedValues = new IMemoryValue[m_Value.Length];
            for (int i = 0; i < m_Value.Length; i++)
                clonedValues[i] = m_Value[i].Clone();
            return new AnonymousObjectValue(clonedValues);
        }

        public ObjectValue? Convert(Environment env, ObjectType type)
        {
            ObjectValue objectValue = new();
            Parameter[] attributes = type.Attributes;
            for (int i = 0; i < attributes.Length; i++)
            {
                Parameter attribute = attributes[i];
                if (i < m_Value.Length)
                {
                    if (m_Value[i] is AnonymousObjectValue anonymousValue)
                    {
                        ATypeInstance? attributeType = env.GetTypeInstance(attribute.TypeID);
                        if (attributeType == null || attributeType is not ObjectType objectType)
                            return null; // Invalid type for conversion
                        IMemoryObject? convertedValue = anonymousValue.Convert(env, objectType);
                        if (convertedValue == null)
                            return null; // Conversion failed
                        objectValue.Set(attribute.ID, convertedValue);
                    }
                    else if (m_Value[i] is IMemoryValue memoryValue)
                        objectValue.Set(attribute.ID, m_Value[i].Clone());
                }
                else
                {
                    IMemoryValue? defaultValue = attribute.DefaultValue;
                    if (defaultValue == null)
                        return null; // Missing required attribute
                    objectValue.Set(attribute.ID, defaultValue.Clone());
                }
            }
            return objectValue;
        }
    }
}
