export type Todo = {
  id: string
  title: string
  description: string | null
  isCompleted: boolean
  dueAtUtc: string | null
  createdAtUtc: string
  updatedAtUtc: string
  version: number
}

export type PagedTodos = {
  items: Todo[]
  page: number
  pageSize: number
  totalCount: number
}

export type CreateTodoInput = {
  title: string
  description?: string | null
  dueAtUtc?: string | null
}

export type UpdateTodoInput = {
  title: string
  description?: string | null
  isCompleted: boolean
  dueAtUtc?: string | null
  version: number
}
