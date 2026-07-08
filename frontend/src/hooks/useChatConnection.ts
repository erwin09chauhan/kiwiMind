import { useEffect, useRef, useState } from 'react'
import * as signalR from '@microsoft/signalr'
import { API_BASE_URL } from '@/lib/api-client'
import { tokenStorage } from '@/lib/token-storage'
import type { MessageDto } from '@/types/api'

export function useChatConnection(knowledgeBaseId: string, conversationId: string) {
  const connectionRef = useRef<signalR.HubConnection | null>(null)
  const [streamingText, setStreamingText] = useState<string | null>(null)
  const [completedMessage, setCompletedMessage] = useState<MessageDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isConnected, setIsConnected] = useState(false)

  useEffect(() => {
    let cancelled = false

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/chat?access_token=${tokenStorage.getAccessToken()}`, {
        withCredentials: false,
      })
      .withAutomaticReconnect()
      .build()

    connection.on('ReceiveToken', (token: string) => {
      setStreamingText((prev) => (prev ?? '') + token)
    })

    connection.on('MessageComplete', (message: MessageDto) => {
      setStreamingText(null)
      setCompletedMessage(message)
    })

    connection.on('Error', (message: string) => {
      setStreamingText(null)
      setError(message)
    })

    connection
      .start()
      .then(() => {
        if (!cancelled) setIsConnected(true)
      })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to connect to chat.')
      })

    connectionRef.current = connection

    return () => {
      cancelled = true
      void connection.stop()
      connectionRef.current = null
    }
  }, [knowledgeBaseId, conversationId])

  async function sendMessage(content: string) {
    setError(null)
    setStreamingText('')
    try {
      await connectionRef.current?.invoke('SendMessage', knowledgeBaseId, conversationId, content)
    } catch (err) {
      setStreamingText(null)
      setError(err instanceof Error ? err.message : 'Failed to send message.')
    }
  }

  return { isConnected, streamingText, completedMessage, error, sendMessage, clearError: () => setError(null) }
}
