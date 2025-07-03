using System.Diagnostics.CodeAnalysis;

namespace CorpseLib.Scripts.Type
{
    public class Types
    {
        public static readonly TypeInfo VOID_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("void"), [], 0);
        public static readonly TypeInfo BOOL_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("bool"), [], 0);
        public static readonly TypeInfo CHAR_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("char"), [], 0);
        public static readonly TypeInfo UCHAR_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("uchar"), [], 0);
        public static readonly TypeInfo SHORT_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("short"), [], 0);
        public static readonly TypeInfo USHORT_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("ushort"), [], 0);
        public static readonly TypeInfo INT_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("int"), [], 0);
        public static readonly TypeInfo UINT_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("uint"), [], 0);
        public static readonly TypeInfo LONG_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("long"), [], 0);
        public static readonly TypeInfo ULONG_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("ulong"), [], 0);
        public static readonly TypeInfo FLOAT_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("float"), [], 0);
        public static readonly TypeInfo DOUBLE_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("double"), [], 0);
        public static readonly TypeInfo STRING_TYPE_INFO = new(false, false, false, [], ConversionTable.ConvertStr("string"), [], 0);
        public static readonly VoidType VOID = new();
        public static readonly BoolType BOOL = new();
        public static readonly CharType CHAR = new();
        public static readonly UCharType UCHAR = new();
        public static readonly ShortType SHORT = new();
        public static readonly UShortType USHORT = new();
        public static readonly IntType INT = new();
        public static readonly UIntType UINT = new();
        public static readonly LongType LONG = new();
        public static readonly ULongType ULONG = new();
        public static readonly FloatType FLOAT = new();
        public static readonly DoubleType DOUBLE = new();
        public static readonly StringType STRING = new();
        internal static readonly ATypeInstance[] PRIMITIVE_TYPES = [VOID, BOOL, CHAR, UCHAR, SHORT, USHORT, INT, UINT, LONG, ULONG, FLOAT, DOUBLE, STRING];
        private static readonly Dictionary<int, int> m_Table = [];

        static Types()
        {
            m_Table.Add(VOID_TYPE_INFO.ID, 0);
            m_Table.Add(BOOL_TYPE_INFO.ID, 1);
            m_Table.Add(CHAR_TYPE_INFO.ID, 2);
            m_Table.Add(UCHAR_TYPE_INFO.ID, 3);
            m_Table.Add(SHORT_TYPE_INFO.ID, 4);
            m_Table.Add(USHORT_TYPE_INFO.ID, 5);
            m_Table.Add(INT_TYPE_INFO.ID, 6);
            m_Table.Add(UINT_TYPE_INFO.ID, 7);
            m_Table.Add(LONG_TYPE_INFO.ID, 8);
            m_Table.Add(ULONG_TYPE_INFO.ID, 9);
            m_Table.Add(FLOAT_TYPE_INFO.ID, 10);
            m_Table.Add(DOUBLE_TYPE_INFO.ID, 11);
            m_Table.Add(STRING_TYPE_INFO.ID, 12);
        }

        internal static bool IsPrimitive(TypeInfo type) => type == VOID_TYPE_INFO || type == BOOL_TYPE_INFO || type == CHAR_TYPE_INFO || type == UCHAR_TYPE_INFO || type == SHORT_TYPE_INFO || type == USHORT_TYPE_INFO || type == INT_TYPE_INFO || type == UINT_TYPE_INFO || type == LONG_TYPE_INFO || type == ULONG_TYPE_INFO || type == FLOAT_TYPE_INFO || type == DOUBLE_TYPE_INFO || type == STRING_TYPE_INFO;
        internal static bool IsNumber(TypeInfo type) => type == CHAR_TYPE_INFO || type == UCHAR_TYPE_INFO || type == SHORT_TYPE_INFO || type == USHORT_TYPE_INFO || type == INT_TYPE_INFO || type == UINT_TYPE_INFO || type == LONG_TYPE_INFO || type == ULONG_TYPE_INFO || type == FLOAT_TYPE_INFO || type == DOUBLE_TYPE_INFO;
        internal static bool IsIntegralNumber(TypeInfo type) => type == CHAR_TYPE_INFO || type == UCHAR_TYPE_INFO || type == SHORT_TYPE_INFO || type == USHORT_TYPE_INFO || type == INT_TYPE_INFO || type == UINT_TYPE_INFO || type == LONG_TYPE_INFO || type == ULONG_TYPE_INFO;
        internal static bool IsDecimalNumber(TypeInfo type) => type == FLOAT_TYPE_INFO || type == DOUBLE_TYPE_INFO;

        internal static bool TryGet(int type, [MaybeNullWhen(false)] out int instance)
        {
            if (m_Table.TryGetValue(type, out instance))
                return true;
            instance = -1;
            return false;
        }
    }
}
