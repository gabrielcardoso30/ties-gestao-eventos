import { useMutation } from '@tanstack/react-query'
import { Navigate } from 'react-router-dom'
import { useState } from 'react'
import { CalendarDays } from 'lucide-react'
import { api } from '@/shared/api/http'
import type { CriarSessaoResponse } from '@/shared/api/types'
import { useAuth } from '@/shared/auth'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'

export function LoginPage() {
  const { autenticado, entrar } = useAuth()
  const [email, setEmail] = useState('admin@gestaoeventos.local')
  const [senha, setSenha] = useState('Admin@123456')
  const login = useMutation({ mutationFn: () => api.post<CriarSessaoResponse>('/identidade/sessoes', { usuarioEmail: email, senha }, { ignorarNaoAutenticado: true }), onSuccess: entrar })
  if (autenticado) return <Navigate to="/" replace />
  return <main className="grid min-h-screen place-items-center bg-sidebar p-4"><Card className="w-full max-w-md"><CardHeader><div className="mb-3 grid size-11 place-items-center rounded-lg bg-primary text-primary-foreground"><CalendarDays /></div><CardTitle>Gestão de Eventos</CardTitle><CardDescription>Monolito modular, simples para evoluir.</CardDescription></CardHeader><CardContent><form className="space-y-4" onSubmit={(e) => { e.preventDefault(); login.mutate() }}><div className="space-y-2"><Label htmlFor="email">E-mail</Label><Input id="email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required /></div><div className="space-y-2"><Label htmlFor="senha">Senha</Label><Input id="senha" type="password" value={senha} onChange={(e) => setSenha(e.target.value)} required /></div>{login.isError && <p className="text-sm text-destructive">Credenciais inválidas ou API indisponível.</p>}<Button className="w-full" variant="cta" disabled={login.isPending}>{login.isPending ? 'Entrando…' : 'Entrar'}</Button></form></CardContent></Card></main>
}
