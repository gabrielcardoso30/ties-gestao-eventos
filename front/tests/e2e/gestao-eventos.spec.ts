import { expect, test, type Page } from '@playwright/test'

async function autenticar(page: Page) {
  await page.goto('/login')
  await page.getByLabel(/e-mail/i).fill('admin@gestaoeventos.local')
  await page.getByLabel(/senha/i).fill('Admin@123456')
  await page.getByRole('button', { name: /entrar/i }).click()
  await expect(page.getByRole('heading', { name: 'Visão geral' })).toBeVisible()
}

test.beforeEach(async ({ page }) => autenticar(page))

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
