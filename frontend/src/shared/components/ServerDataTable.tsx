import {
  Box,
  Paper,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableFooter,
  TableHead,
  TablePagination,
  TableRow,
  TableSortLabel,
  Typography,
} from '@mui/material'
import { alpha, keyframes } from '@mui/material/styles'
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
  type PaginationState,
  type RowData,
  type SortingState,
} from '@tanstack/react-table'
import { useEffect, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { EmptyState } from './EmptyState'

declare module '@tanstack/react-table' {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  interface ColumnMeta<TData extends RowData, TValue> {
    /** Right-aligns the column and renders it with tabular (monospaced) figures. */
    numeric?: boolean
    align?: 'left' | 'right' | 'center'
    /** Truncate long free text to one line instead of wrapping (full text on hover). */
    ellipsis?: boolean
  }
}

const flash = keyframes`from { background-color: var(--flash-color); } to { background-color: transparent; }`

interface ServerDataTableProps<TData> {
  ariaLabel: string
  rows: TData[]
  columns: ColumnDef<TData>[]
  rowCount: number
  pagination: PaginationState
  /** Only columns with `enableSorting: true` get sort arrows (UX-20: no fake sorting). */
  sorting?: SortingState
  onPaginationChange: (value: PaginationState) => void
  onSortingChange?: (value: SortingState) => void
  getRowId?: (row: TData) => string
  isLoading?: boolean
  emptyMessage?: string
  emptyHint?: string
  emptyAction?: ReactNode
  /** Width below which the table scrolls horizontally instead of squeezing columns. */
  minWidth?: number
  /** UX-13: totals row, keyed by column id; the first column shows "Ukupno" unless given. */
  totals?: Record<string, ReactNode>
  /** DES-05: briefly highlights this row (e.g. the record just saved). */
  highlightRowId?: string
}

export function ServerDataTable<TData>({
  ariaLabel,
  rows,
  columns,
  rowCount,
  pagination,
  sorting = [],
  onPaginationChange,
  onSortingChange,
  getRowId,
  isLoading = false,
  emptyMessage,
  emptyHint,
  emptyAction,
  minWidth = 720,
  totals,
  highlightRowId,
}: ServerDataTableProps<TData>) {
  const { t } = useTranslation()
  const table = useReactTable({
    data: rows,
    columns,
    rowCount,
    state: { pagination, sorting },
    defaultColumn: { enableSorting: false },
    enableSorting: Boolean(onSortingChange),
    manualPagination: true,
    manualSorting: true,
    onPaginationChange: (updater) =>
      onPaginationChange(typeof updater === 'function' ? updater(pagination) : updater),
    onSortingChange: (updater) =>
      onSortingChange?.(typeof updater === 'function' ? updater(sorting) : updater),
    getCoreRowModel: getCoreRowModel(),
    getRowId,
  })

  // UX-25: after a company/filter switch the old page may no longer exist — go back to page 1.
  const outOfRange = !isLoading && rowCount > 0 && pagination.pageIndex * pagination.pageSize >= rowCount
  useEffect(() => {
    if (outOfRange) onPaginationChange({ ...pagination, pageIndex: 0 })
  }, [outOfRange, onPaginationChange, pagination])

  const leafColumns = table.getAllLeafColumns()
  const columnCount = leafColumns.length
  const showEmpty = !isLoading && rows.length === 0

  return (
    <Paper variant="outlined" sx={{ overflow: 'hidden' }}>
      {/* Scroll affordance: on phones the table keeps its natural width and scrolls
          sideways instead of wrapping every accounting column into unreadable stacks. */}
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{ display: { xs: 'block', md: 'none' }, px: 2, pt: 1 }}
      >
        {t('ui.scrollHint')}
      </Typography>
      <TableContainer
        sx={{
          overflow: 'auto',
          // UX-25: the header stays visible while scrolling long lists.
          maxHeight: 'max(360px, calc(100vh - 260px))',
          // subtle right-edge fade so it is visible that content continues off-screen
          backgroundImage: (theme) =>
            `linear-gradient(to left, ${theme.palette.background.paper}, rgba(0,0,0,0))`,
          backgroundSize: '24px 100%',
          backgroundPosition: 'right center',
          backgroundRepeat: 'no-repeat',
          backgroundAttachment: 'local',
        }}
      >
        <Table stickyHeader aria-label={ariaLabel} aria-busy={isLoading} sx={{ minWidth }}>
          <TableHead>
            {table.getHeaderGroups().map((group) => (
              <TableRow key={group.id}>
                {group.headers.map((header) => {
                  const meta = header.column.columnDef.meta
                  const align = meta?.align ?? (meta?.numeric ? 'right' : 'left')
                  const sorted = header.column.getIsSorted()
                  const content = header.isPlaceholder
                    ? null
                    : flexRender(header.column.columnDef.header, header.getContext())
                  return (
                    <TableCell
                      key={header.id}
                      align={align}
                      sortDirection={sorted || false}
                      sx={{ whiteSpace: 'nowrap' }}
                    >
                      {header.column.getCanSort() ? (
                        <TableSortLabel
                          active={Boolean(sorted)}
                          direction={sorted === 'desc' ? 'desc' : 'asc'}
                          onClick={header.column.getToggleSortingHandler()}
                        >
                          {content}
                        </TableSortLabel>
                      ) : (
                        content
                      )}
                    </TableCell>
                  )
                })}
              </TableRow>
            ))}
          </TableHead>
          <TableBody>
            {isLoading
              ? Array.from({ length: Math.min(pagination.pageSize, 10) }).map((_, rowIndex) => (
                  <TableRow key={`skeleton-${rowIndex}`}>
                    {Array.from({ length: columnCount }).map((__, cellIndex) => (
                      <TableCell key={cellIndex}>
                        <Skeleton variant="text" />
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              : table.getRowModel().rows.map((row) => (
                  <TableRow
                    key={row.id}
                    hover
                    sx={
                      row.id === highlightRowId
                        ? (theme) => ({
                            '--flash-color': alpha(theme.palette.primary.main, 0.16),
                            '& > td': { animation: `${flash} 2.4s ease-out` },
                          })
                        : undefined
                    }
                  >
                    {row.getVisibleCells().map((cell) => {
                      const meta = cell.column.columnDef.meta
                      const numeric = Boolean(meta?.numeric)
                      const value = meta?.ellipsis ? cell.getValue() : undefined
                      return (
                        <TableCell
                          key={cell.id}
                          align={meta?.align ?? (numeric ? 'right' : 'left')}
                          title={typeof value === 'string' || typeof value === 'number' ? String(value) : undefined}
                          sx={{
                            ...(numeric
                              ? { fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' }
                              : null),
                            ...(meta?.ellipsis
                              ? { maxWidth: 260, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }
                              : null),
                          }}
                        >
                          {flexRender(cell.column.columnDef.cell, cell.getContext())}
                        </TableCell>
                      )
                    })}
                  </TableRow>
                ))}
            {showEmpty ? (
              <TableRow>
                <TableCell colSpan={columnCount} align="center" sx={{ border: 0 }}>
                  <Box sx={{ position: 'sticky', left: 0 }}>
                    <EmptyState message={emptyMessage ?? t('table.empty')} hint={emptyHint} action={emptyAction} />
                  </Box>
                </TableCell>
              </TableRow>
            ) : null}
          </TableBody>
          {totals && !showEmpty ? (
            <TableFooter>
              <TableRow>
                {leafColumns.map((column, index) => {
                  const meta = column.columnDef.meta
                  const numeric = Boolean(meta?.numeric)
                  return (
                    <TableCell
                      key={column.id}
                      align={meta?.align ?? (numeric ? 'right' : 'left')}
                      sx={{
                        position: 'sticky',
                        bottom: 0,
                        bgcolor: 'background.paper',
                        borderTop: 2,
                        borderColor: 'divider',
                        color: 'text.primary',
                        fontWeight: 650,
                        fontSize: '0.8125rem',
                        whiteSpace: 'nowrap',
                        fontVariantNumeric: numeric ? 'tabular-nums' : undefined,
                      }}
                    >
                      {totals[column.id] ?? (index === 0 ? t('ui.total') : null)}
                    </TableCell>
                  )
                })}
              </TableRow>
            </TableFooter>
          ) : null}
        </Table>
      </TableContainer>
      <TablePagination
        component="div"
        count={rowCount}
        page={outOfRange ? 0 : pagination.pageIndex}
        rowsPerPage={pagination.pageSize}
        rowsPerPageOptions={[10, 25, 50, 100]}
        onPageChange={(_, page) => table.setPageIndex(page)}
        onRowsPerPageChange={(event) => onPaginationChange({ pageIndex: 0, pageSize: Number(event.target.value) })}
        labelRowsPerPage={t('table.rowsPerPage')}
        labelDisplayedRows={({ from, to, count }) =>
          t('table.displayedRows', { from, to, count: count === -1 ? `>${to}` : count })
        }
      />
    </Paper>
  )
}
