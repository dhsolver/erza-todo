import { useState } from 'react'

type AuthPanelProps = {
  onLogin: (email: string, password: string) => Promise<void>
  onRegister: (email: string, password: string) => Promise<void>
  loading: boolean
}

export function AuthPanel({ onLogin, onRegister, loading }: AuthPanelProps) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const [emailTouched, setEmailTouched] = useState(false)
  const [passwordTouched, setPasswordTouched] = useState(false)

  const trimmedEmail = email.trim()
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  const emailError = emailTouched && !emailPattern.test(trimmedEmail) ? 'Please enter a valid email address.' : null
  const passwordError = passwordTouched && password.length < 8 ? 'Password must be at least 8 characters.' : null
  const isValid = emailPattern.test(trimmedEmail) && password.length >= 8

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    setEmailTouched(true)
    setPasswordTouched(true)
    if (!isValid) return
    if (mode === 'login') await onLogin(email.trim(), password)
    else await onRegister(email.trim(), password)
  }

  return (
    <section className="card auth-panel">
      <h2>{mode === 'login' ? 'Sign in' : 'Create account'}</h2>
      <p className="muted panel-lede">Tasks are private to your account.</p>
      <form onSubmit={submit}>
        <label className="field">
          <span>Email</span>
          <input
            type="email"
            value={email}
            onChange={(e) => {
              setEmail(e.target.value)
            }}
            onBlur={() => setEmailTouched(true)}
            className={emailError ? 'invalid' : undefined}
            required
          />
          {emailError ? <small className="field-error">{emailError}</small> : null}
        </label>
        <label className="field">
          <span>Password</span>
          <input
            type="password"
            value={password}
            onChange={(e) => {
              setPassword(e.target.value)
            }}
            onBlur={() => setPasswordTouched(true)}
            minLength={8}
            className={passwordError ? 'invalid' : undefined}
            required
          />
          {passwordError ? <small className="field-error">{passwordError}</small> : null}
        </label>
        <div className="auth-actions">
          <button type="submit" className="btn primary" disabled={loading || !isValid}>
            {mode === 'login' ? 'Sign in' : 'Register'}
          </button>
          <button type="button" className="btn ghost" onClick={() => setMode(mode === 'login' ? 'register' : 'login')} disabled={loading}>
            {mode === 'login' ? 'Need an account?' : 'Have an account?'}
          </button>
        </div>
      </form>
    </section>
  )
}
