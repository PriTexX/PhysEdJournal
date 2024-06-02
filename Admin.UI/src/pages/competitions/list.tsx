import { createColumnHelper } from '@tanstack/react-table';

import { DataGrid } from '@/widgets/data-grid/ui/data-grid';

import type { Competition } from './types';
import type { IResourceComponentsProps } from '@refinedev/core';

const columnHelper = createColumnHelper<Competition>();

const columns = [
  columnHelper.accessor('competitionName', {
    header: 'Название соревнования',
    enableSorting: false,
  }),
];

export const CompetitionListPage: React.FC<IResourceComponentsProps> = () => {
  return (
    <DataGrid
      recordId="competitionName"
      canExportCsv
      allColumns={columns}
      defaultColumnsKeys={['competitionName']}
      quickFilters={[{ field: 'competitionName', label: 'Соревнование' }]}
    />
  );
};
