import { createContext } from 'react';
import { useLocalStorage } from 'usehooks-ts';
import { z } from 'zod';

import type { Dispatch, FC, PropsWithChildren, SetStateAction } from 'react';

const tableSchema = z.array(z.string());

const tableSchemasSchema = z.record(z.string(), tableSchema.optional());

export type TableSchema = z.infer<typeof tableSchema>;

export type TableSchemas = z.infer<typeof tableSchemasSchema>;

export const DataGridContext = createContext<{
  schemas: TableSchemas;
  setSchemas: Dispatch<SetStateAction<Record<string, string[] | undefined>>>;
}>({ schemas: {}, setSchemas: () => {} });

export const TableSchemasProvider: FC<PropsWithChildren> = ({ children }) => {
  const [tableSchemas, setTableSchemas] = useLocalStorage<TableSchemas>(
    'table-schemas',
    {},
    {
      serializer(value) {
        return JSON.stringify(value);
      },

      deserializer(value) {
        try {
          const parsedJson = JSON.parse(value);
          const parsedValue = tableSchemasSchema.parse(parsedJson);

          return parsedValue;
        } catch {
          return {};
        }
      },
    },
  );

  return (
    <DataGridContext.Provider
      value={{ schemas: tableSchemas, setSchemas: setTableSchemas }}
    >
      {children}
    </DataGridContext.Provider>
  );
};
