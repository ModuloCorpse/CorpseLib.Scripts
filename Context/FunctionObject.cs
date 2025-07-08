namespace CorpseLib.Scripts.Context
{
    public class FunctionObject(AFunction function, int[] tags, int[] comments) : EnvironmentObject(function.Signature.ID, tags, comments)
    {
        private readonly AFunction m_Function = function;

        public AFunction Function => m_Function;

        public override bool IsValid() => true;
    }
}
