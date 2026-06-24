namespace KiwiMind.Application.Common.Interfaces;

public record TextChunk(int Index, string Content, int TokenCount);

public interface ITextChunker
{
    IReadOnlyList<TextChunk> Chunk(string text);
}
