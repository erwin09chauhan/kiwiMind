import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { knowledgeBasesApi } from '@/lib/api'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
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

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Knowledge Bases</h1>
        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger render={<Button>New knowledge base</Button>} />
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
                {createMutation.isPending ? 'Creating…' : 'Create'}
              </Button>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {isLoading && <p className="text-muted-foreground">Loading…</p>}

      {knowledgeBases?.length === 0 && (
        <p className="text-muted-foreground">No knowledge bases yet. Create one to get started.</p>
      )}

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {knowledgeBases?.map((kb) => (
          <Link key={kb.id} to={`/knowledge-bases/${kb.id}`}>
            <Card className="hover:border-primary/50 transition-colors">
              <CardHeader>
                <CardTitle className="text-lg">{kb.name}</CardTitle>
              </CardHeader>
              <CardContent className="text-muted-foreground text-sm">
                {kb.documentCount} document{kb.documentCount === 1 ? '' : 's'}
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  )
}
