using KiwiMind.Application.Documents;
using KiwiMind.Application.Documents.Delete;
using KiwiMind.Application.Documents.Get;
using KiwiMind.Application.Documents.List;
using KiwiMind.Application.Documents.Upload;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KiwiMind.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/knowledge-bases/{knowledgeBaseId:guid}/documents")]
public class DocumentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<DocumentDto>> Upload(Guid knowledgeBaseId, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var command = new UploadDocumentCommand(knowledgeBaseId, file.FileName, file.ContentType, file.Length, stream);
        var result = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { knowledgeBaseId, id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentDto>>> GetAll(Guid knowledgeBaseId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDocumentsQuery(knowledgeBaseId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> GetById(Guid knowledgeBaseId, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDocumentByIdQuery(knowledgeBaseId, id), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid knowledgeBaseId, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteDocumentCommand(knowledgeBaseId, id), cancellationToken);
        return NoContent();
    }
}
