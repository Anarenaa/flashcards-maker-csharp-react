using Application.Common.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Common.Models;
using Application.Sets.Commands;
using Domain.Sets;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/sets")]
[ApiController]
public class SetsController(ISender sender, ISetQueries setQueries, IFlashcardQueries flashcardQueries) : ControllerBase
{
    // GET /api/sets?pageNumber=value&pageSize=value&searchTerm=value...
    [HttpGet]
    public async Task<ActionResult<PagedResult<SetDto>>> GetSets(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? fromLang = null,
        [FromQuery] string? toLang = null,
        [FromQuery] bool? isPublic = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var filters = new SetFilterOptions(searchTerm, fromLang, toLang, isPublic, sortBy, sortDescending);

        var pagedSets = await setQueries.GetAllPaged(
            pageNumber, 
            pageSize,
            filters,
            cancellationToken);
        
        return Ok(pagedSets);
    }

    // GET /api/sets/{id}
    [HttpGet("{id:long}")]
    public async Task<ActionResult<SetDto>> GetSet(
        long id,
        CancellationToken cancellationToken)
    {
        var set = await setQueries.GetById(new SetId(id), cancellationToken);

        return set.Match<ActionResult<SetDto>>(
            c => Ok(c),
            () => NotFound());
    }
    // GET /api/sets/{id}/flashcards?pageNumber=value&pageSize=value
    [HttpGet("{id:long}/flashcards")]
    public async Task<ActionResult<PagedResult<FlashcardDto>>> GetFlashcardsBySet(
        long id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var result = await flashcardQueries.GetAllPaged(
            pageNumber, 
            pageSize, 
            new SetId(id), 
            cancellationToken);
        return Ok(result.Map(FlashcardDto.FromDomainModel));
    }

    // POST /api/sets
    [HttpPost]
    public async Task<ActionResult<SetDto>> CreateSet(
        [FromBody] CreateSetDto request,
        CancellationToken cancellationToken)
    {
        var input = new CreateSetCommand
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            FromLang = request.FromLang,
            ToLang = request.ToLang,
            IsPublic = request.IsPublic,
            IsGenerated = request.IsGenerated
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<SetDto>>(
            c => CreatedAtAction(nameof(GetSet), new { id = c.Id.Value }, SetDto.FromDomainModel(c)),
            e => e.ToObjectResult());
    }

    // PUT /api/sets/{id}
    [HttpPut("{id:long}")]
    public async Task<ActionResult<SetDto>> UpdateSet(
        long id,
        [FromBody] UpdateSetDto request,
        CancellationToken cancellationToken)
    {
        var input = new UpdateSetCommand
        {
            SetId = id,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            FromLang = request.FromLang,
            ToLang = request.ToLang,
            IsPublic = request.IsPublic
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<SetDto>>(
            c => Ok(SetDto.FromDomainModel(c)),
            e => e.ToObjectResult());
    }

    // DELETE /api/sets/{id}
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<SetDto>> DeleteSet(long id, CancellationToken cancellationToken)
    {
        var input = new DeleteSetCommand
        {
            SetId = id
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<SetDto>>(
            c => Ok(SetDto.FromDomainModel(c)),
            e => e.ToObjectResult());
    }
}