namespace CorpseLib.Scripts.Type.Primitive
{
    public class VoidType : ARawPrimitive
    {
        public VoidType() : base("void") { }

        public override object[]? Parse(string str) => null;

        public override string ToString(object[]? value) => string.Empty;
    }
}
