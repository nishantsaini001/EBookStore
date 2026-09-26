using Microsoft.EntityFrameworkCore;

namespace BooksShoppingProjectMVC.Repositories
{
    public class BookRepository :IBookRepository
    {
        private readonly ApplicationDbContext _context;
        public BookRepository(ApplicationDbContext context) {

            _context = context;

        }

        public async Task AddBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteBook(Book book)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateBook(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task<Book>? GetBookById(int Id) => await _context.Books.FindAsync(Id);

        public async Task<IEnumerable<Book>> GetBooks() => await _context.Books.Include(a=>a.Genre).ToListAsync();
    }
}
