using Microsoft.AspNetCore.Mvc;
using Week2Api.Models;
using Week2Api.Services;

namespace Week2Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BookStore _store;

    public BooksController(BookStore store)
    {
        _store = store;
    }

    // GET: api/books
    [HttpGet]
    public ActionResult<IEnumerable<Book>> GetAll()
    {
        return Ok(_store.GetAll());
    }

    // GET: api/books/5
    [HttpGet("{id:int}")]
    public ActionResult<Book> GetById(int id)
    {
        var book = _store.Get(id);
        if (book is null)
            return NotFound();

        return Ok(book);
    }

    // POST: api/books
    [HttpPost]
    public ActionResult<Book> Create(Book book)
    {
        var created = _store.Add(book);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/books/5
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Book book)
    {
        if (!_store.Update(id, book))
            return NotFound();

        return NoContent();
    }

    // DELETE: api/books/5
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_store.Delete(id))
            return NotFound();

        return NoContent();
    }
}
