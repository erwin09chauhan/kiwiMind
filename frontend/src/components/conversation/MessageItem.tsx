import { Sparkles } from 'lucide-react'
import type { Citation, MessageRole } from '@/types/api'
import { CitationChips } from './CitationChips'

export function MessageItem({
  role,
  content,
  citations = [],
  streaming = false,
  fileNameByDocumentId,
}: {
  role: MessageRole
  content: string
  citations?: Citation[]
  streaming?: boolean
  fileNameByDocumentId: Map<string, string>
}) {
  if (role === 'User') {
    return (
      <div className="flex justify-end">
        <div className="bg-secondary text-secondary-foreground max-w-[85%] sm:max-w-[80%] rounded-2xl rounded-br-md px-4 py-2.5 text-[15px] leading-relaxed break-words whitespace-pre-wrap">
          {content}
        </div>
      </div>
    )
  }

  return (
    <div className="flex gap-3">
      <span className="bg-primary text-primary-foreground flex size-8 shrink-0 items-center justify-center rounded-lg">
        <Sparkles className="size-4" />
      </span>
      <div className="min-w-0 flex-1 space-y-2 pt-1">
        <p className="text-[15px] leading-relaxed break-words whitespace-pre-wrap">
          {content}
          {streaming && <span className="ml-0.5 inline-block animate-pulse">▍</span>}
        </p>
        <CitationChips citations={citations} fileNameByDocumentId={fileNameByDocumentId} />
      </div>
    </div>
  )
}
