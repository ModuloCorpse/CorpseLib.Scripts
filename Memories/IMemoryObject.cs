namespace CorpseLib.Scripts.Memories
{
    public interface IMemoryObject : IMemoryValue
    {
        public IMemoryValue[] Properties { get; }
    }
}
