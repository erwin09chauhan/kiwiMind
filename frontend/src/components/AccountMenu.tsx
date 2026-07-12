import { useEffect, useRef, useState } from 'react'
import { ChevronDown, LogOut } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'
import { Button } from '@/components/ui/button'

function initialsFromEmail(email: string): string {
  const parts = email.split('@')[0].split(/[._-]+/).filter(Boolean)
  if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase()
  return email.slice(0, 2).toUpperCase()
}

function displayNameFromEmail(email: string): string {
  return email
    .split('@')[0]
    .split(/[._-]+/)
    .filter(Boolean)
    .map((p) => p[0].toUpperCase() + p.slice(1))
    .join(' ')
}

export function AccountMenu() {
  const { email, logout } = useAuth()
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return
    function onPointerDown(e: PointerEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === 'Escape') setOpen(false)
    }
    document.addEventListener('pointerdown', onPointerDown)
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('pointerdown', onPointerDown)
      document.removeEventListener('keydown', onKeyDown)
    }
  }, [open])

  if (!email) {
    return (
      <Button variant="ghost" size="sm" onClick={logout} className="text-muted-foreground gap-1.5">
        <LogOut className="size-3.5" />
        Log out
      </Button>
    )
  }

  const name = displayNameFromEmail(email)
  const initials = initialsFromEmail(email)

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        aria-haspopup="menu"
        aria-expanded={open}
        className="hover:bg-accent flex items-center gap-2 rounded-full py-1 pr-2 pl-1 transition-colors"
      >
        <Avatar initials={initials} />
        <span className="hidden text-sm font-medium sm:inline">{name}</span>
        <ChevronDown className="text-muted-foreground size-4" />
      </button>

      {open && (
        <div
          role="menu"
          className="border-border bg-popover absolute right-0 z-50 mt-2 w-60 rounded-xl border p-1 shadow-lg"
        >
          <div className="flex items-center gap-2.5 px-2 py-2">
            <Avatar initials={initials} />
            <div className="min-w-0">
              <p className="truncate text-sm font-medium">{name}</p>
              <p className="text-muted-foreground truncate text-xs">{email}</p>
            </div>
          </div>
          <div className="bg-border my-1 h-px" />
          <button
            type="button"
            role="menuitem"
            onClick={() => {
              setOpen(false)
              logout()
            }}
            className="text-destructive hover:bg-destructive/10 flex w-full items-center gap-2 rounded-lg px-2 py-1.5 text-sm transition-colors"
          >
            <LogOut className="size-4" />
            Log out
          </button>
        </div>
      )}
    </div>
  )
}

function Avatar({ initials }: { initials: string }) {
  return (
    <span className="bg-primary/15 text-primary flex size-7 shrink-0 items-center justify-center rounded-full text-xs font-semibold">
      {initials}
    </span>
  )
}
