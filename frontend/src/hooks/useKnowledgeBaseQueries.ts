import { useQuery } from '@tanstack/react-query'
import { conversationsApi, documentsApi, knowledgeBasesApi } from '@/lib/api'

export function useKnowledgeBase(knowledgeBaseId: string) {
  return useQuery({
    queryKey: ['knowledge-bases', knowledgeBaseId],
    queryFn: () => knowledgeBasesApi.get(knowledgeBaseId),
  })
}

export function useDocuments(knowledgeBaseId: string) {
  return useQuery({
    queryKey: ['documents', knowledgeBaseId],
    queryFn: () => documentsApi.list(knowledgeBaseId),
    refetchInterval: 3000,
  })
}

export function useConversations(knowledgeBaseId: string) {
  return useQuery({
    queryKey: ['conversations', knowledgeBaseId],
    queryFn: () => conversationsApi.list(knowledgeBaseId),
  })
}
