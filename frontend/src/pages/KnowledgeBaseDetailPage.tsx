import { Link, useParams } from 'react-router-dom'
import { ChevronLeft, FileText, Library, MessagesSquare } from 'lucide-react'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { useConversations, useDocuments, useKnowledgeBase } from '@/hooks/useKnowledgeBaseQueries'
import { DocumentsTab } from '@/components/knowledge-base/DocumentsTab'
import { ConversationsTab } from '@/components/knowledge-base/ConversationsTab'

export function KnowledgeBaseDetailPage() {
  const { id } = useParams<{ id: string }>()
  const knowledgeBaseId = id!

  const { data: knowledgeBase } = useKnowledgeBase(knowledgeBaseId)
  const { data: documents } = useDocuments(knowledgeBaseId)
  const { data: conversations } = useConversations(knowledgeBaseId)

  const documentCount = documents?.length ?? knowledgeBase?.documentCount ?? 0
  const conversationCount = conversations?.length ?? 0

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-4">
        <Link
          to="/knowledge-bases"
          className="text-muted-foreground hover:text-foreground inline-flex w-fit items-center gap-1 text-sm transition-colors"
        >
          <ChevronLeft className="size-4" />
          Knowledge bases
        </Link>
        <div className="flex items-center gap-3">
          <span className="bg-primary/10 text-primary flex size-11 items-center justify-center rounded-xl">
            <Library className="size-5.5" />
          </span>
          <div className="flex min-w-0 flex-col">
            <h1 className="text-2xl font-semibold tracking-tight break-words">{knowledgeBase?.name ?? '…'}</h1>
            {knowledgeBase && (
              <p className="text-muted-foreground text-sm">
                Created{' '}
                {new Date(knowledgeBase.createdAt).toLocaleDateString(undefined, {
                  month: 'short',
                  day: 'numeric',
                  year: 'numeric',
                })}
              </p>
            )}
          </div>
        </div>
      </div>

      <Tabs defaultValue="documents" className="flex-col gap-6">
        <TabsList>
          <TabsTrigger value="documents">
            <FileText className="size-4" />
            Documents
            <TabCount value={documentCount} />
          </TabsTrigger>
          <TabsTrigger value="conversations">
            <MessagesSquare className="size-4" />
            Conversations
            <TabCount value={conversationCount} />
          </TabsTrigger>
        </TabsList>

        <TabsContent value="documents">
          <DocumentsTab knowledgeBaseId={knowledgeBaseId} />
        </TabsContent>

        <TabsContent value="conversations">
          <ConversationsTab knowledgeBaseId={knowledgeBaseId} />
        </TabsContent>
      </Tabs>
    </div>
  )
}

function TabCount({ value }: { value: number }) {
  return (
    <span className="bg-foreground/8 text-muted-foreground ml-0.5 rounded-full px-1.5 text-[11px] font-medium tabular-nums">
      {value}
    </span>
  )
}
