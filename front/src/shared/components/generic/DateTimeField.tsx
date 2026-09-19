import * as React from 'react'

import { Input } from '@/shared/components/ui/input'
import { cn } from '@/shared/lib/utils'

export interface DateTimeFieldProps extends Omit<React.ComponentProps<'input'>, 'type' | 'value' | 'onChange'> {
  /** Valor no formato de `datetime-local` ("yyyy-MM-ddTHH:mm"). Converta com `shared/lib/datetime`. */
  value: string | null | undefined
  onChange: (valor: string) => void
  /** `date` para somente data. */
  modo?: 'datetime' | 'date'
}

/** Campo de data/hora nativo (`datetime-local`) com valor controlado como string. */
export const DateTimeField = React.forwardRef<HTMLInputElement, DateTimeFieldProps>(function DateTimeField(
  { value, onChange, modo = 'datetime', className, ...props },
  ref,
) {
  return (
    <Input
      ref={ref}
      type={modo === 'date' ? 'date' : 'datetime-local'}
      value={value ?? ''}
      onChange={(e) => onChange(e.target.value)}
      className={cn('font-numeric', className)}
      {...props}
    />
  )
})
