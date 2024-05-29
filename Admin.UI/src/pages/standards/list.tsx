import { createColumnHelper } from '@tanstack/react-table';

import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import { standardTypeRus, standardTypes } from './types';

import type { StandardHistory } from './types';

const columnHelper = createColumnHelper<StandardHistory>();

const columns = [
  columnHelper.accessor('studentGuid', {
    header: 'Студент',
    enableSorting: false,
  }),

  columnHelper.accessor('teacherGuid', {
    header: 'Преподаватель',
    enableSorting: false,
  }),

  columnHelper.accessor('points', {
    header: 'Баллы',
    enableSorting: false,
  }),

  columnHelper.accessor('date', {
    header: 'Дата',
    enableSorting: true,
  }),

  columnHelper.accessor('standardType', {
    header: 'Тип норматива',
    enableSorting: false,
    cell(v) {
      return standardTypeRus[v.getValue()];
    },
  }),

  columnHelper.accessor('comment', {
    header: 'Комментарий',
    enableSorting: false,
  }),
];

export const StandardsListPage = () => {
  return (
    <DataGrid
      recordId="id"
      allColumns={columns}
      defaultColumnsKeys={[
        'studentGuid',
        'teacherGuid',
        'date',
        'points',
        'standardType',
        'comment',
      ]}
      filters={[
        {
          column: 'date',
          type: 'date',
          name: 'Дата',
        },
        {
          column: 'studentGuid',
          type: 'text',
          name: 'Гуид студента',
        },
        {
          column: 'teacherGuid',
          type: 'text',
          name: 'Гуид преподавателя',
        },
        {
          column: 'standardType',
          type: 'select',
          name: 'Вид работ',
          options: standardTypes,
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
