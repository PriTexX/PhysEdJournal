import { createColumnHelper } from '@tanstack/react-table';

import { TeacherCell } from '@/shared/components/teacher-cell';
import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import { SyncGroupsButton } from './sync-csv-groups';

import type { Group } from './types';

const columnHelper = createColumnHelper<Group>();

const columns = [
  columnHelper.accessor('groupName', {
    header: 'Группа',
    enableSorting: true,
    meta: { canCopy: true },
  }),

  columnHelper.accessor('visitValue', {
    header: 'Стоимость посещения',
    enableSorting: true,
  }),

  columnHelper.accessor('curatorGuid', {
    header: 'Куратор',
    enableSorting: false,
    cell(v) {
      const guid = v.getValue();
      return guid && <TeacherCell guid={guid} />;
    },
  }),
];

function groupCsvMapper(item: Group, teachersMap: Map<string, string>) {
  const curatorGuid =
    item.curatorGuid && (teachersMap.get(item.curatorGuid) ?? item.curatorGuid);
  return { ...item, curatorGuid };
}

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
      refineCoreProps={{
        sorters: {
          initial: [{ field: 'groupName', order: 'asc' }],
        },
      }}
      customCsvMapper={groupCsvMapper}
      additionalButtons={[<SyncGroupsButton />]}
    />
  );
};
