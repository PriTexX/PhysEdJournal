import { createColumnHelper } from '@tanstack/react-table';

import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import type { Teacher } from './types';

const columnHelper = createColumnHelper<Teacher>();

const columns = [
  columnHelper.accessor('fullName', {
    header: 'ФИО',
    enableSorting: true,
  }),

  columnHelper.accessor('teacherGuid', {
    header: 'Гуид',
    enableSorting: false,
    meta: { canCopy: true },
  }),

  columnHelper.accessor('permissions', {
    header: 'Права',
    enableSorting: false,
  }),
];

export const TeacherListPage = () => {
  return (
    <DataGrid
      recordId="teacherGuid"
      canExportCsv
      allColumns={columns}
      defaultColumnsKeys={['fullName', 'teacherGuid', 'permissions']}
      quickFilters={[
        { field: 'fullName', label: 'ФИО' },
        { field: 'teacherGuid', label: 'Гуид' },
      ]}
      filters={[
        {
          column: 'fullName',
          type: 'text',
          name: 'ФИО',
        },
      ]}
      refineCoreProps={{
        sorters: {
          initial: [{ field: 'fullName', order: 'asc' }],
        },
      }}
    />
  );
};
