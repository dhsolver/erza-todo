import type { FormEvent } from 'react'

type TaskComposerProps = {
  title: string
  description: string
  dueLocal: string
  onTitleChange: (value: string) => void
  onDescriptionChange: (value: string) => void
  onDueLocalChange: (value: string) => void
  onSubmit: (e: FormEvent) => void
}

export function TaskComposer({
  title,
  description,
  dueLocal,
  onTitleChange,
  onDescriptionChange,
  onDueLocalChange,
  onSubmit,
}: TaskComposerProps) {
  return (
    <form className="card compose" onSubmit={onSubmit}>
      <h2>Create task</h2>
      <p className="muted panel-lede">Capture clear outcomes, owners, and deadlines.</p>
      <label className="field">
        <span>Title</span>
        <input
          value={title}
          onChange={(e) => onTitleChange(e.target.value)}
          maxLength={500}
          placeholder="What needs doing?"
          required
        />
      </label>
      <label className="field">
        <span>Notes (optional)</span>
        <textarea
          value={description}
          onChange={(e) => onDescriptionChange(e.target.value)}
          maxLength={4000}
          rows={4}
          placeholder="Extra context, acceptance criteria, blockers..."
        />
      </label>
      <label className="field">
        <span>Due (optional)</span>
        <input type="datetime-local" value={dueLocal} onChange={(e) => onDueLocalChange(e.target.value)} />
      </label>
      <button type="submit" className="btn primary">
        Add task
      </button>
    </form>
  )
}
