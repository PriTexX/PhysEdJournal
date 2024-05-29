import { createColumnHelper } from '@tanstack/react-table';

import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import type { Semester } from './types';

const columnHelper = createColumnHelper<Semester>();

const columns = [
  columnHelper.accessor('name', {
    header: 'Название',
    enableSorting: false,
  }),

  columnHelper.accessor('isCurrent', {
    header: 'Текущий',
    enableSorting: true,
    cell(v) {
      return v.getValue() ? 'Да' : 'Нет';
    },
  }),
];

export const SemesterListPage = () => {
  return (
    <DataGrid
      recordId="name"
      allColumns={columns}
      defaultColumnsKeys={['name', 'isCurrent']}
      quickFilters={[{ field: 'name', label: 'Название' }]}
      filters={[
        { column: 'name', type: 'text', name: 'Название' },
        { column: 'isCurrent', type: 'boolean', name: 'Текущий' },
      ]}
      refineCoreProps={{
        sorters: {
          initial: [{ field: 'isCurrent', order: 'desc' }],
        },
      }}
    />
  );
};
