using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.Conversations.Delete;

public class DeleteConversationCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<DeleteConversationCommand>
{
    public async Task Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await db.Conversations
            .SingleOrDefaultAsync(c => c.Id == request.ConversationId
                && c.KnowledgeBaseId == request.KnowledgeBaseId
                && c.UserId == currentUser.UserId, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException(nameof(Conversation), request.ConversationId);
        }

        db.Conversations.Remove(conversation);
        await db.SaveChangesAsync(cancellationToken);
    }
}
