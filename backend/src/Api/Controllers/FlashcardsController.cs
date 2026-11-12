using Application.Common.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Common.Models;
using Application.Flashcards.Commands;
using Domain.Flashcards;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/flashcards")]
[ApiController]
public class FlashcardsController(ISender sender, IFlashcardQueries flashcardQueries) : ControllerBase
{
    // GET /api/flashcards?pageNumber=value&pageSize=value
    [HttpGet]
    public async Task<ActionResult<PagedResult<FlashcardDto>>> GetFlashcards(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        
        var pagedFlashcards = await flashcardQueries.GetAllPaged(
            pageNumber, 
            pageSize,
            null,
            cancellationToken);
        
        return Ok(pagedFlashcards.Map(FlashcardDto.FromDomainModel));
    }

    // GET /api/flashcards/{id}
    [HttpGet("{id:long}")]
    public async Task<ActionResult<FlashcardDto>> GetFlashcard(long id, CancellationToken cancellationToken)
    {
        var flashcard = await flashcardQueries.GetById(new FlashcardId(id), cancellationToken);

        return flashcard.Match<ActionResult<FlashcardDto>>(
            c => FlashcardDto.FromDomainModel(c),
            () => NotFound());
    }

    // POST /api/flashcards
    [HttpPost]
    public async Task<ActionResult<FlashcardDto>> CreateFlashcard(
        [FromBody] CreateFlashcardDto request,
        CancellationToken cancellationToken)
    {
        var input = new CreateFlashcardCommand
        {
            Term = request.Term,
            Definition = request.Definition,
            SetId = request.SetId
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<FlashcardDto>>(
            c => CreatedAtAction(nameof(GetFlashcard), new { id = c.Id.Value }, FlashcardDto.FromDomainModel(c)),
            e => e.ToObjectResult());
    }

    // PUT /api/flashcards/{id}
    [HttpPut("{id:long}")]
    public async Task<ActionResult<FlashcardDto>> UpdateFlashcard(
        long id,
        [FromBody] UpdateFlashcardDto request,
        CancellationToken cancellationToken)
    {
        var input = new UpdateFlashcardCommand
        {
            FlashcardId = id,
            Term = request.Term,
            Definition = request.Definition
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<FlashcardDto>>(
            c => Ok(FlashcardDto.FromDomainModel(c)),
            e => e.ToObjectResult());
    }

    // DELETE /api/flashcards/{id}
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<FlashcardDto>> DeleteFlashcard(long id, CancellationToken cancellationToken)
    {
        var input = new DeleteFlashcardCommand
        {
            FlashcardId = id
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<FlashcardDto>>(
            c => Ok(FlashcardDto.FromDomainModel(c)),
            e => e.ToObjectResult());
    }
}
