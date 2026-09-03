using Week2Api.Models;

namespace Week2Api.Services;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(int id, Book book, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
