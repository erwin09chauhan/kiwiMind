import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { FileText, Library, Plus } from 'lucide-react'
import { knowledgeBasesApi } from '@/lib/api'
import type { KnowledgeBaseDto } from '@/types/api'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'

export function KnowledgeBasesPage() {
  const queryClient = useQueryClient()
  const { data: knowledgeBases, isLoading } = useQuery({
    queryKey: ['knowledge-bases'],
    queryFn: knowledgeBasesApi.list,
  })

  const [name, setName] = useState('')
  const [open, setOpen] = useState(false)
  const createMutation = useMutation({
    mutationFn: (name: string) => knowledgeBasesApi.create(name),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['knowledge-bases'] })
      setName('')
      setOpen(false)
    },
  })

  function handleCreate(e: FormEvent) {
    e.preventDefault()
    if (name.trim()) {
      createMutation.mutate(name.trim())
    }
  }

  const isEmpty = !isLoading && knowledgeBases?.length === 0

  return (
    <div className="flex flex-col gap-8">
      <div className="flex items-end justify-between gap-4">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-semibold tracking-tight">Knowledge bases</h1>
          <p className="text-muted-foreground text-sm">Upload documents and chat with them.</p>
        </div>
        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger
            render={
              <Button className="gap-1.5">
                <Plus className="size-4" />
                New
              </Button>
            }
          />
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create a knowledge base</DialogTitle>
            </DialogHeader>
            <form onSubmit={handleCreate} className="flex flex-col gap-4">
              <Input
                placeholder="e.g. Work Docs"
                value={name}
                onChange={(e) => setName(e.target.value)}
                autoFocus
              />
              <Button type="submit" disabled={createMutation.isPending}>
                {createMutation.isPending ? 'Creating…' : 'Create knowledge base'}
              </Button>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {isLoading && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="bg-card h-[104px] animate-pulse rounded-xl border" />
          ))}
        </div>
      )}

      {isEmpty && (
        <div className="border-border flex flex-col items-center gap-4 rounded-xl border border-dashed py-16 text-center">
          <span className="bg-secondary text-muted-foreground flex size-12 items-center justify-center rounded-full">
            <Library className="size-6" />
          </span>
          <div className="flex flex-col gap-1">
            <p className="font-medium">No knowledge bases yet</p>
            <p className="text-muted-foreground text-sm">Create your first one to start uploading documents.</p>
          </div>
          <Button onClick={() => setOpen(true)} className="gap-1.5">
            <Plus className="size-4" />
            New knowledge base
          </Button>
        </div>
      )}

      {!isLoading && knowledgeBases && knowledgeBases.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {knowledgeBases.map((kb) => (
            <KnowledgeBaseCard key={kb.id} knowledgeBase={kb} />
          ))}
        </div>
      )}
    </div>
  )
}

function KnowledgeBaseCard({ knowledgeBase }: { knowledgeBase: KnowledgeBaseDto }) {
  return (
    <Link
      to={`/knowledge-bases/${knowledgeBase.id}`}
      className="group bg-card hover:border-primary/40 flex flex-col gap-4 rounded-xl border p-5 transition-all hover:shadow-sm"
    >
      <span className="bg-primary/10 text-primary flex size-10 items-center justify-center rounded-lg">
        <FileText className="size-5" />
      </span>
      <div className="flex flex-col gap-1">
        <h2 className="group-hover:text-primary font-medium tracking-tight transition-colors">
          {knowledgeBase.name}
        </h2>
        <p className="text-muted-foreground text-xs">
          {knowledgeBase.documentCount} document{knowledgeBase.documentCount === 1 ? '' : 's'}
          {' · '}
          {new Date(knowledgeBase.createdAt).toLocaleDateString(undefined, {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
          })}
        </p>
      </div>
    </Link>
  )
}
