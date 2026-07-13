import { useRef, useState, type FormEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { AlertCircle, FileText, Loader2, Plus, Trash2, Upload, X } from 'lucide-react'
import { documentsApi } from '@/lib/api'
import { useDocuments } from '@/hooks/useKnowledgeBaseQueries'
import { formatFileSize, formatRelativeDate } from '@/lib/format'
import { Button } from '@/components/ui/button'
import { StatusBadge } from './StatusBadge'

const MAX_FILE_SIZE = 10 * 1024 * 1024
const ALLOWED_EXTENSIONS = ['.pdf', '.docx', '.txt', '.md']

export function DocumentsTab({ knowledgeBaseId }: { knowledgeBaseId: string }) {
  const queryClient = useQueryClient()
  const { data: documents } = useDocuments(knowledgeBaseId)

  const fileInputRef = useRef<HTMLInputElement>(null)
  const [uploadError, setUploadError] = useState<string | null>(null)

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['documents', knowledgeBaseId] })
    queryClient.invalidateQueries({ queryKey: ['knowledge-bases'] })
  }

  const uploadMutation = useMutation({
    mutationFn: (file: File) => documentsApi.upload(knowledgeBaseId, file),
    onSuccess: () => {
      setUploadError(null)
      invalidate()
    },
    onError: (err) => {
      const message = isAxiosError(err) ? (err.response?.data as { message?: string })?.message : null
      setUploadError(message ?? 'Upload failed. Please try again.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (documentId: string) => documentsApi.delete(knowledgeBaseId, documentId),
    onSuccess: invalidate,
  })

  function handleFileSelected(e: FormEvent<HTMLInputElement>) {
    const file = e.currentTarget.files?.[0]
    e.currentTarget.value = ''
    if (!file) return

    setUploadError(null)

    const extension = `.${file.name.split('.').pop()?.toLowerCase() ?? ''}`
    if (!ALLOWED_EXTENSIONS.includes(extension)) {
      setUploadError(`“${file.name}” isn’t a supported type. Use PDF, DOCX, TXT, or MD.`)
      return
    }
    if (file.size > MAX_FILE_SIZE) {
      setUploadError(`“${file.name}” is ${formatFileSize(file.size)} — the limit is 10 MB.`)
      return
    }

    uploadMutation.mutate(file)
  }

  const pickFile = () => fileInputRef.current?.click()
  const hasDocuments = (documents?.length ?? 0) > 0

  return (
    <div className="flex flex-col gap-4">
      <input
        ref={fileInputRef}
        type="file"
        accept=".pdf,.docx,.txt,.md"
        className="hidden"
        onChange={handleFileSelected}
      />

      {documents === undefined && (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="bg-card h-[58px] animate-pulse rounded-lg border" />
          ))}
        </div>
      )}

      {documents && !hasDocuments && (
        <button
          type="button"
          onClick={pickFile}
          disabled={uploadMutation.isPending}
          className="border-border hover:border-primary/50 hover:bg-accent/40 flex w-full flex-col items-center justify-center gap-2 rounded-xl border border-dashed px-4 py-16 text-center transition-colors disabled:opacity-60"
        >
          <span className="bg-secondary text-muted-foreground flex size-12 items-center justify-center rounded-full">
            {uploadMutation.isPending ? <Loader2 className="size-6 animate-spin" /> : <Upload className="size-6" />}
          </span>
          <span className="font-medium">{uploadMutation.isPending ? 'Uploading…' : 'Upload your first document'}</span>
          <span className="text-muted-foreground text-xs">PDF, DOCX, TXT, or MD — up to 10MB</span>
        </button>
      )}

      {hasDocuments && (
        <div className="flex items-center justify-between">
          <p className="text-muted-foreground text-sm">
            {documents!.length} document{documents!.length === 1 ? '' : 's'}
          </p>
          <Button variant="outline" size="sm" onClick={pickFile} disabled={uploadMutation.isPending} className="gap-1.5">
            {uploadMutation.isPending ? <Loader2 className="size-3.5 animate-spin" /> : <Plus className="size-3.5" />}
            Add document
          </Button>
        </div>
      )}

      {uploadError && (
        <div className="border-destructive/30 bg-destructive/5 text-destructive flex items-start gap-2 rounded-lg border px-3 py-2 text-sm">
          <AlertCircle className="mt-0.5 size-4 shrink-0" />
          <span className="flex-1">{uploadError}</span>
          <button
            type="button"
            onClick={() => setUploadError(null)}
            className="shrink-0 opacity-70 hover:opacity-100"
            aria-label="Dismiss"
          >
            <X className="size-4" />
          </button>
        </div>
      )}

      {hasDocuments && (
        <div className="flex flex-col gap-2">
          {documents!.map((doc) => (
            <div
              key={doc.id}
              className="group bg-card hover:bg-accent/40 flex items-center gap-3 rounded-lg border px-3 py-2.5 transition-colors"
            >
              <span className="bg-secondary text-muted-foreground flex size-9 shrink-0 items-center justify-center rounded-md">
                <FileText className="size-4.5" />
              </span>
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium">{doc.fileName}</p>
                <p className="text-muted-foreground text-xs">
                  Added {formatRelativeDate(doc.createdAt)}
                  {doc.pageCount != null && ` · ${doc.pageCount} pages`}
                </p>
              </div>
              <StatusBadge status={doc.status} />
              <Button
                variant="ghost"
                size="icon-sm"
                onClick={() => deleteMutation.mutate(doc.id)}
                disabled={deleteMutation.isPending}
                className="text-muted-foreground hover:text-destructive"
                aria-label={`Delete ${doc.fileName}`}
              >
                <Trash2 className="size-4" />
              </Button>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
