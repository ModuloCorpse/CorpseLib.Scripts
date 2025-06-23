namespace CorpseLib.Scripts.Context
{
    public class Signature(int[] namespaces, int id) : IEquatable<Signature?>
    {
        public readonly int[] Namespaces = namespaces;
        public readonly int ID = id;

        public override bool Equals(object? obj) => Equals(obj as Signature);
        public bool Equals(Signature? other) => other is not null && EqualityComparer<int[]>.Default.Equals(Namespaces, other.Namespaces) && ID == other.ID;
        public override int GetHashCode() => HashCode.Combine(Namespaces, ID);
        public static bool operator ==(Signature? left, Signature? right) => EqualityComparer<Signature>.Default.Equals(left, right);
        public static bool operator !=(Signature? left, Signature? right) => !(left == right);
    }
}
