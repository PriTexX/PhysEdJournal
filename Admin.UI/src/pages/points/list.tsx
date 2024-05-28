import { createColumnHelper } from '@tanstack/react-table';

import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import { workTypeRus, workTypes } from './types';

import type { PointsHistory } from './types';

const columnHelper = createColumnHelper<PointsHistory>();

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

  columnHelper.accessor('workType', {
    header: 'Вид работ',
    enableSorting: false,
    cell(v) {
      return workTypeRus[v.getValue()];
    },
  }),

  columnHelper.accessor('comment', {
    header: 'Комментарий',
    enableSorting: false,
  }),
];

export const PointsListPage = () => {
  return (
    <DataGrid
      recordId="id"
      allColumns={columns}
      defaultColumnsKeys={[
        'studentGuid',
        'teacherGuid',
        'date',
        'points',
        'workType',
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
          column: 'workType',
          type: 'select',
          name: 'Вид работ',
          options: workTypes,
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
