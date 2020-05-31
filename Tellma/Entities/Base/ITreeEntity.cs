namespace Tellma.Entities
{
    public interface IParentIndex
    {
        int? ParentIndex { get; set; }
    }

    public interface ITreeEntityForSave<TKey> : IParentIndex where TKey: struct
    {
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
