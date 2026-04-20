type HeaderSummaryProps = {
  total: number
  open: number
  done: number
}

export function HeaderSummary({ total, open, done }: HeaderSummaryProps) {
  return (
    <header className="header card">
      <div>
        <p className="eyebrow">Ezra Task Manager</p>
        <h1>Plan your work.</h1>
      </div>
      <div className="kpi-grid" aria-label="Task summary">
        <article className="kpi">
          <span className="kpi-label">Total</span>
          <strong>{total}</strong>
        </article>
        <article className="kpi">
          <span className="kpi-label">Open</span>
          <strong>{open}</strong>
        </article>
        <article className="kpi">
          <span className="kpi-label">Done</span>
          <strong>{done}</strong>
        </article>
      </div>
    </header>
  )
}
