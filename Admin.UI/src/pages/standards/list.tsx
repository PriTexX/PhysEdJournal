import { createColumnHelper } from '@tanstack/react-table';

import { DateCell } from '@/shared/components/date-cell';
import { TeacherCell } from '@/shared/components/teacher-cell';
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
      canExportCsv
      allColumns={columns}
      defaultColumnsKeys={[
        'studentGuid',
        'teacherGuid',
        'date',
        'points',
        'standardType',
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
          column: 'standardType',
          type: 'select',
          name: 'Вид работ',
          options: standardTypes.map((t) => ({
            value: t,
            label: standardTypeRus[t],
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
