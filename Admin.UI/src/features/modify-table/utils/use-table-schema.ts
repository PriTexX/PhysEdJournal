import { useCallback, useContext } from 'react';

import { DataGridContext } from './table-schemas-provider';

import type { TableSchema } from './table-schemas-provider';

export const useTableSchema = (resourceName: string) => {
  const { schemas, setSchemas } = useContext(DataGridContext);

  const setSchema = useCallback(
    (newSchema: TableSchema) => {
      setSchemas((oldRecord) => ({
        ...oldRecord,
        [resourceName]: newSchema,
      }));
    },
    [setSchemas, resourceName],
  );

  return {
    schema: schemas[resourceName],
    setSchema,
  };
};
