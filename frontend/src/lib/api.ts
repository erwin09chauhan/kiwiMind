import { apiClient } from './api-client'
import type {
  AuthResult,
  ConversationDetailDto,
  ConversationDto,
  DocumentDto,
  KnowledgeBaseDto,
  MessageDto,
} from '@/types/api'

export const authApi = {
  register: (email: string, password: string) =>
    apiClient.post<AuthResult>('/api/auth/register', { email, password }).then((r) => r.data),
  login: (email: string, password: string) =>
    apiClient.post<AuthResult>('/api/auth/login', { email, password }).then((r) => r.data),
}

export const knowledgeBasesApi = {
  list: () => apiClient.get<KnowledgeBaseDto[]>('/api/knowledge-bases').then((r) => r.data),
  get: (id: string) => apiClient.get<KnowledgeBaseDto>(`/api/knowledge-bases/${id}`).then((r) => r.data),
  create: (name: string) =>
    apiClient.post<KnowledgeBaseDto>('/api/knowledge-bases', { name }).then((r) => r.data),
  delete: (id: string) => apiClient.delete(`/api/knowledge-bases/${id}`),
}

export const documentsApi = {
  list: (knowledgeBaseId: string) =>
    apiClient.get<DocumentDto[]>(`/api/knowledge-bases/${knowledgeBaseId}/documents`).then((r) => r.data),
  upload: (knowledgeBaseId: string, file: File) => {
    const formData = new FormData()
    formData.append('file', file)
    return apiClient
      .post<DocumentDto>(`/api/knowledge-bases/${knowledgeBaseId}/documents`, formData)
      .then((r) => r.data)
  },
  delete: (knowledgeBaseId: string, documentId: string) =>
    apiClient.delete(`/api/knowledge-bases/${knowledgeBaseId}/documents/${documentId}`),
}

export const conversationsApi = {
  list: (knowledgeBaseId: string) =>
    apiClient
      .get<ConversationDto[]>(`/api/knowledge-bases/${knowledgeBaseId}/conversations`)
      .then((r) => r.data),
  get: (knowledgeBaseId: string, conversationId: string) =>
    apiClient
      .get<ConversationDetailDto>(`/api/knowledge-bases/${knowledgeBaseId}/conversations/${conversationId}`)
      .then((r) => r.data),
  create: (knowledgeBaseId: string, title: string) =>
    apiClient
      .post<ConversationDto>(`/api/knowledge-bases/${knowledgeBaseId}/conversations`, { title })
      .then((r) => r.data),
  delete: (knowledgeBaseId: string, conversationId: string) =>
    apiClient.delete(`/api/knowledge-bases/${knowledgeBaseId}/conversations/${conversationId}`),
  sendMessage: (knowledgeBaseId: string, conversationId: string, content: string) =>
    apiClient
      .post<MessageDto>(`/api/knowledge-bases/${knowledgeBaseId}/conversations/${conversationId}/messages`, {
        content,
      })
      .then((r) => r.data),
}
