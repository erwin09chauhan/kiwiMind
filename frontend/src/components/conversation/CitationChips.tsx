import { FileText } from 'lucide-react'
import type { Citation } from '@/types/api'

function dedupeByDocument(citations: Citation[]): Citation[] {
  const seen = new Set<string>()
  return citations.filter((c) => {
    if (seen.has(c.documentId)) return false
    seen.add(c.documentId)
    return true
  })
}

export function CitationChips({
  citations,
  fileNameByDocumentId,
}: {
  citations: Citation[]
  fileNameByDocumentId: Map<string, string>
}) {
  const unique = dedupeByDocument(citations)
  if (unique.length === 0) return null

  return (
    <div className="flex flex-wrap gap-1.5 pt-1">
      {unique.map((citation) => (
        <span
          key={citation.chunkId}
          title={citation.page != null ? `Page ${citation.page}` : undefined}
          className="border-border bg-background text-muted-foreground inline-flex items-center gap-1.5 rounded-full border px-2 py-0.5 text-xs"
        >
          <FileText className="size-3" />
          {fileNameByDocumentId.get(citation.documentId) ?? 'Unknown document'}
        </span>
      ))}
    </div>
  )
}
