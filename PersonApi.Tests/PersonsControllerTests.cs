using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonApi.Controllers;
using PersonApi.Data;
using PersonApi.Models;
using Xunit;

namespace PersonApi.Tests;

public class PersonsControllerTests
{
    private static AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetPerson_ReturnsNotFound_WhenIdDoesNotExist()
    {
        using var context = GetInMemoryDbContext();
        var controller = new PersonsController(context);
        var result = await controller.GetPerson(999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreatePerson_ReturnsCreatedAtAction_WithEmptyBodyAndLocation()
    {
        using var context = GetInMemoryDbContext();
        var controller = new PersonsController(context);
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await controller.CreatePerson(person);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Null(createdAtActionResult.Value);
        Assert.Equal("GetPerson", createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.RouteValues);
        Assert.Equal(person.Id, createdAtActionResult.RouteValues!["id"]);
    }

    [Fact]
    public async Task GetPersons_ReturnsAllPersons()
    {
        using var context = GetInMemoryDbContext();
        context.Persons.Add(new Person { FirstName = "A", LastName = "B", DateOfBirth = DateTime.UtcNow });
        context.Persons.Add(new Person { FirstName = "C", LastName = "D", DateOfBirth = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var controller = new PersonsController(context);

        var result = await controller.GetPersons();

        var actionResult = Assert.IsType<ActionResult<IEnumerable<Person>>>(result);
        var persons = Assert.IsAssignableFrom<IEnumerable<Person>>(actionResult.Value);
        Assert.Equal(2, persons.Count());
    }

    [Fact]
    public async Task DeletePerson_ReturnsNoContent_WhenPersonExists()
    {
        using var context = GetInMemoryDbContext();
        var person = new Person { FirstName = "To", LastName = "Delete", DateOfBirth = DateTime.UtcNow };
        context.Persons.Add(person);
        await context.SaveChangesAsync();
        var controller = new PersonsController(context);

        var result = await controller.DeletePerson(person.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await context.Persons.FindAsync(person.Id));
    }

    [Fact]
    public async Task PatchPerson_ReturnsNoContent_AndUpdatesFields()
    {
        using var context = GetInMemoryDbContext();
        var person = new Person { FirstName = "Old", LastName = "Name", DateOfBirth = DateTime.UtcNow };
        context.Persons.Add(person);
        await context.SaveChangesAsync();
        var controller = new PersonsController(context);
        var updatedPerson = new Person { FirstName = "New" };

        var result = await controller.PatchPerson(person.Id, updatedPerson);

        Assert.IsType<NoContentResult>(result);
        var fromDb = await context.Persons.FindAsync(person.Id);
        Assert.Equal("New", fromDb!.FirstName);
        Assert.Equal("Name", fromDb.LastName);
    }
}