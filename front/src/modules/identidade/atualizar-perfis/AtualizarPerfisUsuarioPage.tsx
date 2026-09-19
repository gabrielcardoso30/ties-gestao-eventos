import { useParams } from 'react-router-dom'
import { EntityFormPage } from '@/shared/components/generic/EntityFormPage'

const perfis = [
  { value: 'Administrador', label: 'Administrador' },
  { value: 'Organizador', label: 'Organizador' },
  { value: 'Participante', label: 'Participante' },
]

export function AtualizarPerfisUsuarioPage() {
  const { id } = useParams()
  return <EntityFormPage titulo="Alterar perfil" descricao="Substitua os perfis de autorização deste usuário." voltarPara="/usuarios" endpoint={`/identidade/usuarios/${id}/perfis`} method="put" initial={{ perfil: 'Participante' }} fields={[
    { name: 'perfil', label: 'Novo perfil', type: 'select', required: true, options: perfis },
  ]} transformar={dados => ({ perfis: [dados.perfil] })} aoSalvar={() => '/usuarios'} invalidar={['usuarios']} />
}
