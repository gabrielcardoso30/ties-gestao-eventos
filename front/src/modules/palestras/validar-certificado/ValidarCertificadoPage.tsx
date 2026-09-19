import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { Award, CalendarDays } from 'lucide-react'
import { api } from '@/shared/api/http'
import { queryKeys } from '@/shared/api/query-keys'
import type { ValidarCertificadoResponse } from '@/shared/api/types'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { Badge } from '@/shared/components/ui/badge'
import { DescriptionList } from '@/shared/components/generic/DescriptionList'
import { ErrorState } from '@/shared/components/generic/ErrorState'
import { LoadingState } from '@/shared/components/generic/LoadingState'
import { formatarDataHora } from '@/shared/lib/format'

/**
 * Página PÚBLICA (sem login) de validação de certificado: qualquer pessoa com o código
 * confirma a autenticidade em `GET /api/v1/palestras/certificados/{codigo}` (endpoint anônimo).
 */
export function ValidarCertificadoPage() {
  const { codigo = '' } = useParams()
  const consulta = useQuery({
    queryKey: queryKeys.palestras.certificado(codigo),
    queryFn: () => api.get<ValidarCertificadoResponse>(`/palestras/certificados/${encodeURIComponent(codigo)}`),
    enabled: codigo.length > 0,
    retry: false,
  })

  return (
    <main className="grid min-h-screen place-items-center bg-sidebar p-4">
      <Card className="w-full max-w-xl">
        <CardHeader>
          <div className="mb-3 grid size-11 place-items-center rounded-lg bg-primary text-primary-foreground"><Award /></div>
          <CardTitle>Validação de certificado</CardTitle>
          <CardDescription>
            Código <span className="font-numeric tracking-widest">{codigo || '—'}</span>
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {consulta.isPending && <LoadingState linhas={5} />}
          {consulta.isError && (
            <ErrorState
              erro={consulta.error}
              titulo="Certificado não encontrado"
              descricao="Confira o código informado. Certificados válidos são emitidos apenas para participantes com presença registrada em palestras já encerradas."
              onTentarNovamente={() => consulta.refetch()}
            />
          )}
          {consulta.isSuccess && (
            <>
              <Badge variant="secondary" className="gap-1"><CalendarDays className="size-3" /> Certificado autêntico</Badge>
              <DescriptionList
                colunas={1}
                itens={[
                  { label: 'Participante', valor: consulta.data.pessoaNome },
                  { label: 'Palestra', valor: consulta.data.palestraTitulo },
                  { label: 'Evento', valor: consulta.data.eventoNome },
                  { label: 'Realizada em', valor: formatarDataHora(consulta.data.palestraInicio) },
                  { label: 'Carga horária', valor: `${consulta.data.certificadoCargaHorariaMinutos} minutos`, numerico: true },
                  { label: 'Emitido em', valor: formatarDataHora(consulta.data.certificadoEmitidoEm) },
                ]}
              />
            </>
          )}
          <p className="text-xs text-muted-foreground">
            Gestão de Eventos · <Link to="/login" className="underline underline-offset-4">Acessar o sistema</Link>
          </p>
        </CardContent>
      </Card>
    </main>
  )
}
