import type { Todo } from '../types'

type Filter = 'all' | 'open' | 'done'

type TaskListProps = {
  items: Todo[]
  loading: boolean
  filter: Filter
  page: number
  total: number
  totalPages: number
  onFilterChange: (filter: Filter) => void
  onRefresh: () => void
  onToggleDone: (todo: Todo) => void
  onDelete: (todo: Todo) => void
  onPreviousPage: () => void
  onNextPage: () => void
}

function formatDue(value: string | null): string {
  if (!value) return ''
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return ''
  return d.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}

export function TaskList({
  items,
  loading,
  filter,
  page,
  total,
  totalPages,
  onFilterChange,
  onRefresh,
  onToggleDone,
  onDelete,
  onPreviousPage,
  onNextPage,
}: TaskListProps) {
  return (
    <section className="card list">
      <div className="toolbar">
        <div>
          <h2>Tasks</h2>
          <p className="muted panel-lede">Showing {items.length} item(s) on this page</p>
        </div>
        <div className="toolbar-actions">
          <div className="filters" role="tablist" aria-label="Filter tasks">
            {(['all', 'open', 'done'] as const).map((f) => (
              <button
                key={f}
                type="button"
                role="tab"
                aria-selected={filter === f}
                className={filter === f ? 'tab active' : 'tab'}
                onClick={() => onFilterChange(f)}
              >
                {f === 'all' ? 'All' : f === 'open' ? 'Open' : 'Done'}
              </button>
            ))}
          </div>
          <button type="button" className="btn ghost" onClick={onRefresh} disabled={loading}>
            Refresh
          </button>
        </div>
      </div>

      {loading ? <p className="muted">Loading tasks...</p> : null}

      {!loading && items.length === 0 ? (
        <div className="empty">
          <p>No tasks found for this filter.</p>
          <p className="muted">Create a task or switch filters to see more.</p>
        </div>
      ) : null}

      <ul className="todos">
        {items.map((t) => (
          <li key={t.id} className={t.isCompleted ? 'todo done' : 'todo'}>
            <div className="todo-top">
              <label className="check">
                <input type="checkbox" checked={t.isCompleted} onChange={() => onToggleDone(t)} />
                <span className="title">{t.title}</span>
              </label>
              <span className={t.isCompleted ? 'pill done' : 'pill open'}>{t.isCompleted ? 'Completed' : 'Open'}</span>
            </div>
            {t.description ? <p className="desc">{t.description}</p> : null}
            {t.dueAtUtc ? <p className="due">Due {formatDue(t.dueAtUtc)}</p> : null}
            <div className="row-actions">
              <button type="button" className="btn danger" onClick={() => onDelete(t)}>
                Delete
              </button>
            </div>
          </li>
        ))}
      </ul>

      {totalPages > 1 ? (
        <footer className="pager">
          <button type="button" className="btn ghost" disabled={page <= 1} onClick={onPreviousPage}>
            Previous
          </button>
          <span className="muted">
            Page {page} of {totalPages} ({total} tasks)
          </span>
          <button type="button" className="btn ghost" disabled={page >= totalPages} onClick={onNextPage}>
            Next
          </button>
        </footer>
      ) : null}
    </section>
  )
}
