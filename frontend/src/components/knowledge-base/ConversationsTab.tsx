import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ChevronRight, MessagesSquare, Plus, Trash2 } from 'lucide-react'
import { conversationsApi } from '@/lib/api'
import { formatRelativeDate } from '@/lib/format'
import { useConversations } from '@/hooks/useKnowledgeBaseQueries'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'

export function ConversationsTab({ knowledgeBaseId }: { knowledgeBaseId: string }) {
  const queryClient = useQueryClient()
  const { data: conversations } = useConversations(knowledgeBaseId)

  const [title, setTitle] = useState('')

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['conversations', knowledgeBaseId] })

  const createMutation = useMutation({
    mutationFn: (title: string) => conversationsApi.create(knowledgeBaseId, title),
    onSuccess: () => {
      invalidate()
      setTitle('')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (conversationId: string) => conversationsApi.delete(knowledgeBaseId, conversationId),
    onSuccess: invalidate,
  })

  function handleCreate(e: FormEvent) {
    e.preventDefault()
    if (title.trim()) {
      createMutation.mutate(title.trim())
    }
  }

  return (
    <div className="flex flex-col gap-4">
      <form onSubmit={handleCreate} className="flex gap-2">
        <Input
          placeholder="Start a new conversation…"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
        <Button type="submit" disabled={createMutation.isPending} className="gap-1.5">
          <Plus className="size-4" />
          New
        </Button>
      </form>

      {conversations === undefined && (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 2 }).map((_, i) => (
            <div key={i} className="bg-card h-[58px] animate-pulse rounded-lg border" />
          ))}
        </div>
      )}

      {conversations?.length === 0 && (
        <div className="flex flex-col items-center gap-3 py-14 text-center">
          <span className="bg-secondary text-muted-foreground flex size-12 items-center justify-center rounded-full">
            <MessagesSquare className="size-6" />
          </span>
          <div className="flex flex-col gap-0.5">
            <p className="font-medium">No conversations yet</p>
            <p className="text-muted-foreground text-sm">Start one above to chat with this knowledge base.</p>
          </div>
        </div>
      )}

      <div className="flex flex-col gap-2">
        {conversations?.map((conv) => (
          <Link
            key={conv.id}
            to={`/knowledge-bases/${knowledgeBaseId}/conversations/${conv.id}`}
            className="group bg-card hover:border-primary/40 hover:bg-accent/40 focus-visible:border-ring focus-visible:ring-ring/50 flex items-center gap-3 rounded-lg border px-3 py-2.5 transition-colors outline-none focus-visible:ring-[3px]"
          >
            <span className="bg-primary/10 text-primary flex size-9 shrink-0 items-center justify-center rounded-md">
              <MessagesSquare className="size-4.5" />
            </span>
            <div className="min-w-0 flex-1">
              <p className="group-hover:text-primary truncate text-sm font-medium transition-colors">{conv.title}</p>
              <p className="text-muted-foreground text-xs">
                {conv.messageCount} message{conv.messageCount === 1 ? '' : 's'} · {formatRelativeDate(conv.createdAt)}
              </p>
            </div>
            <Button
              variant="ghost"
              size="icon-sm"
              onClick={(e) => {
                e.preventDefault()
                deleteMutation.mutate(conv.id)
              }}
              disabled={deleteMutation.isPending}
              className="text-muted-foreground hover:text-destructive max-sm:opacity-100 opacity-0 transition-opacity group-hover:opacity-100 focus-visible:opacity-100"
              aria-label={`Delete ${conv.title}`}
            >
              <Trash2 className="size-4" />
            </Button>
            <ChevronRight className="text-muted-foreground size-4 shrink-0" />
          </Link>
        ))}
      </div>
    </div>
  )
}
