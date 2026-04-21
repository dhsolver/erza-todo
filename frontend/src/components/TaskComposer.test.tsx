import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { TaskComposer } from './TaskComposer'

describe('TaskComposer', () => {
  it('renders controlled fields and sends updates', () => {
    const onTitleChange = vi.fn()
    const onDescriptionChange = vi.fn()
    const onDueLocalChange = vi.fn()
    const onSubmit = vi.fn((e) => e.preventDefault())

    render(
      <TaskComposer
        title="Draft release notes"
        description="Cover key feature updates"
        dueLocal="2026-04-21T10:30"
        onTitleChange={onTitleChange}
        onDescriptionChange={onDescriptionChange}
        onDueLocalChange={onDueLocalChange}
        onSubmit={onSubmit}
      />,
    )

    fireEvent.change(screen.getByPlaceholderText('What needs doing?'), { target: { value: 'Ship v1' } })
    fireEvent.change(screen.getByPlaceholderText('Extra context, acceptance criteria, blockers...'), {
      target: { value: 'Run smoke tests' },
    })
    fireEvent.change(screen.getByDisplayValue('2026-04-21T10:30'), { target: { value: '2026-04-22T12:00' } })
    fireEvent.submit(screen.getByRole('button', { name: 'Add task' }))

    expect(onTitleChange).toHaveBeenCalledWith('Ship v1')
    expect(onDescriptionChange).toHaveBeenCalledWith('Run smoke tests')
    expect(onDueLocalChange).toHaveBeenCalledWith('2026-04-22T12:00')
    expect(onSubmit).toHaveBeenCalledOnce()
  })
})
