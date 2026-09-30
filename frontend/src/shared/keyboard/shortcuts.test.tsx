import { fireEvent, render, screen } from '@testing-library/react'
import { useGlobalShortcuts, useShortcut } from './shortcuts'

function Harness({ navigate, openHelp, onNew }: { navigate: (path: string) => void; openHelp: () => void; onNew: () => void }) {
  useGlobalShortcuts({ navigate, openHelp })
  useShortcut('alt+n', onNew)
  return (
    <main id="main-content">
      <input aria-label="polje" />
      <input aria-label="pretraga" type="search" />
    </main>
  )
}

function setup() {
  const navigate = vi.fn()
  const openHelp = vi.fn()
  const onNew = vi.fn()
  render(<Harness navigate={navigate} openHelp={openHelp} onNew={onNew} />)
  return { navigate, openHelp, onNew }
}

describe('keyboard shortcuts', () => {
  it('g then a letter navigates', () => {
    const { navigate } = setup()
    fireEvent.keyDown(document.body, { key: 'g' })
    fireEvent.keyDown(document.body, { key: 'p' })
    expect(navigate).toHaveBeenCalledWith('/partners')
  })

  it('a letter without the g prefix does nothing', () => {
    const { navigate } = setup()
    fireEvent.keyDown(document.body, { key: 'p' })
    expect(navigate).not.toHaveBeenCalled()
  })

  it('ignores plain keys while typing in an input', () => {
    const { navigate, openHelp } = setup()
    const input = screen.getByLabelText('polje')
    fireEvent.keyDown(input, { key: 'g' })
    fireEvent.keyDown(input, { key: 'p' })
    fireEvent.keyDown(input, { key: '?' })
    expect(navigate).not.toHaveBeenCalled()
    expect(openHelp).not.toHaveBeenCalled()
  })

  it('still fires Alt combos inside an input', () => {
    const { onNew } = setup()
    fireEvent.keyDown(screen.getByLabelText('polje'), { key: 'n', code: 'KeyN', altKey: true })
    expect(onNew).toHaveBeenCalledTimes(1)
  })

  it('? opens help and / focuses the page search', () => {
    const { openHelp } = setup()
    fireEvent.keyDown(document.body, { key: '?', shiftKey: true })
    expect(openHelp).toHaveBeenCalled()
    fireEvent.keyDown(document.body, { key: '/' })
    expect(screen.getByLabelText('pretraga')).toHaveFocus()
  })
})
