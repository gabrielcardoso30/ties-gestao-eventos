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
import { CriarLocalPage } from '@/modules/locais/criar/CriarLocalPage'
import { AtualizarLocalPage } from '@/modules/locais/atualizar/AtualizarLocalPage'
import { ObterLocalPage } from '@/modules/locais/obter/ObterLocalPage'
import { CriarPessoaPage } from '@/modules/pessoas/criar/CriarPessoaPage'
import { AtualizarPessoaPage } from '@/modules/pessoas/atualizar/AtualizarPessoaPage'
import { ObterPessoaPage } from '@/modules/pessoas/obter/ObterPessoaPage'
import { CriarEventoPage } from '@/modules/eventos/criar/CriarEventoPage'
import { AtualizarEventoPage } from '@/modules/eventos/atualizar/AtualizarEventoPage'
import { ObterEventoPage } from '@/modules/eventos/obter/ObterEventoPage'
import { CriarPalestraPage } from '@/modules/palestras/criar/CriarPalestraPage'
import { AtualizarPalestraPage } from '@/modules/palestras/atualizar/AtualizarPalestraPage'
import { ObterPalestraPage } from '@/modules/palestras/obter/ObterPalestraPage'
import { RegistrarUsuarioPage } from '@/modules/identidade/registrar/RegistrarUsuarioPage'
import { AtualizarPerfisUsuarioPage } from '@/modules/identidade/atualizar-perfis/AtualizarPerfisUsuarioPage'
import { ObterRegistroAuditoriaPage } from '@/modules/auditoria/obter/ObterRegistroAuditoriaPage'

const queryClient = new QueryClient({ defaultOptions: { queries: { staleTime: 30_000, retry: 1 } } })
function RotaProtegida() { const { autenticado } = useAuth(); return autenticado ? <Outlet /> : <Navigate to="/login" replace /> }
export default function App() { return <ThemeProvider><QueryClientProvider client={queryClient}><AuthProvider><BrowserRouter><Routes><Route path="/login" element={<LoginPage />} /><Route element={<RotaProtegida />}><Route element={<AppShell />}><Route index element={<DashboardPage />} />
  <Route path="locais" element={<ListarLocaisPage />} /><Route path="locais/novo" element={<CriarLocalPage />} /><Route path="locais/:id" element={<ObterLocalPage />} /><Route path="locais/:id/editar" element={<AtualizarLocalPage />} />
  <Route path="pessoas" element={<ListarPessoasPage />} /><Route path="pessoas/nova" element={<CriarPessoaPage />} /><Route path="pessoas/:id" element={<ObterPessoaPage />} /><Route path="pessoas/:id/editar" element={<AtualizarPessoaPage />} />
  <Route path="eventos" element={<ListarEventosPage />} /><Route path="eventos/novo" element={<CriarEventoPage />} /><Route path="eventos/:id" element={<ObterEventoPage />} /><Route path="eventos/:id/editar" element={<AtualizarEventoPage />} />
  <Route path="palestras" element={<ListarPalestrasPage />} /><Route path="palestras/nova" element={<CriarPalestraPage />} /><Route path="palestras/:id" element={<ObterPalestraPage />} /><Route path="palestras/:id/editar" element={<AtualizarPalestraPage />} />
  <Route path="usuarios" element={<ListarUsuariosPage />} /><Route path="usuarios/novo" element={<RegistrarUsuarioPage />} /><Route path="usuarios/:id/perfis" element={<AtualizarPerfisUsuarioPage />} />
  <Route path="auditoria" element={<ListarAuditoriaPage />} /><Route path="auditoria/:id" element={<ObterRegistroAuditoriaPage />} />
  </Route></Route><Route path="*" element={<Navigate to="/" replace />} /></Routes></BrowserRouter><Toaster richColors /></AuthProvider></QueryClientProvider></ThemeProvider> }
