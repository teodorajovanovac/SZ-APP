import {
  Paper,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  Typography,
} from '@mui/material'
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
  type PaginationState,
  type SortingState,
} from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'

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
      <TableContainer>
        <Table aria-label={ariaLabel} aria-busy={isLoading}>
          <TableHead>
            {table.getHeaderGroups().map((group) => (
              <TableRow key={group.id}>
                {group.headers.map((header) => (
                  <TableCell
                    key={header.id}
                    sortDirection={header.column.getIsSorted() || false}
                    onClick={header.column.getToggleSortingHandler()}
                    sx={{ cursor: header.column.getCanSort() ? 'pointer' : 'default', fontWeight: 600 }}
                  >
                    {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
                  </TableCell>
                ))}
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
                    {row.getVisibleCells().map((cell) => (
                      <TableCell key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</TableCell>
                    ))}
                  </TableRow>
                ))}
            {showEmpty ? (
              <TableRow>
                <TableCell colSpan={columnCount} align="center" sx={{ py: 4 }}>
                  <Typography color="text.secondary">{emptyMessage ?? t('table.empty')}</Typography>
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
