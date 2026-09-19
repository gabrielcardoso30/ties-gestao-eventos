import { EntityFormPage } from '@/shared/components/generic/EntityFormPage'

const perfis = [
  { value: 'Administrador', label: 'Administrador' },
  { value: 'Organizador', label: 'Organizador' },
  { value: 'Participante', label: 'Participante' },
]

export function RegistrarUsuarioPage() {
  return <EntityFormPage titulo="Novo usuário" descricao="Crie uma conta e atribua seu perfil inicial." voltarPara="/usuarios" endpoint="/identidade/usuarios" method="post" initial={{ perfil: 'Participante' }} fields={[
    { name: 'usuarioNome', label: 'Nome', required: true },
    { name: 'usuarioEmail', label: 'E-mail', type: 'email', required: true },
    { name: 'senha', label: 'Senha', type: 'password', required: true },
    { name: 'perfil', label: 'Perfil', type: 'select', required: true, options: perfis },
  ]} transformar={dados => ({ usuarioNome: dados.usuarioNome, usuarioEmail: dados.usuarioEmail, senha: dados.senha, perfis: [dados.perfil] })} aoSalvar={() => '/usuarios'} invalidar={['usuarios']} />
}
