// Detects a cold-start: the API runs on Container Apps with scale-to-zero, so
// the first request after a period of inactivity waits ~10s while a replica
// spins up. We surface a friendly "waking up" banner whenever any request
// stays in flight longer than SLOW_MS, so that delay reads as intentional
// rather than broken. Cleared as soon as no requests are outstanding.

type Listener = (waking: boolean) => void

const SLOW_MS = 2500

const listeners = new Set<Listener>()
let waking = false
let inFlight = 0
let slowTimer: ReturnType<typeof setTimeout> | null = null

function emit() {
  for (const listener of listeners) {
    listener(waking)
  }
}

export function subscribeServerWaking(listener: Listener): () => void {
  listeners.add(listener)
  listener(waking)
  return () => {
    listeners.delete(listener)
  }
}

export function notifyRequestStart() {
  inFlight += 1
  if (inFlight === 1 && slowTimer === null) {
    slowTimer = setTimeout(() => {
      waking = true
      emit()
    }, SLOW_MS)
  }
}

export function notifyRequestEnd() {
  inFlight = Math.max(0, inFlight - 1)
  if (inFlight === 0) {
    if (slowTimer !== null) {
      clearTimeout(slowTimer)
      slowTimer = null
    }
    if (waking) {
      waking = false
      emit()
    }
  }
}
