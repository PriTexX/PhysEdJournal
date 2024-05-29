import { useMemo } from 'react';

import type { BaseRecord } from '@refinedev/core';
import type { ColumnDef, ColumnDefResolved } from '@tanstack/react-table';

export const useTranslateColumnKeys = <D extends BaseRecord>(
  columnKeys: (keyof D)[],
  columns: ColumnDef<D, any>[],
): string[] => {
  const columnNames = useMemo(() => {
    const keyToNameMap = new Map<keyof D, string>(
      columns.map((c) => {
        const key = String((c as ColumnDefResolved<D, any>).accessorKey);
        const header = String(c.header);

        return [key, header];
      }),
    );

    return columnKeys.map((key) => keyToNameMap.get(key)).filter(Boolean);
  }, [columnKeys, columns]);

  return columnNames as string[];
};
