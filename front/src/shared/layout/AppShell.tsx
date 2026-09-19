import { CalendarDays, MapPin, Mic2, Users, ShieldCheck, ScrollText, LayoutDashboard, LogOut, Menu } from 'lucide-react'
import { Link, NavLink, Outlet } from 'react-router-dom'

import { useAuth } from '@/shared/auth'
import { Button } from '@/shared/components/ui/button'
import { Sheet, SheetContent, SheetTrigger } from '@/shared/components/ui/sheet'
import { cn } from '@/shared/lib/utils'

const itens = [
  ['/', 'Visão geral', LayoutDashboard], ['/eventos', 'Eventos', CalendarDays], ['/palestras', 'Palestras', Mic2],
  ['/pessoas', 'Pessoas', Users], ['/locais', 'Locais', MapPin], ['/usuarios', 'Usuários', ShieldCheck], ['/auditoria', 'Auditoria', ScrollText],
] as const

function Navegacao() {
  return <nav className="space-y-1">{itens.map(([to, label, Icon]) => <NavLink key={to} to={to} end={to === '/'} className={({ isActive }) => cn('flex items-center gap-3 rounded-md px-3 py-2 text-sm transition-colors', isActive ? 'bg-sidebar-primary text-sidebar-primary-foreground' : 'text-sidebar-foreground hover:bg-sidebar-accent hover:text-white')}><Icon className="size-4" />{label}</NavLink>)}</nav>
}

export function AppShell() {
  const { usuario, sair } = useAuth()
  return <div className="min-h-screen bg-muted/35">
    <aside className="fixed inset-y-0 left-0 hidden w-64 bg-sidebar p-5 md:block"><Link to="/" className="mb-8 block text-lg font-bold text-white">Gestão<span className="text-highlight">Eventos</span></Link><Navegacao /></aside>
    <div className="md:pl-64">
      <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b bg-background/95 px-4 backdrop-blur md:px-8">
        <Sheet><SheetTrigger asChild><Button variant="ghost" size="icon" className="md:hidden"><Menu /></Button></SheetTrigger><SheetContent side="left" className="w-64 bg-sidebar p-5"><Navegacao /></SheetContent></Sheet>
        <div className="ml-auto flex items-center gap-3"><div className="hidden text-right sm:block"><p className="text-sm font-medium">{usuario?.usuarioNome}</p><p className="text-xs text-muted-foreground">{usuario?.perfis.join(', ')}</p></div><Button variant="ghost" size="icon" onClick={sair} title="Sair"><LogOut /></Button></div>
      </header>
      <main className="mx-auto max-w-7xl p-4 md:p-8"><Outlet /></main>
    </div>
  </div>
}
