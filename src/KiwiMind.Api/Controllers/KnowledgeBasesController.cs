using KiwiMind.Application.KnowledgeBases;
using KiwiMind.Application.KnowledgeBases.Create;
using KiwiMind.Application.KnowledgeBases.Delete;
using KiwiMind.Application.KnowledgeBases.Get;
using KiwiMind.Application.KnowledgeBases.List;
using KiwiMind.Application.Retrieval;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KiwiMind.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/knowledge-bases")]
public class KnowledgeBasesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<KnowledgeBaseDto>> Create(CreateKnowledgeBaseCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<KnowledgeBaseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetKnowledgeBasesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<KnowledgeBaseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetKnowledgeBaseByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteKnowledgeBaseCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/search")]
    public async Task<ActionResult<List<ChunkSearchResultDto>>> Search(
        Guid id, [FromQuery] string query, [FromQuery] int topK = 5, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new SearchKnowledgeBaseQuery(id, query, topK), cancellationToken);
        return Ok(result);
    }
}
