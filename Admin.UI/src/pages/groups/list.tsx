import { createColumnHelper } from '@tanstack/react-table';

import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import type { Group } from './types';

const columnHelper = createColumnHelper<Group>();

const columns = [
  columnHelper.accessor('groupName', {
    header: 'Группа',
    enableSorting: true,
  }),

  columnHelper.accessor('visitValue', {
    header: 'Стоимость посещения',
    enableSorting: true,
  }),

  columnHelper.accessor('curatorGuid', {
    header: 'Куратор',
    enableSorting: false,
  }),
];

export const GroupListPage = () => {
  return (
    <DataGrid
      recordId="groupName"
      canExportCsv
      allColumns={columns}
      defaultColumnsKeys={['groupName', 'visitValue', 'curatorGuid']}
      quickFilters={[
        { field: 'groupName', label: 'Группа' },
        { field: 'visitValue', label: 'Стоимость посещения' },
      ]}
      filters={[
        { column: 'groupName', type: 'text', name: 'Группа' },
        { column: 'visitValue', type: 'number', name: 'Стоимость посещения' },
        {
          column: 'curatorGuid',
          type: 'text',
          name: 'Куратор',
          withNullChecking: true,
        },
      ]}
    />
  );
};
