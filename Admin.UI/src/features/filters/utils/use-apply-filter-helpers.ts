import type { CrudFilter, CrudFilters, LogicalFilter } from '@refinedev/core';

export const useApplyFiltersHelpers = () => {
  const updateFilter = (
    appliedFilters: CrudFilters,
    filterToUpdate: LogicalFilter,
  ): CrudFilter[] => {
    return [
      ...appliedFilters.filter((f) =>
        'field' in f ? f.field !== filterToUpdate.field : true,
      ),
      filterToUpdate,
    ];
  };

  const clearFilter = (
    appliedFilters: CrudFilters,
    fieldToClear: string,
  ): CrudFilter[] => {
    return appliedFilters.filter((f) =>
      'field' in f ? f.field !== fieldToClear : true,
    );
  };

  return { updateFilter, clearFilter };
};
