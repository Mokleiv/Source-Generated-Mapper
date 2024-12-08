using InputCodeGenerator.Inputs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var input = new UpdateBookInput(1, "The Hobbit", "J.R.R. Tolkien", 310);

var book = input.ToBook();
Console.WriteLine(book);

app.Run();