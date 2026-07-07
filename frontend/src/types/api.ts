export interface AuthResult {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
}

export interface KnowledgeBaseDto {
  id: string
  name: string
  createdAt: string
  documentCount: number
}

export type DocumentStatus = 'Queued' | 'Processing' | 'Ready' | 'Failed'

export interface DocumentDto {
  id: string
  fileName: string
  status: DocumentStatus
  pageCount: number | null
  createdAt: string
}

export interface Citation {
  documentId: string
  chunkId: string
  page: number | null
}

export type MessageRole = 'User' | 'Assistant'

export interface MessageDto {
  id: string
  role: MessageRole
  content: string
  citations: Citation[]
  tokensUsed: number
  createdAt: string
}

export interface ConversationDto {
  id: string
  knowledgeBaseId: string
  title: string
  createdAt: string
  messageCount: number
}

export interface ConversationDetailDto {
  id: string
  knowledgeBaseId: string
  title: string
  createdAt: string
  messages: MessageDto[]
}
