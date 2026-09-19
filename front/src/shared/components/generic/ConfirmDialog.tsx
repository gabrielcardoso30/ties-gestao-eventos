import { Loader2 } from 'lucide-react'
import type { ReactNode } from 'react'

import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog'
import { Button } from '@/shared/components/ui/button'
import { ErrorState } from './ErrorState'

export interface ConfirmDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  titulo: string
  descricao?: ReactNode
  confirmarLabel?: string
  cancelarLabel?: string
  /** `destructive` para exclusões e cancelamentos. */
  variante?: 'default' | 'destructive'
  onConfirmar: () => void | Promise<unknown>
  isPending?: boolean
  /** Erro da última tentativa (exibido dentro do diálogo). */
  erro?: unknown
  /** Conteúdo extra (ex.: campo de motivo). */
  children?: ReactNode
  /** Desabilita o botão de confirmar (ex.: motivo obrigatório vazio). */
  confirmarDesabilitado?: boolean
}

/** Diálogo de confirmação para ações irreversíveis ou sensíveis. */
export function ConfirmDialog({
  open,
  onOpenChange,
  titulo,
  descricao,
  confirmarLabel = 'Confirmar',
  cancelarLabel = 'Cancelar',
  variante = 'default',
  onConfirmar,
  isPending,
  erro,
  children,
  confirmarDesabilitado,
}: ConfirmDialogProps) {
  return (
    <AlertDialog open={open} onOpenChange={(v) => !isPending && onOpenChange(v)}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{titulo}</AlertDialogTitle>
          {descricao && <AlertDialogDescription>{descricao}</AlertDialogDescription>}
        </AlertDialogHeader>
        {children}
        {erro ? <ErrorState erro={erro} compacto /> : null}
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isPending}>{cancelarLabel}</AlertDialogCancel>
          <Button type="button" variant={variante} disabled={isPending || confirmarDesabilitado} onClick={() => void onConfirmar()}>
            {isPending && <Loader2 className="animate-spin" />}
            {confirmarLabel}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
