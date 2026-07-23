using KiwiMind.Application.Conversations;
using KiwiMind.Application.Conversations.Create;
using KiwiMind.Application.Conversations.Delete;
using KiwiMind.Application.Conversations.Get;
using KiwiMind.Application.Conversations.List;
using KiwiMind.Application.Conversations.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KiwiMind.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/knowledge-bases/{knowledgeBaseId:guid}/conversations")]
public class ConversationsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ConversationDto>> Create(Guid knowledgeBaseId, [FromBody] CreateConversationRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateConversationCommand(knowledgeBaseId, request.Title), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { knowledgeBaseId, id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<ConversationDto>>> GetAll(Guid knowledgeBaseId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetConversationsQuery(knowledgeBaseId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationDetailDto>> GetById(Guid knowledgeBaseId, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetConversationByIdQuery(knowledgeBaseId, id), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid knowledgeBaseId, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteConversationCommand(knowledgeBaseId, id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/messages")]
    [EnableRateLimiting("chat")]
    public async Task<ActionResult<MessageDto>> SendMessage(
        Guid knowledgeBaseId, Guid id, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SendMessageCommand(knowledgeBaseId, id, request.Content), cancellationToken);
        return Ok(result);
    }
}

public record CreateConversationRequest(string Title);
public record SendMessageRequest(string Content);
