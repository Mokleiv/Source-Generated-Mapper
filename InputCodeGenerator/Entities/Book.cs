namespace InputCodeGenerator.Entities;

public sealed class Book
{
    public required string Name { get; set; }
    public required string Author { get; set; }
    public int PageCount { get; set; }
}