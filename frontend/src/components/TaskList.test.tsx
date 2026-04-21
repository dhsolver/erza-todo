import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import type { Todo } from '../types'
import { TaskList } from './TaskList'

const sampleTodo: Todo = {
  id: '1',
  title: 'Prepare demo',
  description: 'Sync with backend APIs',
  isCompleted: false,
  dueAtUtc: null,
  createdAtUtc: '2026-04-21T10:00:00Z',
  updatedAtUtc: '2026-04-21T10:00:00Z',
  version: 0,
}

describe('TaskList', () => {
  it('invokes handlers for filter, refresh, toggle, delete, and paging', () => {
    const onFilterChange = vi.fn()
    const onRefresh = vi.fn()
    const onToggleDone = vi.fn()
    const onDelete = vi.fn()
    const onPreviousPage = vi.fn()
    const onNextPage = vi.fn()

    render(
      <TaskList
        items={[sampleTodo]}
        loading={false}
        filter="all"
        page={2}
        total={30}
        totalPages={3}
        onFilterChange={onFilterChange}
        onRefresh={onRefresh}
        onToggleDone={onToggleDone}
        onDelete={onDelete}
        onPreviousPage={onPreviousPage}
        onNextPage={onNextPage}
      />,
    )

    fireEvent.click(screen.getByRole('tab', { name: 'Open' }))
    fireEvent.click(screen.getByRole('button', { name: 'Refresh' }))
    fireEvent.click(screen.getByRole('checkbox'))
    fireEvent.click(screen.getByRole('button', { name: 'Delete' }))
    fireEvent.click(screen.getByRole('button', { name: 'Previous' }))
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))

    expect(onFilterChange).toHaveBeenCalledWith('open')
    expect(onRefresh).toHaveBeenCalledOnce()
    expect(onToggleDone).toHaveBeenCalledWith(sampleTodo)
    expect(onDelete).toHaveBeenCalledWith(sampleTodo)
    expect(onPreviousPage).toHaveBeenCalledOnce()
    expect(onNextPage).toHaveBeenCalledOnce()
  })

  it('shows empty state when there are no tasks', () => {
    render(
      <TaskList
        items={[]}
        loading={false}
        filter="done"
        page={1}
        total={0}
        totalPages={1}
        onFilterChange={vi.fn()}
        onRefresh={vi.fn()}
        onToggleDone={vi.fn()}
        onDelete={vi.fn()}
        onPreviousPage={vi.fn()}
        onNextPage={vi.fn()}
      />,
    )

    expect(screen.getByText('No tasks found for this filter.')).toBeInTheDocument()
  })
})
