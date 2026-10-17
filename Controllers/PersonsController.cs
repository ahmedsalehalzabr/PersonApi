using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonApi.Data;
using PersonApi.Models;

namespace PersonApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PersonsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Person>>> GetPersons()
    {
        return await _context.Persons.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Person>> GetPerson(int id)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null) return NotFound();
        return person;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePerson(Person person)
    {
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, null);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchPerson(int id, [FromBody] Person updatedPerson)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null) return NotFound();

        if (!string.IsNullOrEmpty(updatedPerson.FirstName))
            person.FirstName = updatedPerson.FirstName;
        if (!string.IsNullOrEmpty(updatedPerson.LastName))
            person.LastName = updatedPerson.LastName;
        if (updatedPerson.Email != null)
            person.Email = updatedPerson.Email;
        if (updatedPerson.DateOfBirth != default)
            person.DateOfBirth = updatedPerson.DateOfBirth;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePerson(int id)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null) return NotFound();

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
