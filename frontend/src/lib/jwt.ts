export function getEmailFromAccessToken(token: string | null): string | null {
  if (!token) return null
  try {
    const payload = token.split('.')[1]
    const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'))
    const claims = JSON.parse(json) as Record<string, unknown>
    return typeof claims.email === 'string' ? claims.email : null
  } catch {
    return null
  }
}
