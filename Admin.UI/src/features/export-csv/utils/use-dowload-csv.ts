import { useResource } from '@refinedev/core';
import { saveAs } from 'file-saver';
import { json2csv } from 'json-2-csv';
import { useCallback, useEffect, useState } from 'react';

import { useTableColumns } from '@/widgets/data-grid/utils/use-table-columns';

import type { BaseRecord } from '@refinedev/core';
import type { ColumnDef } from '@tanstack/react-table';

export type UseDownloadCsvProps<D extends BaseRecord> = {
  enabled: boolean;
  allColumns: ColumnDef<D, any>[];
  defaultColumnsKeys: (keyof D)[];
};

const mapColumnHeadersToAccessors = <D extends BaseRecord>(
  columns: ColumnDef<D, any>[],
  allColumns: ColumnDef<D, any>[],
) => {
  return columns.map((c) => {
    const col = allColumns.find((a) => a.header == c.header);

    return col
      ? { field: (col as any).accessorKey as string, title: c.header as string }
      : '';
  });
};

export const useDownloadCsv = <D extends BaseRecord>({
  enabled,
  allColumns,
  defaultColumnsKeys,
}: UseDownloadCsvProps<D>) => {
  const { resource } = useResource();

  const [status, setStatus] = useState<'success' | 'error' | null>(null);

  const { columns } = useTableColumns(
    resource?.name ?? '',
    allColumns,
    defaultColumnsKeys,
  );

  const csvKeys = mapColumnHeadersToAccessors(columns, allColumns);

  const fileName = resource?.name ?? 'result';

  useEffect(() => {
    if (!enabled) {
      setStatus(null);
    }
  }, [enabled]);

  const downloadCsv = useCallback(
    (data: object[]) => {
      if (!enabled) {
        return;
      }

      try {
        const csv = json2csv(data, { keys: csvKeys });

        const blob = new Blob([csv], {
          type: 'text/csv;charset=utf-8;',
        });

        saveAs(blob, fileName);

        setStatus('success');
      } catch {
        setStatus('error');
        return;
      }
    },
    [enabled, fileName],
  );

  return { status, downloadCsv, resetStatus: () => setStatus(null) };
};
