namespace CorpseLib.Scripts.Memory
{
    public interface IMemoryObject : IMemoryValue
    {
        public IMemoryValue[] Properties { get; }
    }
}
