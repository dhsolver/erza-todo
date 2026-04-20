import { useCallback, useEffect, useMemo, useState } from 'react'
import * as api from './api/todos'
import type { Todo } from './types'
import { HeaderSummary } from './components/HeaderSummary'
import { TaskComposer } from './components/TaskComposer'
import { TaskList } from './components/TaskList'
import './App.css'

type Filter = 'all' | 'open' | 'done'

export default function App() {
  const [items, setItems] = useState<Todo[]>([])
  const [page, setPage] = useState(1)
  const [pageSize] = useState(25)
  const [total, setTotal] = useState(0)
  const [filter, setFilter] = useState<Filter>('all')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [dueLocal, setDueLocal] = useState('')

  const totalPages = useMemo(
    () => Math.max(1, Math.ceil(total / pageSize)),
    [total, pageSize],
  )
  const completedInPage = useMemo(() => items.filter((t) => t.isCompleted).length, [items])
  const openInPage = useMemo(() => items.length - completedInPage, [items, completedInPage])

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await api.fetchTodos(page, pageSize, filter)
      setItems(data.items)
      setTotal(data.totalCount)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load tasks')
    } finally {
      setLoading(false)
    }
  }, [page, pageSize, filter])

  useEffect(() => {
    void load()
  }, [load])

  async function onCreate(e: React.FormEvent) {
    e.preventDefault()
    if (!title.trim()) return
    setError(null)
    try {
      const dueAtUtc = dueLocal ? new Date(dueLocal).toISOString() : null
      await api.createTodo({
        title: title.trim(),
        description: description.trim() || null,
        dueAtUtc,
      })
      setTitle('')
      setDescription('')
      setDueLocal('')
      setPage(1)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not create task')
    }
  }

  async function toggleDone(t: Todo) {
    setError(null)
    try {
      const updated = await api.updateTodo(t.id, {
        title: t.title,
        description: t.description,
        isCompleted: !t.isCompleted,
        dueAtUtc: t.dueAtUtc,
        version: t.version,
      })
      setItems((prev) => prev.map((x) => (x.id === updated.id ? updated : x)))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Update failed')
      await load()
    }
  }

  async function remove(t: Todo) {
    if (!window.confirm('Delete this task?')) return
    setError(null)
    try {
      await api.deleteTodo(t.id)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delete failed')
    }
  }

  return (
    <div className="app">
      <HeaderSummary total={total} open={openInPage} done={completedInPage} />

      {error ? (
        <div className="banner banner-error" role="alert">
          {error}
        </div>
      ) : null}

      <div className="layout">
        <TaskComposer
          title={title}
          description={description}
          dueLocal={dueLocal}
          onTitleChange={setTitle}
          onDescriptionChange={setDescription}
          onDueLocalChange={setDueLocal}
          onSubmit={onCreate}
        />

        <TaskList
          items={items}
          loading={loading}
          filter={filter}
          page={page}
          total={total}
          totalPages={totalPages}
          onFilterChange={(f) => {
            setFilter(f)
            setPage(1)
          }}
          onRefresh={() => void load()}
          onToggleDone={(t) => void toggleDone(t)}
          onDelete={(t) => void remove(t)}
          onPreviousPage={() => setPage((p) => p - 1)}
          onNextPage={() => setPage((p) => p + 1)}
        />
      </div>
    </div>
  )
}
