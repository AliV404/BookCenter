import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useToast } from '../lib/toast'

/**
 * A mutation that, on success, refreshes every cached query and shows a toast.
 * Refreshing everything is deliberate: one change ripples widely here
 * (a loan changes book availability, reader loan counts, dashboard numbers...).
 */
export function useSaveMutation(mutationFn: () => Promise<unknown>, successMessage: string, onSuccess?: () => void) {
  const queryClient = useQueryClient()
  const notify = useToast()

  return useMutation({
    mutationFn,
    onSuccess: async () => {
      await queryClient.invalidateQueries()
      notify(successMessage)
      onSuccess?.()
    },
  })
}
