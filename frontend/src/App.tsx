import { useCallback, useEffect, useMemo, useState } from 'react'
import * as api from './api/todos'
import type { Todo } from './types'
import './App.css'

type Filter = 'all' | 'open' | 'done'

function formatDue(value: string | null): string {
  if (!value) return ''
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return ''
  return d.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}

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
      <header className="header">
        <h1>Tasks</h1>
        <p className="lede">Simple list with due dates, filters, and safe concurrent edits.</p>
      </header>

      {error ? (
        <div className="banner banner-error" role="alert">
          {error}
        </div>
      ) : null}

      <form className="card compose" onSubmit={onCreate}>
        <h2>New task</h2>
        <label className="field">
          <span>Title</span>
          <input
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            maxLength={500}
            placeholder="What needs doing?"
            required
          />
        </label>
        <label className="field">
          <span>Notes (optional)</span>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            maxLength={4000}
            rows={3}
            placeholder="Extra context"
          />
        </label>
        <label className="field">
          <span>Due (optional)</span>
          <input type="datetime-local" value={dueLocal} onChange={(e) => setDueLocal(e.target.value)} />
        </label>
        <button type="submit" className="btn primary">
          Add task
        </button>
      </form>

      <section className="card list">
        <div className="toolbar">
          <div className="filters" role="tablist" aria-label="Filter tasks">
            {(['all', 'open', 'done'] as const).map((f) => (
              <button
                key={f}
                type="button"
                role="tab"
                aria-selected={filter === f}
                className={filter === f ? 'tab active' : 'tab'}
                onClick={() => {
                  setFilter(f)
                  setPage(1)
                }}
              >
                {f === 'all' ? 'All' : f === 'open' ? 'Open' : 'Done'}
              </button>
            ))}
          </div>
          <button type="button" className="btn ghost" onClick={() => void load()} disabled={loading}>
            Refresh
          </button>
        </div>

        {loading ? <p className="muted">Loading…</p> : null}

        {!loading && items.length === 0 ? (
          <p className="muted">No tasks yet. Add one above.</p>
        ) : null}

        <ul className="todos">
          {items.map((t) => (
            <li key={t.id} className={t.isCompleted ? 'todo done' : 'todo'}>
              <label className="check">
                <input type="checkbox" checked={t.isCompleted} onChange={() => void toggleDone(t)} />
                <span className="title">{t.title}</span>
              </label>
              {t.description ? <p className="desc">{t.description}</p> : null}
              {t.dueAtUtc ? <p className="due">Due {formatDue(t.dueAtUtc)}</p> : null}
              <div className="row-actions">
                <button type="button" className="btn danger" onClick={() => void remove(t)}>
                  Delete
                </button>
              </div>
            </li>
          ))}
        </ul>

        {totalPages > 1 ? (
          <footer className="pager">
            <button type="button" className="btn ghost" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
              Previous
            </button>
            <span className="muted">
              Page {page} of {totalPages} ({total} tasks)
            </span>
            <button
              type="button"
              className="btn ghost"
              disabled={page >= totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </button>
          </footer>
        ) : null}
      </section>
    </div>
  )
}
