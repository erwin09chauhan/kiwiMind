import { useEffect, useState } from 'react'
import { Loader2 } from 'lucide-react'
import { subscribeServerWaking } from '@/lib/server-wake'

export function ServerWakeBanner() {
  const [waking, setWaking] = useState(false)

  useEffect(() => subscribeServerWaking(setWaking), [])

  if (!waking) {
    return null
  }

  return (
    <div className="fixed inset-x-0 top-0 z-50 flex justify-center px-4 pt-3">
      <div className="bg-card text-muted-foreground flex items-center gap-2 rounded-full border px-4 py-2 text-sm shadow-md">
        <Loader2 className="size-4 shrink-0 animate-spin" />
        <span>Waking up the server — this can take a few seconds after inactivity.</span>
      </div>
    </div>
  )
}
