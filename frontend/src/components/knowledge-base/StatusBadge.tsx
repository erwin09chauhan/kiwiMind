import { cn } from '@/lib/utils'
import type { DocumentStatus } from '@/types/api'

const statusStyles: Record<DocumentStatus, { label: string; badge: string; dot: string }> = {
  Ready: { label: 'Ready', badge: 'bg-emerald-500/10 text-emerald-700', dot: 'bg-emerald-500' },
  Processing: { label: 'Processing', badge: 'bg-amber-500/10 text-amber-700', dot: 'bg-amber-500 animate-pulse' },
  Queued: { label: 'Queued', badge: 'bg-amber-500/10 text-amber-700', dot: 'bg-amber-500 animate-pulse' },
  Failed: { label: 'Failed', badge: 'bg-destructive/10 text-destructive', dot: 'bg-destructive' },
}

export function StatusBadge({ status }: { status: DocumentStatus }) {
  const s = statusStyles[status]
  return (
    <span
      className={cn('inline-flex shrink-0 items-center gap-1.5 rounded-full px-2 py-0.5 text-xs font-medium', s.badge)}
    >
      <span className={cn('size-1.5 rounded-full', s.dot)} />
      {s.label}
    </span>
  )
}
