import { createColumnHelper } from '@tanstack/react-table';

import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import type { VisitHistory } from './types';

const columnHelper = createColumnHelper<VisitHistory>();

const columns = [
  columnHelper.accessor('studentGuid', {
    header: 'Гуид студента',
    enableSorting: false,
  }),

  columnHelper.accessor('teacherGuid', {
    header: 'Гуид преподавателя',
    enableSorting: false,
  }),

  columnHelper.accessor('date', {
    header: 'Дата',
    enableSorting: true,
  }),
];

export const VisitsListPage = () => {
  return (
    <DataGrid
      recordId="id"
      canExportCsv
      allColumns={columns}
      defaultColumnsKeys={['studentGuid', 'teacherGuid', 'date']}
      quickFilters={[
        { field: 'studentGuid', label: 'Гуид студента' },
        { field: 'teacherGuid', label: 'Гуид преподавателя' },
      ]}
      filters={[
        {
          column: 'date',
          type: 'date',
          name: 'Дата',
        },
      ]}
      refineCoreProps={{
        sorters: {
          initial: [{ field: 'date', order: 'desc' }],
        },
      }}
    />
  );
};
