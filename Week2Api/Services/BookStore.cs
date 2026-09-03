using System.Collections.Concurrent;
using Week2Api.Models;

namespace Week2Api.Services;

/// <summary>
/// Simple thread-safe in-memory store for books.
/// Registered as a singleton so data persists for the app's lifetime.
/// </summary>
public class BookStore
{
    private readonly ConcurrentDictionary<int, Book> _books = new();
    private int _nextId;

    public BookStore()
    {
        Add(new Book { Title = "The Pragmatic Programmer", Author = "Andrew Hunt", Year = 1999, Isbn = "978-0201616224" });
        Add(new Book { Title = "Clean Code", Author = "Robert C. Martin", Year = 2008, Isbn = "978-0132350884" });
    }

    public IEnumerable<Book> GetAll() => _books.Values.OrderBy(b => b.Id);

    public Book? Get(int id) => _books.TryGetValue(id, out var book) ? book : null;

    public Book Add(Book book)
    {
        book.Id = Interlocked.Increment(ref _nextId);
        _books[book.Id] = book;
        return book;
    }

    public bool Update(int id, Book updated)
    {
        if (!_books.ContainsKey(id))
            return false;

        updated.Id = id;
        _books[id] = updated;
        return true;
    }

    public bool Delete(int id) => _books.TryRemove(id, out _);
}
