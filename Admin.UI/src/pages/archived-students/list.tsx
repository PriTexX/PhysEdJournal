import { createColumnHelper } from '@tanstack/react-table';

import { JsonCell } from '@/shared/components/json-cell';
import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import { workTypeRus } from '../points/types';
import { standardTypeRus } from '../standards/types';
import {
  PointsHistoryRender,
  StandarsHistoryRender,
  VisitsHistoryRender,
} from './history-render';

import type { ArchivedStudent } from './types';

const columnHelper = createColumnHelper<ArchivedStudent>();

const columns = [
  columnHelper.accessor('studentGuid', {
    header: 'Гуид студента',
    enableSorting: false,
  }),

  columnHelper.accessor('fullName', {
    header: 'ФИО',
    enableSorting: false,
  }),

  columnHelper.accessor('groupNumber', {
    header: 'Группа',
    enableSorting: false,
  }),

  columnHelper.accessor('semesterName', {
    header: 'Семестер',
    enableSorting: false,
  }),

  columnHelper.accessor('totalPoints', {
    header: 'Баллы',
    enableSorting: false,
  }),

  columnHelper.accessor('visits', {
    header: 'Кол-во посещений',
    enableSorting: false,
  }),

  columnHelper.accessor('visitsHistory', {
    header: 'Посещения',
    enableSorting: false,
    cell(v) {
      return (
        <JsonCell
          header="История посещений"
          content={<VisitsHistoryRender data={v.getValue()} />}
        />
      );
    },
  }),

  columnHelper.accessor('standardsHistory', {
    header: 'Нормативы',
    enableSorting: false,
    cell(v) {
      return (
        <JsonCell
          header="История нормативов"
          content={<StandarsHistoryRender data={v.getValue()} />}
        />
      );
    },
  }),

  columnHelper.accessor('pointsHistory', {
    header: 'Доп. баллы',
    enableSorting: false,
    cell(v) {
      return (
        <JsonCell
          header="История баллов"
          content={<PointsHistoryRender data={v.getValue()} />}
        />
      );
    },
  }),
];

function archivedStudentsCsvMapper(
  item: ArchivedStudent,
  teachersMap: Map<string, string>,
) {
  const visitsHistory = item.visitsHistory.map((h) => ({
    date: h.date,
    points: h.points,
    teacher: teachersMap.get(h.teacherGuid) ?? h.teacherGuid,
  }));

  const standardsHistory = item.standardsHistory.map((h) => ({
    date: h.date,
    points: h.points,
    standardType: standardTypeRus[h.standardType],
    teacher: teachersMap.get(h.teacherGuid) ?? h.teacherGuid,
  }));

  const pointsHistory = item.pointsHistory.map((h) => ({
    date: h.date,
    points: h.points,
    workType: workTypeRus[h.workType],
    teacher: teachersMap.get(h.teacherGuid) ?? h.teacherGuid,
  }));

  return { ...item, visitsHistory, standardsHistory, pointsHistory };
}

export const ArchivedStudentListPage = () => {
  return (
    <DataGrid
      recordId="id"
      canExportCsv
      allColumns={columns}
      customCsvMapper={archivedStudentsCsvMapper}
      defaultColumnsKeys={[
        'studentGuid',
        'fullName',
        'groupNumber',
        'semesterName',
      ]}
      quickFilters={[
        { field: 'fullName', label: 'ФИО' },
        { field: 'semesterName', label: 'Семестер' },
      ]}
      filters={[
        { column: 'fullName', type: 'text', name: 'ФИО' },
        { column: 'studentGuid', type: 'text', name: 'Гуид студента' },
        { column: 'semesterName', type: 'text', name: 'Семестер' },
        { column: 'groupNumber', type: 'text', name: 'Группа' },
      ]}
    />
  );
};
