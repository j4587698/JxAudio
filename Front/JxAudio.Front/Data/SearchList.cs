namespace JxAudio.Front.Data;

public class SearchList
{
    public int Id { get; set; }
    
    public string? Name { get; set; }

    public int? CoverId { get; set; }

    public SearchType SearchType { get; set; }
}

public enum SearchType
{
    Artist,
    Album,
    Track
}