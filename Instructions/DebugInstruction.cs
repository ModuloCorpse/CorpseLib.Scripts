using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Parser.Instruction.Expressions;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public class DebugInstruction(AExpression expression, string instruction) : AInstruction
    {
        private readonly AExpression m_Expression = expression;
        private readonly string m_Instruction = instruction;

        public AExpression Expression => m_Expression;
        public string Instruction => m_Instruction;

        protected override void Execute(Environment env, Memory memory) => Console.WriteLine(m_Instruction);
        public void Dump(ConversionTable conversionTable) => m_Expression.Dump(conversionTable);
    }
}
