import type { CreateTodoInput, PagedTodos, Todo, UpdateTodoInput } from '../types'

async function parseError(res: Response): Promise<string> {
  try {
    const body = await res.json()
    if (typeof body?.detail === 'string') return body.detail
    if (typeof body?.title === 'string') return body.title
    if (typeof body?.message === 'string') return body.message
  } catch {
    /* ignore */
  }
  return res.statusText || 'Request failed'
}

export async function fetchTodos(
  page: number,
  pageSize: number,
  filter: 'all' | 'open' | 'done',
): Promise<PagedTodos> {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  })
  if (filter === 'open') params.set('isCompleted', 'false')
  if (filter === 'done') params.set('isCompleted', 'true')

  const res = await fetch(`/api/todos?${params.toString()}`)
  if (!res.ok) throw new Error(await parseError(res))
  return res.json()
}

export async function createTodo(input: CreateTodoInput): Promise<Todo> {
  const res = await fetch('/api/todos', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  })
  if (!res.ok) throw new Error(await parseError(res))
  return res.json()
}

export async function updateTodo(id: string, input: UpdateTodoInput): Promise<Todo> {
  const res = await fetch(`/api/todos/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  })
  if (res.status === 409) {
    throw new Error('Someone else changed this task. Refresh the list and try again.')
  }
  if (!res.ok) throw new Error(await parseError(res))
  return res.json()
}

export async function deleteTodo(id: string): Promise<void> {
  const res = await fetch(`/api/todos/${id}`, { method: 'DELETE' })
  if (!res.ok && res.status !== 404) throw new Error(await parseError(res))
}
