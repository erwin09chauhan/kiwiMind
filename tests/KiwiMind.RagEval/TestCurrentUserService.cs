using KiwiMind.Application.Common.Interfaces;

namespace KiwiMind.RagEval;

public class TestCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; }
}
