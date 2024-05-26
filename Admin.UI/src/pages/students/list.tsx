import { createColumnHelper } from '@tanstack/react-table';

import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import type { Student } from './types';
import type { IResourceComponentsProps } from '@refinedev/core';

const columnHelper = createColumnHelper<Student>();

const columns = [
  columnHelper.accessor('studentGuid', {
    header: 'Гуид студента',
    enableSorting: false,
  }),

  columnHelper.accessor('fullName', {
    header: 'ФИО',
    enableSorting: true,
  }),

  columnHelper.accessor('groupNumber', {
    header: 'Группа',
    enableSorting: true,
  }),

  columnHelper.accessor('course', {
    header: 'Курс',
    enableSorting: true,
  }),

  columnHelper.accessor('currentSemesterName', {
    header: 'Семестр',
    enableSorting: false,
  }),
];

export const StudentList: React.FC<IResourceComponentsProps> = () => {
  return (
    <DataGrid
      recordId="studentGuid"
      allColumns={columns}
      defaultColumnsKeys={['fullName', 'course', 'currentSemesterName']}
      quickFilters={[
        { field: 'fullName', label: 'ФИО' },
        { field: 'groupNumber', label: 'Группа' },
      ]}
      filters={[
        { column: 'fullName', name: 'ФИО', type: 'text' },
        { column: 'groupNumber', name: 'Группа', type: 'text' },
      ]}
      refineCoreProps={{
        sorters: {
          initial: [{ field: 'course', order: 'desc' }],
        },
      }}
    />
  );
};
