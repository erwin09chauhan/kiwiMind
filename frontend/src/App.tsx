import { Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from '@/components/Layout'
import { ProtectedRoute } from '@/components/ProtectedRoute'
import { LoginPage } from '@/pages/LoginPage'
import { RegisterPage } from '@/pages/RegisterPage'
import { KnowledgeBasesPage } from '@/pages/KnowledgeBasesPage'
import { KnowledgeBaseDetailPage } from '@/pages/KnowledgeBaseDetailPage'
import { ConversationPage } from '@/pages/ConversationPage'

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          <Route path="/knowledge-bases" element={<KnowledgeBasesPage />} />
          <Route path="/knowledge-bases/:id" element={<KnowledgeBaseDetailPage />} />
          <Route path="/knowledge-bases/:id/conversations/:conversationId" element={<ConversationPage />} />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/knowledge-bases" replace />} />
    </Routes>
  )
}
