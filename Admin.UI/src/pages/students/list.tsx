import { createColumnHelper } from '@tanstack/react-table';

import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import { healthGroupRus, healthGroups } from './types';

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

  columnHelper.accessor('visits', {
    header: 'Посещения',
    enableSorting: false,
  }),

  columnHelper.accessor('pointsForStandards', {
    header: 'Баллы за нормативы',
    enableSorting: false,
  }),

  columnHelper.accessor('additionalPoints', {
    header: 'Доп. баллы',
    enableSorting: false,
  }),

  columnHelper.accessor('hasDebtFromPreviousSemester', {
    header: 'Долг за семестр',
    enableSorting: true,
    cell(v) {
      return v.getValue() ? 'Есть' : 'Нет';
    },
  }),

  columnHelper.accessor('hadDebtInSemester', {
    header: 'Был ли долг',
    enableSorting: true,
    cell(v) {
      return v.getValue() ? 'Есть' : 'Нет';
    },
  }),

  columnHelper.accessor('archivedVisitValue', {
    header: 'Кол-во баллов за посещение (если есть долг)',
    enableSorting: false,
  }),

  columnHelper.accessor('isActive', {
    header: 'Активный',
    enableSorting: false,
    cell(v) {
      return v.getValue() ? 'Да' : 'Нет';
    },
  }),

  columnHelper.accessor('healthGroup', {
    header: 'Группа здоровья',
    enableSorting: false,
    cell(v) {
      return healthGroupRus[v.getValue()];
    },
  }),

  columnHelper.accessor('department', {
    header: 'Кафедра',
    enableSorting: false,
  }),

  columnHelper.accessor('currentSemesterName', {
    header: 'Семестр',
    enableSorting: false,
  }),
];

export const StudentListPage: React.FC<IResourceComponentsProps> = () => {
  return (
    <DataGrid
      recordId="studentGuid"
      allColumns={columns}
      canExportCsv
      defaultColumnsKeys={[
        'fullName',
        'groupNumber',
        'visits',
        'pointsForStandards',
        'additionalPoints',
        'hasDebtFromPreviousSemester',
        'course',
      ]}
      quickFilters={[
        { field: 'fullName', label: 'ФИО' },
        { field: 'groupNumber', label: 'Группа' },
      ]}
      filters={[
        { column: 'fullName', name: 'ФИО', type: 'text' },
        { column: 'groupNumber', name: 'Группа', type: 'text' },
        {
          column: 'hasDebtFromPreviousSemester',
          name: 'Долг',
          type: 'boolean',
        },
        { column: 'hadDebtInSemester', name: 'Был ли долг', type: 'boolean' },
        { column: 'isActive', name: 'Активен', type: 'boolean' },
        {
          column: 'healthGroup',
          name: 'Группа здоровья',
          type: 'select',
          options: healthGroups.map((g) => ({
            value: g,
            label: healthGroupRus[g],
          })),
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
