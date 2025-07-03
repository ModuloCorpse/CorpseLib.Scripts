using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class EqualityOperationNode(bool isNot) : AOperationTreeNode
    {
        private readonly bool m_IsNot = isNot;

        public bool IsNot => m_IsNot;

        internal override bool IsBooleanOperation => true;

        private static bool Equal(object[] left, object[] right)
        {
            if (left.Length != right.Length)
                return false;
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] is object[] subLeft)
                {
                    if (right[i] is not object[] subRight || !Equal(subLeft, subRight))
                        return false;
                }
                else if (left[i] is List<object[]> subLeftList)
                {
                    if (right[i] is not List<object[]> subRightList || subLeftList.Count != subRightList.Count)
                        return false;
                    for (int j = 0; j < subLeftList.Count; j++)
                    {
                        if (!Equal(subLeftList[j], subRightList[j]))
                            return false;
                    }
                }
                else if (!left[i].Equals(right[i]))
                    return false;
            }
            return true;
        }

        protected override object[] Execute(Environment env, FunctionStack functionStack)
        {
            object[] leftValue = m_Children[0].CallOperation(env, functionStack);
            object[] rightValue = m_Children[1].CallOperation(env, functionStack);
            if (m_IsNot)
                return [!Equal(leftValue, rightValue)];
            return [Equal(leftValue, rightValue)];
        }

        protected override bool IsValid(ParsingContext _, string instructionStr) => true;
    }
}
