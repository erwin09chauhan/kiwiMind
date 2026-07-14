import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, Sparkles } from 'lucide-react'
import { conversationsApi, documentsApi } from '@/lib/api'
import { useChatConnection } from '@/hooks/useChatConnection'
import { MessageItem } from '@/components/conversation/MessageItem'
import { ChatComposer } from '@/components/conversation/ChatComposer'
import type { MessageDto } from '@/types/api'

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

  function handleSend() {
    const content = draft.trim()
    if (!content || isStreaming) return

    setLocalMessages((prev) => [
      ...prev,
      {
        id: crypto.randomUUID(),
        role: 'User',
        content,
        citations: [],
        tokensUsed: 0,
        createdAt: new Date().toISOString(),
      },
    ])
    setDraft('')
    void sendMessage(content)
  }

  const bottomRef = useRef<HTMLDivElement>(null)
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [localMessages, streamingText])

  const isEmpty = localMessages.length === 0 && !isStreaming

  return (
    <div className="flex min-h-0 flex-1 flex-col gap-4">
      <div className="flex flex-col gap-1">
        <Link
          to={`/knowledge-bases/${kbId}`}
          className="text-muted-foreground hover:text-foreground inline-flex w-fit items-center gap-1 text-sm transition-colors"
        >
          <ChevronLeft className="size-4" />
          Back
        </Link>
        <h1 className="text-xl font-semibold tracking-tight">{conversation?.title ?? '…'}</h1>
      </div>

      <div className="min-h-0 flex-1 overflow-y-auto">
        <div className="mx-auto flex min-h-full max-w-3xl flex-col gap-6 py-2">
          {isEmpty && (
            <div className="my-auto flex flex-col items-center gap-3 text-center">
              <span className="bg-primary/10 text-primary flex size-12 items-center justify-center rounded-2xl">
                <Sparkles className="size-6" />
              </span>
              <div className="flex flex-col gap-1">
                <p className="font-medium">Ask anything about this knowledge base</p>
                <p className="text-muted-foreground text-sm">
                  Answers are grounded in your documents, with citations.
                </p>
              </div>
            </div>
          )}

          {localMessages.map((message) => (
            <MessageItem
              key={message.id}
              role={message.role}
              content={message.content}
              citations={message.citations}
              fileNameByDocumentId={fileNameByDocumentId}
            />
          ))}

          {isStreaming && (
            <MessageItem
              role="Assistant"
              content={streamingText ?? ''}
              streaming
              fileNameByDocumentId={fileNameByDocumentId}
            />
          )}

          <div ref={bottomRef} />
        </div>
      </div>

      <div className="mx-auto w-full max-w-3xl">
        {error && (
          <div className="border-destructive/30 bg-destructive/5 text-destructive mb-2 flex items-center justify-between gap-2 rounded-lg border px-3 py-2 text-sm">
            <span>{error}</span>
            <button type="button" onClick={clearError} className="shrink-0 font-medium underline">
              Dismiss
            </button>
          </div>
        )}
        <ChatComposer value={draft} onChange={setDraft} onSubmit={handleSend} disabled={isStreaming} />
      </div>
    </div>
  )
}
