import React from 'react';

import type { CrudFilters, LogicalFilter } from '@refinedev/core';

export const useFiltersState = (filtersApplied: CrudFilters) => {
  const filtersState = React.useMemo(
    () =>
      filtersApplied.reduce(
        (acc, filter) => {
          if (
            filter.operator == 'and' ||
            filter.operator == 'or' ||
            !('field' in filter)
          ) {
            return acc;
          }

          acc[filter.field] = filter;

          return acc;
        },
        {} as Record<string, LogicalFilter | undefined>,
      ),
    [filtersApplied],
  );

  return filtersState;
};
