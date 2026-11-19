import type { FormEvent, ReactNode } from 'react'
import { ErrorBanner } from './ErrorBanner'
import { Modal } from './Modal'

interface FormModalProps {
  title: string
  submitLabel: string
  isSubmitting: boolean
  error: unknown
  onSubmit: () => void
  onClose: () => void
  children: ReactNode
}

/** A modal containing a <form>: Enter submits, the error banner and footer buttons are built in. */
export function FormModal({ title, submitLabel, isSubmitting, error, onSubmit, onClose, children }: FormModalProps) {
  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    onSubmit()
  }

  return (
    <Modal title={title} onClose={onClose}>
      <form onSubmit={handleSubmit} noValidate>
        <div className="modal-body">
          <ErrorBanner error={error} />
          {children}
        </div>
        <footer className="modal-footer">
          <button type="button" className="button" onClick={onClose}>
            Cancel
          </button>
          <button type="submit" className="button button--primary" disabled={isSubmitting}>
            {isSubmitting ? 'Saving…' : submitLabel}
          </button>
        </footer>
      </form>
    </Modal>
  )
}
