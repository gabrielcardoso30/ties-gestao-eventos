import type { ReactNode } from 'react'

import { cn } from '@/shared/lib/utils'

export interface ScheduleColumn {
  id: string
  label: string
  color?: string | null
}

export interface ScheduleEntry {
  id: string
  columnId: string
  slotId: string
}

export interface ScheduleSlot {
  id: string
  label: ReactNode
}

interface Props<TEntry extends ScheduleEntry> {
  columns: ScheduleColumn[]
  slots: ScheduleSlot[]
  entries: TEntry[]
  renderEntry: (entry: TEntry) => ReactNode
  emptyLabel?: string
}

/** Grade genérica de programação: horários nas linhas e categorias nas colunas. */
export function ScheduleGrid<TEntry extends ScheduleEntry>({ columns, slots, entries, renderEntry, emptyLabel = 'Sem atividade' }: Props<TEntry>) {
  return (
    <div className="overflow-x-auto rounded-xl border bg-card" data-testid="schedule-grid">
      <table className="w-full min-w-[720px] border-collapse text-sm">
        <thead>
          <tr className="border-b bg-muted/40">
            <th scope="col" className="sticky left-0 z-10 w-40 bg-muted/90 p-4 text-left font-medium">Horário</th>
            {columns.map(column => (
              <th key={column.id} scope="col" className="min-w-64 border-l p-4 text-left font-semibold">
                <span className="flex items-center gap-2">
                  <span className="size-2.5 shrink-0 rounded-full" style={{ backgroundColor: column.color || '#64748B' }} />
                  {column.label}
                </span>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {slots.map(slot => (
            <tr key={slot.id} className="border-b last:border-b-0">
              <th scope="row" className="sticky left-0 z-10 bg-card p-4 text-left align-top font-medium text-muted-foreground">{slot.label}</th>
              {columns.map(column => {
                const cellEntries = entries.filter(entry => entry.slotId === slot.id && entry.columnId === column.id)
                return (
                  <td key={column.id} className="border-l p-3 align-top">
                    <div className={cn('space-y-2', cellEntries.length === 0 && 'text-muted-foreground')}>
                      {cellEntries.length > 0 ? cellEntries.map(entry => <div key={entry.id}>{renderEntry(entry)}</div>) : <span className="text-xs">{emptyLabel}</span>}
                    </div>
                  </td>
                )
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
