import { useRef, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { conversationsApi, documentsApi, knowledgeBasesApi } from '@/lib/api'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import type { DocumentStatus } from '@/types/api'

const statusVariant: Record<DocumentStatus, 'default' | 'secondary' | 'destructive'> = {
  Queued: 'secondary',
  Processing: 'secondary',
  Ready: 'default',
  Failed: 'destructive',
}

export function KnowledgeBaseDetailPage() {
  const { id } = useParams<{ id: string }>()
  const knowledgeBaseId = id!
  const queryClient = useQueryClient()

  const { data: knowledgeBase } = useQuery({
    queryKey: ['knowledge-bases', knowledgeBaseId],
    queryFn: () => knowledgeBasesApi.get(knowledgeBaseId),
  })

  const { data: documents } = useQuery({
    queryKey: ['documents', knowledgeBaseId],
    queryFn: () => documentsApi.list(knowledgeBaseId),
    refetchInterval: 3000,
  })

  const { data: conversations } = useQuery({
    queryKey: ['conversations', knowledgeBaseId],
    queryFn: () => conversationsApi.list(knowledgeBaseId),
  })

  const fileInputRef = useRef<HTMLInputElement>(null)
  const uploadMutation = useMutation({
    mutationFn: (file: File) => documentsApi.upload(knowledgeBaseId, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['documents', knowledgeBaseId] })
      queryClient.invalidateQueries({ queryKey: ['knowledge-bases'] })
    },
  })

  const deleteDocumentMutation = useMutation({
    mutationFn: (documentId: string) => documentsApi.delete(knowledgeBaseId, documentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['documents', knowledgeBaseId] })
      queryClient.invalidateQueries({ queryKey: ['knowledge-bases'] })
    },
  })

  const [conversationTitle, setConversationTitle] = useState('')
  const createConversationMutation = useMutation({
    mutationFn: (title: string) => conversationsApi.create(knowledgeBaseId, title),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['conversations', knowledgeBaseId] })
      setConversationTitle('')
    },
  })

  const deleteConversationMutation = useMutation({
    mutationFn: (conversationId: string) => conversationsApi.delete(knowledgeBaseId, conversationId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['conversations', knowledgeBaseId] }),
  })

  function handleFileSelected(e: FormEvent<HTMLInputElement>) {
    const file = e.currentTarget.files?.[0]
    if (file) {
      uploadMutation.mutate(file)
      e.currentTarget.value = ''
    }
  }

  function handleCreateConversation(e: FormEvent) {
    e.preventDefault()
    if (conversationTitle.trim()) {
      createConversationMutation.mutate(conversationTitle.trim())
    }
  }

  return (
    <div className="flex flex-col gap-6">
      <div>
        <Link to="/knowledge-bases" className="text-muted-foreground text-sm underline">
          ← All knowledge bases
        </Link>
        <h1 className="text-2xl font-semibold">{knowledgeBase?.name ?? '…'}</h1>
      </div>

      <Tabs defaultValue="documents">
        <TabsList>
          <TabsTrigger value="documents">Documents</TabsTrigger>
          <TabsTrigger value="conversations">Conversations</TabsTrigger>
        </TabsList>

        <TabsContent value="documents" className="flex flex-col gap-4">
          <div className="flex items-center gap-2">
            <input
              ref={fileInputRef}
              type="file"
              accept=".pdf,.docx,.txt,.md"
              className="hidden"
              onChange={handleFileSelected}
            />
            <Button onClick={() => fileInputRef.current?.click()} disabled={uploadMutation.isPending}>
              {uploadMutation.isPending ? 'Uploading…' : 'Upload document'}
            </Button>
            <span className="text-muted-foreground text-sm">PDF, DOCX, TXT, or MD — up to 10MB</span>
          </div>

          <div className="flex flex-col gap-2">
            {documents?.length === 0 && <p className="text-muted-foreground">No documents yet.</p>}
            {documents?.map((doc) => (
              <div key={doc.id} className="flex items-center justify-between rounded-md border px-4 py-3">
                <div className="flex items-center gap-3">
                  <span>{doc.fileName}</span>
                  <Badge variant={statusVariant[doc.status]}>{doc.status}</Badge>
                  {doc.pageCount != null && (
                    <span className="text-muted-foreground text-sm">{doc.pageCount} pages</span>
                  )}
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => deleteDocumentMutation.mutate(doc.id)}
                  disabled={deleteDocumentMutation.isPending}
                >
                  Delete
                </Button>
              </div>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="conversations" className="flex flex-col gap-4">
          <form onSubmit={handleCreateConversation} className="flex gap-2">
            <Input
              placeholder="New conversation title"
              value={conversationTitle}
              onChange={(e) => setConversationTitle(e.target.value)}
            />
            <Button type="submit" disabled={createConversationMutation.isPending}>
              Start
            </Button>
          </form>

          <div className="flex flex-col gap-2">
            {conversations?.length === 0 && <p className="text-muted-foreground">No conversations yet.</p>}
            {conversations?.map((conv) => (
              <Link
                key={conv.id}
                to={`/knowledge-bases/${knowledgeBaseId}/conversations/${conv.id}`}
                className="flex items-center justify-between rounded-md border px-4 py-3 hover:border-primary/50"
              >
                <div className="flex items-center gap-3">
                  <span>{conv.title}</span>
                  <span className="text-muted-foreground text-sm">{conv.messageCount} messages</span>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={(e) => {
                    e.preventDefault()
                    deleteConversationMutation.mutate(conv.id)
                  }}
                  disabled={deleteConversationMutation.isPending}
                >
                  Delete
                </Button>
              </Link>
            ))}
          </div>
        </TabsContent>
      </Tabs>
    </div>
  )
}
