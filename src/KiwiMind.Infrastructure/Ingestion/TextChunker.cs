using KiwiMind.Application.Common.Interfaces;

namespace KiwiMind.Infrastructure.Ingestion;

public class TextChunker : ITextChunker
{
    private const int ChunkSizeWords = 600;
    private const int OverlapWords = 90;

    public IReadOnlyList<TextChunk> Chunk(string text)
    {
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return [];
        }

        var chunks = new List<TextChunk>();
        var index = 0;
        var start = 0;

        while (start < words.Length)
        {
            var length = Math.Min(ChunkSizeWords, words.Length - start);
            var content = string.Join(' ', words, start, length);
            chunks.Add(new TextChunk(index, content, length));

            index++;
            start += ChunkSizeWords - OverlapWords;
        }

        return chunks;
    }
}
