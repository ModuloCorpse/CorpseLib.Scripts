﻿using CorpseLib.Scripts.Context;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public abstract class AInstruction
    {
        internal void ExecuteInstruction(Environment env, FunctionStack instructionStack) => Execute(env, instructionStack);

        protected abstract void Execute(Environment env, FunctionStack instructionStack);
    }
}
