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
  await expect(page.getByRole('button', { name: 'Excluir' })).toBeVisible()
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
  await page.getByRole('button', { name: 'Excluir' }).click()
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
  await expect(page.getByText('Ambiente único')).toBeVisible()
  await expect(page.getByText('80 pessoas')).toBeVisible()
  await page.getByRole('button', { name: 'Excluir' }).click()
  await page.getByRole('button', { name: 'Excluir' }).last().click()
  await expect(page.getByRole('heading', { name: 'Locais' })).toBeVisible()
})

test('CRUD de evento remoto preserva formato e situação inicial', async ({ page }) => {
  const nome = `Evento Playwright ${Date.now()}`
  const inicio = new Date(Date.now() + 2 * 86400000)
  const fim = new Date(inicio.getTime() + 2 * 3600000)
  const valorData = (data: Date) => new Date(data.getTime() - data.getTimezoneOffset() * 60000).toISOString().slice(0, 16)
  await page.goto('/eventos/novo')
  await page.getByLabel('Nome do evento *').fill(nome)
  await page.getByLabel('Formato *').selectOption('Remoto')
  await page.getByLabel('Início *').fill(valorData(inicio))
  await page.getByLabel('Fim *').fill(valorData(fim))
  await page.getByLabel('Link remoto').fill('https://evento.teste.local/sala')
  await page.getByRole('button', { name: 'Salvar' }).click()
  await expect(page.getByRole('heading', { name: nome })).toBeVisible()
  await expect(page.getByText('Rascunho')).toBeVisible()
  await expect(page.getByText('Remoto', { exact: true })).toBeVisible()
  await page.getByRole('button', { name: 'Excluir' }).click()
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
  await expect(page.getByText(nome)).toBeVisible()
})
