import type { ReactNode } from 'react'

interface FieldProps {
  label: string
  errors?: string[]
  hint?: string
  children: ReactNode
}

/** Label + control + validation messages. Wrapping in <label> links them for screen readers. */
export function Field({ label, errors, hint, children }: FieldProps) {
  const hasErrors = errors !== undefined && errors.length > 0
  return (
    <label className={`field${hasErrors ? ' field--invalid' : ''}`}>
      <span className="field-label">{label}</span>
      {children}
      {hint && !hasErrors && <span className="field-hint">{hint}</span>}
      {errors?.map((message) => (
        <span key={message} className="field-error">
          {message}
        </span>
      ))}
    </label>
  )
}
