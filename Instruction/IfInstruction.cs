namespace CorpseLib.Scripts.Instruction
{
    public class IfInstruction(Condition condition, List<AInstruction> body) : AConditionalInstruction(condition, body)
    {
        private readonly List<IfInstruction> m_Elifs = [];
        private readonly ScopedInstructions m_ElseBody = new();
        public ScopedInstructions ElseBody => m_ElseBody;
        public IfInstruction[] Elifs => [..m_Elifs];

        internal void AddElif(Condition condition, List<AInstruction> body) => m_Elifs.Add(new(condition, body));
        internal void SetElseBody(List<AInstruction> body) => m_ElseBody.AddInstructions(body);

        private ScopedInstructions GetInstructions(Frame frame, FunctionStack functionStack)
        {
            if (EvaluateCondition(frame, functionStack))
                return Body;
            foreach (var elif in m_Elifs)
            {
                if (elif.EvaluateCondition(frame, functionStack))
                    return elif.Body;
            }
            if (!m_ElseBody.IsEmpty)
                return m_ElseBody;
            return new();
        }

        protected override void Execute(Frame env, FunctionStack functionStack)
        {
            ScopedInstructions instructions = GetInstructions(env, functionStack);
            if (instructions.IsEmpty)
                return;
            _ = instructions.Execute(env, functionStack);
        }
    }
}
