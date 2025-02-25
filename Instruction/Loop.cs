namespace CorpseLib.Scripts.Instruction
{
    public class Break : AInstruction { public override string ToScriptString(ConversionTable _) => string.Empty; protected override void Execute(Environment env, FunctionStack instructionStack) { } }

    public class Continue : AInstruction { public override string ToScriptString(ConversionTable _) => string.Empty; protected override void Execute(Environment env, FunctionStack instructionStack) { } }
}
