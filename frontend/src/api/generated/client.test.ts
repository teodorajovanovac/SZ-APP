import { ApiProblemError, apiRequest } from './client'

describe('apiRequest', () => {
  it('šalje cookie kredencijale i čita JSON odgovor', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ id: 42 }), {
        status: 200,
        headers: { 'content-type': 'application/json' },
      }),
    )

    await expect(apiRequest<{ id: number }>('/api/v1/test')).resolves.toEqual({ id: 42 })
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/test', expect.objectContaining({ credentials: 'include' }))
  })

  it('pretvara RFC Problem Details u tipizovanu grešku', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ title: 'Neispravan zahtev', detail: 'Polje nije validno.' }), {
        status: 422,
        headers: { 'content-type': 'application/problem+json' },
      }),
    )

    const promise = apiRequest('/api/v1/test')
    await expect(promise).rejects.toBeInstanceOf(ApiProblemError)
    await expect(promise).rejects.toMatchObject({
      problem: { status: 422, title: 'Neispravan zahtev' },
    })
  })
})
