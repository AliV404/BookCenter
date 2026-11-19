import { ErrorBanner } from './ErrorBanner'
import { Modal } from './Modal'
import { useSaveMutation } from '../hooks/useSaveMutation'

interface ConfirmDialogProps {
  title: string
  message: string
  confirmLabel?: string
  /** 'danger' (default) for destructive actions, 'primary' for reversible ones. */
  tone?: 'danger' | 'primary'
  successMessage: string
  onConfirm: () => Promise<unknown>
  onClose: () => void
}

/** Confirmation for consequential actions. Server refusals (e.g. 409 "still has books") show inside the dialog. */
export function ConfirmDialog({
  title,
  message,
  confirmLabel = 'Delete',
  tone = 'danger',
  successMessage,
  onConfirm,
  onClose,
}: ConfirmDialogProps) {
  const mutation = useSaveMutation(onConfirm, successMessage, onClose)

  return (
    <Modal title={title} onClose={onClose}>
      <div className="modal-body">
        <ErrorBanner error={mutation.error} />
        <p>{message}</p>
      </div>
      <footer className="modal-footer">
        <button type="button" className="button" onClick={onClose}>
          Cancel
        </button>
        <button
          type="button"
          className={`button button--${tone}`}
          disabled={mutation.isPending}
          onClick={() => mutation.mutate()}
        >
          {mutation.isPending ? 'Working…' : confirmLabel}
        </button>
      </footer>
    </Modal>
  )
}
