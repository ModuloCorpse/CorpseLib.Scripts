﻿using CorpseLib.Scripts.Memories;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public class ScopedInstructions
    {
        public enum EExecutionResult
        {
            Continued,
            Breaked,
            Returned,
            None
        }

        private readonly List<AInstruction> m_Instructions;

        public ScopedInstructions(List<AInstruction> instructions) => m_Instructions = instructions;
        public ScopedInstructions() => m_Instructions = [];

        public AInstruction[] Instructions => [..m_Instructions];
        public bool IsEmpty => m_Instructions.Count == 0;
        public int Count => m_Instructions.Count;

        public void AddInstruction(AInstruction instruction) => m_Instructions.Add(instruction);
        public void AddInstructions(IEnumerable<AInstruction> instructions) => m_Instructions.AddRange(instructions);

        public EExecutionResult Execute(Environment env, Memory memory)
        {
            memory.OpenScope();
            foreach (AInstruction instruction in m_Instructions)
            {
                if (instruction is Break)
                {
                    memory.CloseScope();
                    return EExecutionResult.Breaked;
                }
                else if (instruction is Continue)
                {
                    memory.CloseScope();
                    return EExecutionResult.Continued;
                }
                else
                {
                    instruction.ExecuteInstruction(env, memory);
                    if (memory.HasReturn)
                    {
                        memory.CloseScope();
                        return EExecutionResult.Returned;
                    }
                }
            }
            memory.CloseScope();
            return EExecutionResult.None;
        }
    }
}
