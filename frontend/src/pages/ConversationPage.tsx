import { useEffect, useMemo, useRef, useState, type FormEvent, type KeyboardEvent } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { conversationsApi, documentsApi } from '@/lib/api'
import { useChatConnection } from '@/hooks/useChatConnection'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import type { Citation, MessageDto } from '@/types/api'

export function ConversationPage() {
  const { id: knowledgeBaseId, conversationId } = useParams<{ id: string; conversationId: string }>()
  const kbId = knowledgeBaseId!
  const convId = conversationId!

  const { data: conversation } = useQuery({
    queryKey: ['conversation', kbId, convId],
    queryFn: () => conversationsApi.get(kbId, convId),
  })

  const { data: documents } = useQuery({
    queryKey: ['documents', kbId],
    queryFn: () => documentsApi.list(kbId),
  })

  const fileNameByDocumentId = useMemo(
    () => new Map(documents?.map((d) => [d.id, d.fileName]) ?? []),
    [documents],
  )

  const [localMessages, setLocalMessages] = useState<MessageDto[]>([])
  useEffect(() => {
    if (conversation) {
      setLocalMessages(conversation.messages)
    }
  }, [conversation])

  const { streamingText, completedMessage, error, sendMessage, clearError } = useChatConnection(kbId, convId)

  useEffect(() => {
    if (completedMessage) {
      setLocalMessages((prev) => (prev.some((m) => m.id === completedMessage.id) ? prev : [...prev, completedMessage]))
    }
  }, [completedMessage])

  const [draft, setDraft] = useState('')
  const isStreaming = streamingText !== null

  function handleSend(e?: FormEvent) {
    e?.preventDefault()
    const content = draft.trim()
    if (!content || isStreaming) return

    setLocalMessages((prev) => [
      ...prev,
      { id: crypto.randomUUID(), role: 'User', content, citations: [], tokensUsed: 0, createdAt: new Date().toISOString() },
    ])
    setDraft('')
    void sendMessage(content)
  }

  function handleKeyDown(e: KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  const bottomRef = useRef<HTMLDivElement>(null)
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [localMessages, streamingText])

  return (
    <div className="flex h-[calc(100svh-6rem)] flex-col gap-4">
      <div>
        <Link to={`/knowledge-bases/${kbId}`} className="text-muted-foreground text-sm underline">
          ← Back to knowledge base
        </Link>
        <h1 className="text-2xl font-semibold">{conversation?.title ?? '…'}</h1>
      </div>

      <div className="flex-1 overflow-y-auto rounded-md border p-4">
        <div className="flex flex-col gap-3">
          {localMessages.length === 0 && !isStreaming && (
            <p className="text-muted-foreground">No messages yet. Ask a question below.</p>
          )}
          {localMessages.map((message) => (
            <MessageBubble key={message.id} message={message} fileNameByDocumentId={fileNameByDocumentId} />
          ))}
          {isStreaming && (
            <div className="flex flex-col gap-1 rounded-md border px-4 py-3">
              <Badge variant="default" className="w-fit">
                Assistant
              </Badge>
              <p className="whitespace-pre-wrap">
                {streamingText}
                <span className="animate-pulse">▍</span>
              </p>
            </div>
          )}
          <div ref={bottomRef} />
        </div>
      </div>

      {error && (
        <p className="text-destructive text-sm">
          {error}{' '}
          <button type="button" onClick={clearError} className="underline">
            Dismiss
          </button>
        </p>
      )}

      <form onSubmit={handleSend} className="flex gap-2">
        <Textarea
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Ask a question about this knowledge base…"
          rows={2}
          disabled={isStreaming}
        />
        <Button type="submit" disabled={isStreaming || !draft.trim()}>
          Send
        </Button>
      </form>
    </div>
  )
}

function MessageBubble({
  message,
  fileNameByDocumentId,
}: {
  message: MessageDto
  fileNameByDocumentId: Map<string, string>
}) {
  const uniqueCitations = useMemo(() => dedupeCitations(message.citations), [message.citations])

  return (
    <div className="flex flex-col gap-1 rounded-md border px-4 py-3">
      <Badge variant={message.role === 'User' ? 'secondary' : 'default'} className="w-fit">
        {message.role}
      </Badge>
      <p className="whitespace-pre-wrap">{message.content}</p>
      {uniqueCitations.length > 0 && (
        <div className="mt-1 flex flex-wrap gap-1">
          {uniqueCitations.map((citation) => (
            <Badge
              key={citation.chunkId}
              variant="outline"
              title={citation.page != null ? `Page ${citation.page}` : undefined}
            >
              {fileNameByDocumentId.get(citation.documentId) ?? 'Unknown document'}
            </Badge>
          ))}
        </div>
      )}
    </div>
  )
}

function dedupeCitations(citations: Citation[]): Citation[] {
  const seen = new Set<string>()
  return citations.filter((c) => {
    if (seen.has(c.documentId)) return false
    seen.add(c.documentId)
    return true
  })
}
