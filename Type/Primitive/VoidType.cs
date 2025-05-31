namespace CorpseLib.Scripts.Type.Primitive
{
    public class VoidType : ARawPrimitive
    {
        public VoidType() : base() { }

        public override bool IsOfType(object[]? _) => false;

        public override object[]? Convert(object[] _) => null;
    }
}
