import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { conversationsApi } from '@/lib/api'
import { Badge } from '@/components/ui/badge'

export function ConversationPage() {
  const { id: knowledgeBaseId, conversationId } = useParams<{ id: string; conversationId: string }>()

  const { data: conversation } = useQuery({
    queryKey: ['conversation', knowledgeBaseId, conversationId],
    queryFn: () => conversationsApi.get(knowledgeBaseId!, conversationId!),
  })

  return (
    <div className="flex flex-col gap-6">
      <div>
        <Link to={`/knowledge-bases/${knowledgeBaseId}`} className="text-muted-foreground text-sm underline">
          ← Back to knowledge base
        </Link>
        <h1 className="text-2xl font-semibold">{conversation?.title ?? '…'}</h1>
      </div>

      <div className="flex flex-col gap-3">
        {conversation?.messages.map((message) => (
          <div key={message.id} className="flex flex-col gap-1 rounded-md border px-4 py-3">
            <Badge variant={message.role === 'User' ? 'secondary' : 'default'} className="w-fit">
              {message.role}
            </Badge>
            <p className="whitespace-pre-wrap">{message.content}</p>
          </div>
        ))}
        {conversation?.messages.length === 0 && (
          <p className="text-muted-foreground">No messages yet. Live streaming chat is coming next.</p>
        )}
      </div>
    </div>
  )
}
