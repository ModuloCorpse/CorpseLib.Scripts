namespace CorpseLib.Scripts.Memories
{
    public interface IMemoryArray : IMemoryValue
    {
        public IMemoryValue this[int index] { get; set; }
        public int Length { get; }
    }
}
