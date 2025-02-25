﻿using CorpseLib.Scripts.Type;
using CorpseLib.Scripts.Type.Primitive;
using System.Text;

namespace CorpseLib.Scripts
{
    public class FunctionSignature(ATypeInstance returnType, int id, Parameter[] parameters)
    {
        private readonly ATypeInstance m_ReturnType = returnType;
        private readonly Parameter[] m_Parameters = parameters;
        private readonly int m_ID = id;

        public ATypeInstance ReturnType => m_ReturnType;
        public int ID => m_ID;
        public Parameter[] Parameters => m_Parameters;

        public string ToScriptString(ConversionTable conversionTable)
        {
            StringBuilder sb = new();
            sb.Append("fct ");
            if (m_ReturnType is not VoidType)
                sb.Append(string.Format("{0} ", m_ReturnType.Name));
            sb.Append(string.Format("{0}(", conversionTable.GetName(m_ID)));
            int i = 0;
            foreach (Parameter p in m_Parameters)
            {
                if (i != 0)
                    sb.Append(", ");
                sb.Append(p.ToScriptString(conversionTable));
                ++i;
            }
            sb.Append(')');
            return sb.ToString();
        }

        public override bool Equals(object? obj) => obj is FunctionSignature signature && EqualityComparer<Parameter[]>.Default.Equals(m_Parameters, signature.m_Parameters) && m_ReturnType == signature.m_ReturnType && m_ID == signature.m_ID;

        public override int GetHashCode() => HashCode.Combine(m_Parameters, m_ReturnType, m_ID);
    }
}
