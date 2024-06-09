import { createColumnHelper } from '@tanstack/react-table';

import { useTeachersMap } from '@/features/use-teachers-map';
import { DateCell } from '@/shared/components/date-cell';
import { TeacherCell } from '@/shared/components/teacher-cell';
import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import type { VisitHistory } from './types';

const columnHelper = createColumnHelper<VisitHistory>();

const columns = [
  columnHelper.accessor('studentGuid', {
    header: 'Гуид студента',
    enableSorting: false,
  }),

  columnHelper.accessor('teacherGuid', {
    header: 'Преподаватель',
    enableSorting: false,
    cell(v) {
      return <TeacherCell guid={v.getValue()} />;
    },
  }),

  columnHelper.accessor('date', {
    header: 'Дата',
    enableSorting: true,
    cell(v) {
      return <DateCell date={v.getValue()} />;
    },
  }),
];

export const VisitsListPage = () => {
  return (
    <DataGrid
      recordId="id"
      canExportCsv
      allColumns={columns}
      defaultColumnsKeys={['teacherGuid', 'date', 'studentGuid']}
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
