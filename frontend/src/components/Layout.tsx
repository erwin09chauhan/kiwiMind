import { Link, Outlet } from 'react-router-dom'
import { Sparkles } from 'lucide-react'
import { AccountMenu } from '@/components/AccountMenu'

export function Layout() {
  return (
    <div className="flex min-h-svh flex-col">
      <header className="bg-background/80 sticky top-0 z-40 border-b backdrop-blur-md">
        <div className="mx-auto flex max-w-4xl items-center justify-between px-4 py-2.5">
          <Link to="/knowledge-bases" className="flex items-center gap-2">
            <span className="bg-primary text-primary-foreground flex size-7 items-center justify-center rounded-lg">
              <Sparkles className="size-4" />
            </span>
            <span className="text-[15px] font-semibold tracking-tight">KiwiMind</span>
          </Link>
          <AccountMenu />
        </div>
      </header>
      <main className="mx-auto flex w-full min-h-0 max-w-4xl flex-1 flex-col px-4 py-8 sm:py-10">
        <Outlet />
      </main>
    </div>
  )
}
