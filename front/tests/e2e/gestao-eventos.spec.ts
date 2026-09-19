import { expect, test } from '@playwright/test'

let sessaoAdministrador = ''

test.beforeAll(async ({ request }) => {
  const response = await request.post('/api/v1/identidade/sessoes', { data: { usuarioEmail: 'admin@gestaoeventos.local', senha: 'Admin@123456' } })
  expect(response.ok()).toBeTruthy()
  sessaoAdministrador = JSON.stringify(await response.json())
})

test.beforeEach(async ({ page }) => {
  await page.addInitScript(sessao => localStorage.setItem('gestao-eventos.auth', sessao), sessaoAdministrador)
})

test('regressão: navega da listagem para detalhe e edição', async ({ page }) => {
  await page.goto('/locais')
  await expect(page.getByRole('link', { name: 'Novo registro' })).toBeVisible()
  const primeiraLinha = page.locator('tbody tr').first()
  await expect(primeiraLinha).toBeVisible()
  await primeiraLinha.click()
  await expect(page.getByRole('link', { name: 'Editar' })).toBeVisible()
  await expect(page.getByRole('button', { name: 'Excluir' }).first()).toBeVisible()
  await page.getByRole('link', { name: 'Editar' }).click()
  await expect(page.getByRole('heading', { name: 'Editar local' })).toBeVisible()
  await expect(page.getByLabel('Nome do local *')).not.toHaveValue('')
})

test('CRUD de pessoa: cria, consulta, edita e exclui logicamente', async ({ page }) => {
  const sufixo = Date.now().toString()
  const nome = `Pessoa Playwright ${sufixo}`
  const nomeAlterado = `${nome} Editada`
  await page.goto('/pessoas/nova')
  await page.getByLabel('Nome *').fill(nome)
  await page.getByLabel('E-mail *').fill(`playwright-${sufixo}@teste.local`)
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByRole('heading', { name: nome })).toBeVisible()
  await page.getByRole('link', { name: 'Editar' }).click()
  await page.getByLabel('Nome *').fill(nomeAlterado)
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByRole('heading', { name: nomeAlterado })).toBeVisible()
  await page.getByRole('button', { name: 'Excluir' }).first().click()
  await page.getByRole('button', { name: 'Excluir' }).last().click()
  await expect(page.getByRole('heading', { name: 'Pessoas' })).toBeVisible()
  await expect(page.getByText(nomeAlterado)).toHaveCount(0)
})

test('auditoria permite abrir o registro sem ações de alteração', async ({ page }) => {
  await page.goto('/auditoria')
  const primeiraLinha = page.locator('tbody tr').first()
  await expect(primeiraLinha).toBeVisible()
  await primeiraLinha.click()
  await expect(page.getByText('Estado anterior')).toBeVisible()
  await expect(page.getByText('Estado novo')).toBeVisible()
  await expect(page.getByRole('link', { name: 'Editar' })).toHaveCount(0)
  await expect(page.getByRole('button', { name: 'Excluir' })).toHaveCount(0)
})

test('rota protegida redireciona sessão ausente para o login', async ({ page }) => {
  await page.addInitScript(() => localStorage.removeItem('gestao-eventos.auth'))
  await page.goto('/eventos')
  await expect(page).toHaveURL(/\/login$/)
  await expect(page.getByRole('button', { name: 'Entrar' })).toBeVisible()
})

test('login inválido apresenta erro e não autentica', async ({ page }) => {
  await page.addInitScript(() => localStorage.removeItem('gestao-eventos.auth'))
  await page.goto('/login')
  await page.getByLabel('Senha').fill('senha-incorreta')
  await page.getByRole('button', { name: 'Entrar' }).click()
  await expect(page.getByText('Credenciais inválidas ou API indisponível.')).toBeVisible()
  await expect(page).toHaveURL(/\/login$/)
})

test('todos os módulos expõem suas ações principais', async ({ page }) => {
  for (const [rota, titulo] of [['/locais', 'Locais'], ['/pessoas', 'Pessoas'], ['/eventos', 'Eventos'], ['/palestras', 'Palestras'], ['/usuarios', 'Usuários']] as const) {
    await page.goto(rota)
    await expect(page.getByRole('heading', { name: titulo })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Novo registro' })).toBeVisible()
  }
  await page.goto('/auditoria')
  await expect(page.getByRole('heading', { name: 'Auditoria' })).toBeVisible()
  await expect(page.getByRole('link', { name: 'Novo registro' })).toHaveCount(0)
})

test('CRUD de local cria ambiente único e permite exclusão', async ({ page }) => {
  const nome = `Local Playwright ${Date.now()}`
  await page.goto('/locais/novo')
  await page.getByLabel('Nome do local *').fill(nome)
  await page.getByLabel('Cidade *').fill('Vila Velha')
  await page.getByLabel('UF *').fill('ES')
  await page.getByLabel('Capacidade do ambiente único').fill('80')
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByRole('heading', { name: nome })).toBeVisible()
  await expect(page.getByText('Ambiente único', { exact: true })).toBeVisible()
  await expect(page.getByText('80 pessoas')).toBeVisible()
  await page.getByRole('button', { name: 'Excluir' }).first().click()
  await page.getByRole('button', { name: 'Excluir' }).last().click()
  await expect(page.getByRole('heading', { name: 'Locais' })).toBeVisible()
})

test('CRUD de evento remoto preserva formato e situação inicial', async ({ page }) => {
  const nome = `Evento Playwright ${Date.now()}`
  const inicio = new Date(Date.now() + 2 * 86400000)
  const fim = new Date(inicio.getTime() + 2 * 3600000)
  const valorData = (data: Date) => new Date(data.getTime() - data.getTimezoneOffset() * 60000).toISOString().slice(0, 16)
  await page.goto('/eventos/novo')
  await page.getByLabel('Nome do evento').fill(nome)
  await page.getByLabel('Formato').selectOption('Remoto')
  await page.getByLabel('Início').fill(valorData(inicio))
  await page.getByLabel('Fim').fill(valorData(fim))
  await page.getByLabel('Link remoto').fill('https://evento.teste.local/sala')
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByRole('heading', { name: nome })).toBeVisible()
  await expect(page.getByText('Rascunho')).toBeVisible()
  await expect(page.getByText('Remoto', { exact: true })).toBeVisible()
  await page.getByRole('button', { name: 'Excluir' }).first().click()
  await page.getByRole('button', { name: 'Excluir' }).last().click()
  await expect(page.getByRole('heading', { name: 'Eventos' })).toBeVisible()
})

test('administrador cadastra usuário participante pela interface', async ({ page }) => {
  const sufixo = Date.now()
  const nome = `Usuário Playwright ${sufixo}`
  await page.goto('/usuarios/novo')
  await page.getByLabel('Nome *').fill(nome)
  await page.getByLabel('E-mail *').fill(`usuario-${sufixo}@teste.local`)
  await page.getByLabel('Senha *').fill('Senha@123456')
  await page.getByLabel('Perfil *').selectOption('Participante')
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByRole('heading', { name: 'Usuários' })).toBeVisible()
  await page.getByPlaceholder('Buscar em usuários').fill(nome)
  await expect(page.getByText(nome)).toBeVisible()
})

test('Locais: busca reduz a listagem ao registro desejado', async ({ page, request }) => {
  const nome = `Busca Exclusiva ${Date.now()}`
  const sessao = JSON.parse(sessaoAdministrador) as { accessToken: string }
  const criado = await request.post('/api/v1/locais', { headers: { Authorization: `Bearer ${sessao.accessToken}` }, data: { localNome: nome, enderecoCidade: 'Vila Velha', enderecoUf: 'ES', capacidadeAmbienteUnico: 15 } })
  expect(criado.status()).toBe(201)
  await page.goto('/locais')
  await page.getByPlaceholder('Buscar em locais').fill(nome)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await expect(page.getByText(nome, { exact: true })).toBeVisible()
})

test('Pessoas: campos obrigatórios impedem submissão vazia e cancelar retorna à lista', async ({ page }) => {
  await page.goto('/pessoas/nova')
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page).toHaveURL(/\/pessoas\/nova$/)
  await expect(page.getByLabel('Nome *')).toHaveAttribute('required', '')
  await expect(page.getByLabel('E-mail *')).toHaveAttribute('required', '')
  await page.getByRole('button', { name: 'Cancelar' }).click()
  await expect(page).toHaveURL(/\/pessoas$/)
})

test('Eventos: detalhe inexistente apresenta estado de erro recuperável', async ({ page }) => {
  await page.goto('/eventos/01999999-9999-7999-8999-999999999999')
  await expect(page.getByText(/não encontrado|erro|não foi possível/i).first()).toBeVisible()
})

test('Palestras: usuário abre detalhe e visualiza palestrantes e conteúdos', async ({ page }) => {
  await page.goto('/palestras')
  const primeiraLinha = page.locator('tbody tr').first()
  await expect(primeiraLinha).toBeVisible()
  await primeiraLinha.click()
  await expect(page.getByText(/Palestrantes \(\d+\)/)).toBeVisible()
  await expect(page.getByText(/Conteúdos \(\d+\)/)).toBeVisible()
  await expect(page.getByRole('link', { name: 'Editar' })).toBeVisible()
})

test('Identidade: administrador altera o perfil de um usuário', async ({ page, request }) => {
  const sufixo = Date.now()
  const nome = `Perfil Playwright ${sufixo}`
  const sessao = JSON.parse(sessaoAdministrador) as { accessToken: string }
  const criado = await request.post('/api/v1/identidade/usuarios', { headers: { Authorization: `Bearer ${sessao.accessToken}` }, data: { usuarioNome: nome, usuarioEmail: `perfil-${sufixo}@teste.local`, senha: 'Senha@123456', perfis: ['Participante'] } })
  expect(criado.status()).toBe(201)
  const usuario = await criado.json() as { id: string }
  await page.goto(`/usuarios/${usuario.id}/perfis`)
  await page.getByLabel('Novo perfil *').selectOption('Organizador')
  await page.getByRole('button', { name: 'Salvar' }).click()
  await page.getByPlaceholder('Buscar em usuários').fill(nome)
  await expect(page.getByRole('row', { name: new RegExp(`${nome}.*Organizador`) })).toBeVisible()
})

test('Auditoria: registro detalha identidade, correlação e estados', async ({ page }) => {
  await page.goto('/auditoria')
  await page.locator('tbody tr').first().click()
  await expect(page.getByText('Identificador')).toBeVisible()
  await expect(page.getByText('Trace ID')).toBeVisible()
  await expect(page.getByText('Estado anterior')).toBeVisible()
  await expect(page.getByText('Estado novo')).toBeVisible()
})

test('Palestras: dropdowns relacionam evento, sala e múltiplos palestrantes na criação e edição', async ({ page, request }) => {
  const sufixo = Date.now(); const sessao = JSON.parse(sessaoAdministrador) as { accessToken: string }
  const headers = { Authorization: `Bearer ${sessao.accessToken}` }
  const post = async (url: string, data: unknown) => { const r = await request.post(url, { headers, data }); expect(r.ok()).toBeTruthy(); return r.json() }
  const local = await post('/api/v1/locais', { localNome: `Local Relações ${sufixo}`, enderecoCidade: 'Vila Velha', enderecoUf: 'ES', capacidadeAmbienteUnico: 120 }) as { id: string }
  const localDetalhe = await (await request.get(`/api/v1/locais/${local.id}`, { headers })).json() as { salas: Array<{ id: string }> }
  const criarPessoa = (numero: number) => post('/api/v1/pessoas', { pessoaNome: `Palestrante ${numero} ${sufixo}`, pessoaEmail: `palestrante-${numero}-${sufixo}@teste.local` }) as Promise<{ id: string }>
  const [pessoa1, pessoa2, pessoa3] = await Promise.all([criarPessoa(1), criarPessoa(2), criarPessoa(3)])
  const inicioEvento = new Date(Date.now() + 5 * 86400000); const fimEvento = new Date(inicioEvento.getTime() + 8 * 3600000)
  const evento = await post('/api/v1/eventos', { eventoNome: `Evento Relações ${sufixo}`, eventoDataInicio: inicioEvento.toISOString(), eventoDataFim: fimEvento.toISOString(), eventoFormato: 'Presencial', localId: local.id }) as { id: string }
  const eventoDetalhe = await (await request.get(`/api/v1/eventos/${evento.id}`, { headers })).json() as { trilhas: Array<{ id: string }> }
  const valorData = (data: Date) => new Date(data.getTime() - data.getTimezoneOffset() * 60000).toISOString().slice(0, 16)

  await page.goto('/palestras/nova')
  await page.getByLabel('Título').fill(`Palestra Relações ${sufixo}`)
  await page.getByLabel('Evento', { exact: true }).selectOption(evento.id)
  await page.getByLabel('Trilha').selectOption(eventoDetalhe.trilhas[0].id)
  await expect(page.getByLabel('Local do evento')).toHaveValue(local.id)
  await page.getByLabel('Sala').selectOption(localDetalhe.salas[0].id)
  await page.getByLabel('Início').fill(valorData(new Date(inicioEvento.getTime() + 3600000)))
  await page.getByLabel('Fim').fill(valorData(new Date(inicioEvento.getTime() + 2 * 3600000)))
  await page.getByLabel('Pessoa palestrante').selectOption(pessoa1.id)
  await page.getByRole('button', { name: 'Adicionar' }).click()
  await page.getByLabel('Pessoa palestrante').selectOption(pessoa2.id)
  await page.getByLabel('Papel do palestrante').selectOption('Coautor')
  await page.getByRole('button', { name: 'Adicionar' }).click()
  await expect(page.getByText('Palestrantes (2)')).toBeVisible()
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByText('Trilha única', { exact: true })).toBeVisible()
  await expect(page.getByText(`Palestrante 1 ${sufixo}`)).toBeVisible()
  await expect(page.getByText(`Palestrante 2 ${sufixo}`)).toBeVisible()

  await page.getByRole('link', { name: 'Editar' }).click()
  await expect(page.getByLabel('Evento', { exact: true })).toBeDisabled()
  await page.getByLabel('Pessoa palestrante').selectOption(pessoa3.id)
  await page.getByLabel('Papel do palestrante').selectOption('Mediador')
  await page.getByRole('button', { name: 'Adicionar' }).click()
  await page.getByRole('button', { name: `Remover Palestrante 1 ${sufixo}` }).click()
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByText(`Palestrante 1 ${sufixo}`)).toHaveCount(0)
  await expect(page.getByText(`Palestrante 2 ${sufixo}`)).toBeVisible()
  await expect(page.getByText(`Palestrante 3 ${sufixo}`)).toBeVisible()
})

test('Locais: usuário adiciona, edita e exclui uma sala', async ({ page, request }) => {
  const sufixo = Date.now(); const sessao = JSON.parse(sessaoAdministrador) as { accessToken: string }
  const response = await request.post('/api/v1/locais', { headers: { Authorization: `Bearer ${sessao.accessToken}` }, data: { localNome: `Local Salas ${sufixo}`, enderecoCidade: 'Vila Velha', enderecoUf: 'ES', capacidadeAmbienteUnico: 50 } })
  expect(response.status()).toBe(201); const local = await response.json() as { id: string }
  await page.goto(`/locais/${local.id}`)
  await page.getByRole('button', { name: 'Nova sala' }).click()
  await page.getByLabel('Nome da sala *').fill('Sala Azul')
  await page.getByLabel('Capacidade *').fill('30')
  await page.getByLabel('Tipo *').selectOption('SalaAula')
  await page.getByLabel('Recursos').fill('Projetor e internet')
  await page.getByRole('button', { name: 'Salvar sala' }).click()
  await expect(page.getByText('Salas (2)')).toBeVisible()
  await expect(page.getByText('Sala Azul')).toBeVisible()
  const sala = page.locator('div.divide-y > div').filter({ hasText: 'Sala Azul' })
  await sala.getByRole('button', { name: 'Editar' }).click()
  await page.getByLabel('Nome da sala *').fill('Sala Azul Atualizada')
  await page.getByLabel('Capacidade *').fill('35')
  await page.getByRole('button', { name: 'Salvar sala' }).click()
  await expect(page.getByText('Sala Azul Atualizada')).toBeVisible()
  await expect(page.getByText(/35 pessoas/)).toBeVisible()
  const salaAtualizada = page.locator('div.divide-y > div').filter({ hasText: 'Sala Azul Atualizada' })
  await salaAtualizada.getByRole('button', { name: 'Excluir' }).click()
  await page.getByRole('button', { name: 'Excluir sala' }).click()
  await expect(page.getByText('Sala Azul Atualizada')).toHaveCount(0)
  await expect(page.getByText('Salas (1)')).toBeVisible()
})

test('Eventos: dropdown permite selecionar e trocar o local cadastrado', async ({ page, request }) => {
  const sufixo = Date.now(); const sessao = JSON.parse(sessaoAdministrador) as { accessToken: string }; const headers = { Authorization: `Bearer ${sessao.accessToken}` }
  const criarLocal = async (numero: number) => { const r = await request.post('/api/v1/locais', { headers, data: { localNome: `Local Evento ${numero} ${sufixo}`, enderecoCidade: 'Vila Velha', enderecoUf: 'ES', capacidadeAmbienteUnico: 100 } }); expect(r.status()).toBe(201); return r.json() as Promise<{ id: string }> }
  const [local1, local2] = await Promise.all([criarLocal(1), criarLocal(2)])
  const inicio = new Date(Date.now() + 7 * 86400000); const fim = new Date(inicio.getTime() + 3 * 3600000)
  const valorData = (data: Date) => new Date(data.getTime() - data.getTimezoneOffset() * 60000).toISOString().slice(0, 16)
  const nome = `Evento com Local ${sufixo}`
  await page.goto('/eventos/novo')
  await page.getByLabel('Nome do evento').fill(nome)
  await page.getByLabel('Formato').selectOption('Presencial')
  await page.getByLabel('Local (presencial/híbrido)').selectOption(local1.id)
  await page.getByLabel('Início').fill(valorData(inicio)); await page.getByLabel('Fim').fill(valorData(fim))
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByText(`Local Evento 1 ${sufixo}`)).toBeVisible()
  await page.getByRole('link', { name: 'Editar' }).click()
  await expect(page.getByLabel('Local (presencial/híbrido)')).toHaveValue(local1.id)
  await page.getByLabel('Local (presencial/híbrido)').selectOption(local2.id)
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByText(`Local Evento 2 ${sufixo}`)).toBeVisible()
})

test('Eventos e Palestras: usuário gerencia trilhas e seleciona a trilha da palestra', async ({ page, request }) => {
  const sufixo = Date.now(); const sessao = JSON.parse(sessaoAdministrador) as { accessToken: string }; const headers = { Authorization: `Bearer ${sessao.accessToken}` }
  const inicio = new Date(Date.now() + 9 * 86400000); const fim = new Date(inicio.getTime() + 4 * 3600000)
  const resposta = await request.post('/api/v1/eventos', { headers, data: { eventoNome: `Evento Trilhas ${sufixo}`, eventoDataInicio: inicio.toISOString(), eventoDataFim: fim.toISOString(), eventoFormato: 'Remoto', eventoLinkRemoto: 'https://evento.test', trilhas: [{ trilhaNome: 'Arquitetura', trilhaCor: '#112233' }] } })
  expect(resposta.status()).toBe(201); const evento = await resposta.json() as { id: string }
  await page.goto(`/eventos/${evento.id}`)
  await expect(page.getByText('Trilhas (1)')).toBeVisible()
  await page.getByRole('button', { name: 'Nova trilha' }).click()
  await page.getByLabel('Nome *').fill('Cloud')
  await page.getByLabel('Descrição').fill('Nuvem e plataforma')
  await page.getByRole('button', { name: 'Salvar trilha' }).click()
  await expect(page.getByText('Trilhas (2)')).toBeVisible(); await expect(page.getByText('Cloud', { exact: true })).toBeVisible()
  const cloud = page.locator('div.divide-y > div').filter({ hasText: 'Cloud' }); await cloud.getByRole('button', { name: 'Editar' }).click()
  await page.getByLabel('Nome *').fill('Cloud Native'); await page.getByRole('button', { name: 'Salvar trilha' }).click()
  await expect(page.getByText('Cloud Native', { exact: true })).toBeVisible()
  await page.goto('/palestras/nova'); await page.getByLabel('Evento', { exact: true }).selectOption(evento.id)
  await expect(page.getByLabel('Trilha')).toContainText('Arquitetura'); await expect(page.getByLabel('Trilha')).toContainText('Cloud Native')
})

test('Eventos: formulário cria e edita múltiplas trilhas', async ({ page }) => {
  const sufixo = Date.now(); const inicio = new Date(Date.now() + 12 * 86400000); const fim = new Date(inicio.getTime() + 4 * 3600000)
  const valorData = (data: Date) => new Date(data.getTime() - data.getTimezoneOffset() * 60000).toISOString().slice(0, 16)
  await page.goto('/eventos/novo')
  await page.getByLabel('Nome do evento').fill(`Evento Multi Trilha ${sufixo}`); await page.getByLabel('Formato').selectOption('Remoto')
  await page.getByLabel('Início').fill(valorData(inicio)); await page.getByLabel('Fim').fill(valorData(fim)); await page.getByLabel('Link remoto').fill('https://evento.test')
  await page.getByLabel('Nome da trilha 1').fill('Arquitetura'); await page.getByRole('button', { name: 'Adicionar trilha' }).click(); await page.getByLabel('Nome da trilha 2').fill('Cloud')
  await expect(page.getByText('Trilhas (2)')).toBeVisible(); await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByText('Arquitetura', { exact: true })).toBeVisible(); await expect(page.getByText('Cloud', { exact: true })).toBeVisible()
  await page.getByRole('link', { name: 'Editar' }).click(); await expect(page.getByText('Trilhas (2)')).toBeVisible()
  await page.getByRole('button', { name: 'Adicionar trilha' }).click(); await page.getByLabel('Nome da trilha 3').fill('Dados'); await page.getByRole('button', { name: 'Remover trilha 2' }).click()
  await page.getByRole('button', { name: 'Salvar' }).click(); await expect(page.getByText('Arquitetura', { exact: true })).toBeVisible(); await expect(page.getByText('Dados', { exact: true })).toBeVisible(); await expect(page.getByText('Cloud', { exact: true })).toHaveCount(0)
})
