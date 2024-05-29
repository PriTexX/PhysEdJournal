import { useEffect, useMemo } from 'react';

import { useTableSchema } from '@/features/modify-table';
import { areArraysEqual } from '@/shared/utils/are-arrays-equal';

import { useTranslateColumnKeys } from './use-translate-column-keys';

import type { TableSchema } from '@/features/modify-table/utils/table-schemas-provider';
import type { BaseRecord } from '@refinedev/core';
import type { ColumnDef } from '@tanstack/react-table';

const mapColumnsToItsNames = <D extends BaseRecord>(
  columnNames: string[],
  columns: ColumnDef<D, any>[],
) => {
  return columnNames
    .map((name) => columns.find((column) => column.header === name))
    .filter(Boolean) as ColumnDef<D, any>[];
};

const useMappedColumns = <D extends BaseRecord>(
  tableSchema: TableSchema | undefined,
  fallbackSchema: TableSchema,
  allColumns: ColumnDef<D, any>[],
) => {
  const mappedColumns = useMemo(() => {
    const getColumns = (schema: TableSchema) =>
      mapColumnsToItsNames(schema, allColumns);

    if (tableSchema) {
      const result = getColumns(tableSchema);
      if (result.length === 0) {
        return getColumns(fallbackSchema);
      }

      return result;
    }

    return getColumns(fallbackSchema);
  }, [tableSchema, fallbackSchema, allColumns]);

  return mappedColumns;
};

/** Returns columns, that match saved schema. If no schema saved, returns default columns */
export const useTableColumns = <D extends BaseRecord>(
  resourceName: string,
  allColumns: ColumnDef<D, any>[],
  defaultColumnsKeys: (keyof D)[],
) => {
  const { schema, setSchema } = useTableSchema(resourceName);

  const defaultColumnsNames = useTranslateColumnKeys(
    defaultColumnsKeys,
    allColumns,
  );

  const mappedColumns = useMappedColumns(
    schema,
    defaultColumnsNames,
    allColumns,
  );

  useEffect(() => {
    const newSchema = mappedColumns.map((c) => String(c.header));
    const schemasAreEqual = schema && areArraysEqual(schema, newSchema);

    if (schemasAreEqual) {
      return;
    }

    setSchema(mappedColumns.map((c) => String(c.header)));
  }, [mappedColumns, setSchema, schema]);

  return {
    columns: mappedColumns,
    defaultColumnsNames,
  };
};
