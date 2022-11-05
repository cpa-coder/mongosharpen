namespace MongoSharpen.Test.Dtos;

public class BookDto
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string ISBN { get; set; }
    public IEnumerable<AuthorDto> Authors { get; set; }
}