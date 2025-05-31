namespace CorpseLib.Scripts.Context
{
    public class NamespaceObject(int id, int[] tags, int[] comments) : EnvironmentObject(id, tags, comments)
    {
        private readonly Dictionary<int, EnvironmentObject> m_Objects = [];

        public override bool IsValid() => m_Objects.Count > 0;

        public void ClearInvalid()
        {
            List<int> invalidKeys = [];
            foreach (var obj in m_Objects.Values)
            {
                if (obj is NamespaceObject childNamespaceObject)
                {
                    childNamespaceObject.ClearInvalid();
                    if (!childNamespaceObject.IsValid())
                        invalidKeys.Add(childNamespaceObject.ID);
                }
                else if (!obj.IsValid())
                    invalidKeys.Add(obj.ID);
            }
            foreach (var key in invalidKeys)
                m_Objects.Remove(key);
        }
    }
}
