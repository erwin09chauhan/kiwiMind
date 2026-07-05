using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Conversations.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KiwiMind.Api.Hubs;

[Authorize]
public class ChatHub(ISender sender, IHttpContextAccessor httpContextAccessor) : Hub
{
    private const int SimulatedTokenDelayMilliseconds = 40;

    public async Task SendMessage(Guid knowledgeBaseId, Guid conversationId, string content)
    {
        httpContextAccessor.HttpContext = Context.GetHttpContext();

        try
        {
            var message = await sender.Send(new SendMessageCommand(knowledgeBaseId, conversationId, content));

            foreach (var word in message.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                await Clients.Caller.SendAsync("ReceiveToken", word + " ");
                await Task.Delay(SimulatedTokenDelayMilliseconds);
            }

            await Clients.Caller.SendAsync("MessageComplete", message);
        }
        catch (NotFoundException ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
        catch (InvalidFileException ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
        catch (Exception)
        {
            await Clients.Caller.SendAsync("Error", "An unexpected error occurred while generating the answer.");
        }
    }
}
