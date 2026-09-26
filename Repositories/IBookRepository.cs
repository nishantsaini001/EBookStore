namespace BooksShoppingProjectMVC
{
    public interface IBookRepository
    {
        Task AddBook(Book book);
        Task DeleteBook(Book book);

        Task<Book>? GetBookById(int Id);
        Task<IEnumerable<Book>> GetBooks();

        Task UpdateBook(Book book);
    }
}