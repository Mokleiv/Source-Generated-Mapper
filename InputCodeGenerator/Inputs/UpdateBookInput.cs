using InputCodeGenerator.Attributes;
using InputCodeGenerator.Entities;

namespace InputCodeGenerator.Inputs;

[GenerateMapperInput<Book>]
public sealed partial record UpdateBookInput(int Id, string Name, string Author, int PageCount);