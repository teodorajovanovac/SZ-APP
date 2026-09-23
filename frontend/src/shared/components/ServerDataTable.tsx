import {
  Box,
  Paper,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TableSortLabel,
  Typography,
} from '@mui/material'
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
  type PaginationState,
  type RowData,
  type SortingState,
} from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'

declare module '@tanstack/react-table' {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  interface ColumnMeta<TData extends RowData, TValue> {
    /** Right-aligns the column and renders it with tabular (monospaced) figures. */
    numeric?: boolean
    align?: 'left' | 'right' | 'center'
    /** Truncate long free text to one line instead of wrapping. */
    ellipsis?: boolean
  }
}

interface ServerDataTableProps<TData> {
  ariaLabel: string
  rows: TData[]
  columns: ColumnDef<TData>[]
  rowCount: number
  pagination: PaginationState
  sorting: SortingState
  onPaginationChange: (value: PaginationState) => void
  onSortingChange: (value: SortingState) => void
  getRowId?: (row: TData) => string
  isLoading?: boolean
  emptyMessage?: string
  /** Width below which the table scrolls horizontally instead of squeezing columns. */
  minWidth?: number
}

export function ServerDataTable<TData>({
  ariaLabel,
  rows,
  columns,
  rowCount,
  pagination,
  sorting,
  onPaginationChange,
  onSortingChange,
  getRowId,
  isLoading = false,
  emptyMessage,
  minWidth = 720,
}: ServerDataTableProps<TData>) {
  const { t } = useTranslation()
  const table = useReactTable({
    data: rows,
    columns,
    rowCount,
    state: { pagination, sorting },
    manualPagination: true,
    manualSorting: true,
    onPaginationChange: (updater) =>
      onPaginationChange(typeof updater === 'function' ? updater(pagination) : updater),
    onSortingChange: (updater) =>
      onSortingChange(typeof updater === 'function' ? updater(sorting) : updater),
    getCoreRowModel: getCoreRowModel(),
    getRowId,
  })

  const columnCount = table.getAllLeafColumns().length
  const showEmpty = !isLoading && rows.length === 0

  return (
    <Paper variant="outlined">
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
          overflowX: 'auto',
          // subtle right-edge fade so it is visible that content continues off-screen
          backgroundImage: (theme) =>
            `linear-gradient(to left, ${theme.palette.background.paper}, rgba(0,0,0,0))`,
          backgroundSize: '24px 100%',
          backgroundPosition: 'right center',
          backgroundRepeat: 'no-repeat',
          backgroundAttachment: 'local',
        }}
      >
        <Table aria-label={ariaLabel} aria-busy={isLoading} sx={{ minWidth }}>
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
                      sx={{ fontWeight: 600, whiteSpace: 'nowrap' }}
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
              ? Array.from({ length: Math.min(pagination.pageSize, 5) }).map((_, rowIndex) => (
                  <TableRow key={`skeleton-${rowIndex}`}>
                    {Array.from({ length: columnCount }).map((__, cellIndex) => (
                      <TableCell key={cellIndex}>
                        <Skeleton variant="text" />
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              : table.getRowModel().rows.map((row) => (
                  <TableRow key={row.id} hover>
                    {row.getVisibleCells().map((cell) => {
                      const meta = cell.column.columnDef.meta
                      const numeric = Boolean(meta?.numeric)
                      return (
                        <TableCell
                          key={cell.id}
                          align={meta?.align ?? (numeric ? 'right' : 'left')}
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
                <TableCell colSpan={columnCount} align="center" sx={{ py: 6, border: 0 }}>
                  <Box sx={{ color: 'text.secondary' }}>
                    <Typography>{emptyMessage ?? t('table.empty')}</Typography>
                  </Box>
                </TableCell>
              </TableRow>
            ) : null}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        component="div"
        count={rowCount}
        page={pagination.pageIndex}
        rowsPerPage={pagination.pageSize}
        rowsPerPageOptions={[10, 25, 50, 100]}
        onPageChange={(_, page) => table.setPageIndex(page)}
        onRowsPerPageChange={(event) => table.setPageSize(Number(event.target.value))}
        labelRowsPerPage={t('table.rowsPerPage')}
        labelDisplayedRows={({ from, to, count }) =>
          t('table.displayedRows', { from, to, count: count === -1 ? `>${to}` : count })
        }
      />
    </Paper>
  )
}
