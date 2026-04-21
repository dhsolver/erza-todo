import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'
import * as todosApi from './api/todos'

vi.mock('./api/todos', () => ({
  fetchTodos: vi.fn(),
  createTodo: vi.fn(),
  updateTodo: vi.fn(),
  deleteTodo: vi.fn(),
}))

const mockedApi = vi.mocked(todosApi)

describe('App integration', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('loads tasks and creates a new one from the compose form', async () => {
    mockedApi.fetchTodos.mockResolvedValue({
      items: [
        {
          id: '1',
          title: 'Existing task',
          description: null,
          isCompleted: false,
          dueAtUtc: null,
          createdAtUtc: '2026-04-21T10:00:00Z',
          updatedAtUtc: '2026-04-21T10:00:00Z',
          version: 0,
        },
      ],
      page: 1,
      pageSize: 25,
      totalCount: 1,
    })
    mockedApi.createTodo.mockResolvedValue({
      id: '2',
      title: 'New task',
      description: null,
      isCompleted: false,
      dueAtUtc: null,
      createdAtUtc: '2026-04-21T11:00:00Z',
      updatedAtUtc: '2026-04-21T11:00:00Z',
      version: 0,
    })

    render(<App />)

    await screen.findByText('Existing task')
    expect(mockedApi.fetchTodos).toHaveBeenCalledWith(1, 25, 'all')

    fireEvent.change(screen.getByPlaceholderText('What needs doing?'), { target: { value: 'New task' } })
    fireEvent.submit(screen.getByRole('button', { name: 'Add task' }))

    await waitFor(() =>
      expect(mockedApi.createTodo).toHaveBeenCalledWith({
        title: 'New task',
        description: null,
        dueAtUtc: null,
      }),
    )
    await waitFor(() => expect(mockedApi.fetchTodos).toHaveBeenCalledTimes(2))
  })
})
