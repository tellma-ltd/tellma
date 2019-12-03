namespace BSharp.Entities
{
    public interface ITreeEntityForSave<TKey> where TKey: struct
    {
        int? ParentIndex { get; set; }

        TKey? ParentId { get; set; }
    }

    public interface ITreeEntity<TKey> : ITreeEntityForSave<TKey> where TKey : struct
    {
        short? Level { get; set; }

        int? ActiveChildCount { get; set; }

        int? ChildCount { get; set; }

        HierarchyId Node { get; set; }

        HierarchyId ParentNode { get; set; }
    }
}
