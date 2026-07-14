import { useEffect, useRef, type KeyboardEvent } from 'react'
import { ArrowUp } from 'lucide-react'
import { Button } from '@/components/ui/button'

export function ChatComposer({
  value,
  onChange,
  onSubmit,
  disabled,
}: {
  value: string
  onChange: (value: string) => void
  onSubmit: () => void
  disabled: boolean
}) {
  const textareaRef = useRef<HTMLTextAreaElement>(null)

  useEffect(() => {
    const el = textareaRef.current
    if (!el) return
    el.style.height = 'auto'
    el.style.height = `${Math.min(el.scrollHeight, 160)}px`
  }, [value])

  function handleKeyDown(e: KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      if (!disabled && value.trim()) onSubmit()
    }
  }

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault()
        if (!disabled && value.trim()) onSubmit()
      }}
      className="border-input bg-card focus-within:border-ring focus-within:ring-ring/50 relative flex items-end rounded-2xl border px-3 py-2.5 shadow-sm transition-all focus-within:ring-[3px]"
    >
      <textarea
        ref={textareaRef}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Ask anything about this knowledge base…"
        rows={1}
        disabled={disabled}
        className="placeholder:text-muted-foreground max-h-40 w-full resize-none bg-transparent pr-10 text-base leading-relaxed outline-none disabled:opacity-60 md:text-[15px]"
      />
      <Button
        type="submit"
        size="icon"
        disabled={disabled || !value.trim()}
        aria-label="Send message"
        className="absolute right-2 bottom-2 size-8 rounded-full"
      >
        <ArrowUp className="size-4" />
      </Button>
    </form>
  )
}
