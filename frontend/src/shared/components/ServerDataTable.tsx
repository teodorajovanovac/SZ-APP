import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
} from '@mui/material'
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
  type PaginationState,
  type SortingState,
} from '@tanstack/react-table'

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
}: ServerDataTableProps<TData>) {
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

  return (
    <Paper>
      <TableContainer>
        <Table aria-label={ariaLabel}>
          <TableHead>
            {table.getHeaderGroups().map((group) => (
              <TableRow key={group.id}>
                {group.headers.map((header) => (
                  <TableCell
                    key={header.id}
                    sortDirection={header.column.getIsSorted() || false}
                    onClick={header.column.getToggleSortingHandler()}
                    sx={{ cursor: header.column.getCanSort() ? 'pointer' : 'default' }}
                  >
                    {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
                  </TableCell>
                ))}
              </TableRow>
            ))}
          </TableHead>
          <TableBody>
            {table.getRowModel().rows.map((row) => (
              <TableRow key={row.id} hover>
                {row.getVisibleCells().map((cell) => (
                  <TableCell key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</TableCell>
                ))}
              </TableRow>
            ))}
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
        labelRowsPerPage="Redova po strani:"
      />
    </Paper>
  )
}
