using Microsoft.EntityFrameworkCore;
using Week2Api.Data;
using Week2Api.Models;

namespace Week2Api.Services;

/// <summary>
/// EF Core backed implementation of <see cref="IBookService"/>.
/// Uses async/await against the (in-memory) database.
/// </summary>
public class BookService(AppDbContext db) : IBookService
{
    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Books
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        book.Id = 0; // let the database assign the identity
        db.Books.Add(book);
        await db.SaveChangesAsync(cancellationToken);
        return book;
    }

    public async Task<bool> UpdateAsync(int id, Book book, CancellationToken cancellationToken = default)
    {
        var existing = await db.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (existing is null)
            return false;

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Year = book.Year;
        existing.Isbn = book.Isbn;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await db.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (existing is null)
            return false;

        db.Books.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
