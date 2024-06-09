import { createColumnHelper } from '@tanstack/react-table';

import { DateCell } from '@/shared/components/date-cell';
import { TeacherCell } from '@/shared/components/teacher-cell';
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
    cell(v) {
      return <TeacherCell guid={v.getValue()} />;
    },
  }),

  columnHelper.accessor('points', {
    header: 'Баллы',
    enableSorting: false,
  }),

  columnHelper.accessor('date', {
    header: 'Дата',
    enableSorting: true,
    cell(v) {
      return <DateCell date={v.getValue()} />;
    },
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
      canExportCsv
      allColumns={columns}
      defaultColumnsKeys={[
        'studentGuid',
        'teacherGuid',
        'date',
        'points',
        'workType',
        'comment',
      ]}
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
          options: workTypes.map((t) => ({
            value: t,
            label: workTypeRus[t],
          })),
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
