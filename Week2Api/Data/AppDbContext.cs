using Microsoft.EntityFrameworkCore;
using Week2Api.Models;

namespace Week2Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "The Pragmatic Programmer", Author = "Andrew Hunt", Year = 1999, Isbn = "978-0201616224" },
            new Book { Id = 2, Title = "Clean Code", Author = "Robert C. Martin", Year = 2008, Isbn = "978-0132350884" }
        );
    }
}
