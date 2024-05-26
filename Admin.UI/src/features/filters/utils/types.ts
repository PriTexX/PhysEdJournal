import type { LogicalFilter } from '@refinedev/core';

export type DataGridFilterType =
  | 'boolean'
  | 'number'
  | 'text'
  | 'date'
  | 'select'
  | 'null';

export type DataGridSelectFilter = {
  type: 'select';
  options: Readonly<string[]>;
};

export type DataGridSimpleFilter = {
  type: Exclude<DataGridFilterType, 'select'>;
};

export type XFilterProps<T = any> = {
  logicalFilter: LogicalFilter;
  onChange: (filter: LogicalFilter) => void;
  additionalInfo?: T;
};

export type DataGridFilter<D> = {
  name?: string;
  column: keyof D & string;
  withNullChecking?: boolean;
} & (DataGridSimpleFilter | DataGridSelectFilter);
