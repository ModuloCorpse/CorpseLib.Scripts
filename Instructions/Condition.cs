namespace CorpseLib.Scripts.Instructions
{
    public class Condition(string condition)
    {
        private readonly string m_Condition = condition;
        public string ConditionStr => m_Condition;
    }
}
