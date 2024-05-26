import type { DataGridFilter } from '@/features/filters/utils/types';
import type {
  BaseRecord,
  CrudFilter,
  CrudFilters,
  LogicalFilter,
} from '@refinedev/core';

const isLogicalFilter = (
  crudFilter: CrudFilter,
): crudFilter is LogicalFilter => {
  return 'field' in crudFilter;
};

type FilterTypeMap<D extends BaseRecord> = Record<
  string,
  DataGridFilter<D>['type']
>;

type FixFunctionsMap<D extends BaseRecord> = Partial<{
  [key in DataGridFilter<D>['type']]: (filter: LogicalFilter) => void;
}>;

const getFilterTypeMap = <D extends BaseRecord>(
  filters: DataGridFilter<D>[],
): FilterTypeMap<D> => {
  const result = {} as FilterTypeMap<D>;

  for (const filter of filters) {
    result[filter.column] = filter.type;
  }
  return result;
};

const fixBooleanValue = (filter: LogicalFilter) => {
  if (filter.value === 'true') {
    filter.value = true;
  }

  if (filter.value === 'false') {
    filter.value = false;
  }
};

const fixNumberValue = (filter: LogicalFilter) => {
  const numericValue = Number(filter.value);
  if (!Number.isNaN(numericValue)) {
    filter.value = numericValue;
  }
};

const fixMethods: FixFunctionsMap<BaseRecord> = {
  boolean: fixBooleanValue,
  number: fixNumberValue,
};

/**
 * Fix artifacts when parsing from query params
 */
export const useFixCoreFilters = <D extends BaseRecord>(
  filters: CrudFilters,
  allFilters: DataGridFilter<D>[] = [],
) => {
  const filterTypeMap = getFilterTypeMap(allFilters);

  for (const filter of filters) {
    if (!isLogicalFilter(filter)) {
      continue;
    }

    const filterType = filterTypeMap[filter.field];

    const fixFilter = fixMethods[filterType];

    fixFilter && fixFilter(filter);
  }
};
