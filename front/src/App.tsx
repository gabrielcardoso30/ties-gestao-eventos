import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Outlet, Route, Routes } from 'react-router-dom'
import { AuthProvider, useAuth } from '@/shared/auth'
import { AppShell } from '@/shared/layout/AppShell'
import { Toaster } from '@/shared/components/ui/sonner'
import { ThemeProvider } from '@/shared/lib/theme'
import { LoginPage } from '@/modules/identidade/criar-sessao/LoginPage'
import { DashboardPage } from '@/modules/dashboard/visualizar/DashboardPage'
import { ListarLocaisPage } from '@/modules/locais/listar/ListarLocaisPage'
import { ListarPessoasPage } from '@/modules/pessoas/listar/ListarPessoasPage'
import { ListarEventosPage } from '@/modules/eventos/listar/ListarEventosPage'
import { ListarPalestrasPage } from '@/modules/palestras/listar/ListarPalestrasPage'
import { ListarUsuariosPage } from '@/modules/identidade/listar/ListarUsuariosPage'
import { ListarAuditoriaPage } from '@/modules/auditoria/listar/ListarAuditoriaPage'

const queryClient = new QueryClient({ defaultOptions: { queries: { staleTime: 30_000, retry: 1 } } })
function RotaProtegida() { const { autenticado } = useAuth(); return autenticado ? <Outlet /> : <Navigate to="/login" replace /> }
export default function App() { return <ThemeProvider><QueryClientProvider client={queryClient}><AuthProvider><BrowserRouter><Routes><Route path="/login" element={<LoginPage />} /><Route element={<RotaProtegida />}><Route element={<AppShell />}><Route index element={<DashboardPage />} /><Route path="locais" element={<ListarLocaisPage />} /><Route path="pessoas" element={<ListarPessoasPage />} /><Route path="eventos" element={<ListarEventosPage />} /><Route path="palestras" element={<ListarPalestrasPage />} /><Route path="usuarios" element={<ListarUsuariosPage />} /><Route path="auditoria" element={<ListarAuditoriaPage />} /></Route></Route><Route path="*" element={<Navigate to="/" replace />} /></Routes></BrowserRouter><Toaster richColors /></AuthProvider></QueryClientProvider></ThemeProvider> }
