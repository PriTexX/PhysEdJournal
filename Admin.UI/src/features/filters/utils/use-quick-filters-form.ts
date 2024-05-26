import { useEffect, useState } from 'react';

import { useApplyFiltersHelpers } from './use-apply-filter-helpers';
import { useFiltersState } from './use-filters-state';

import type { BaseRecord, CrudFilters, LogicalFilter } from '@refinedev/core';

export const useQuickFiltersForm = <D extends BaseRecord>(
  filtersApplied: CrudFilters,
  searchByFields: Array<keyof D>,
) => {
  const filtersState = useFiltersState(filtersApplied);

  const [form, setForm] = useState(filtersState);

  const { clearFilter, updateFilter } = useApplyFiltersHelpers();

  useEffect(() => {
    setForm(filtersState);
  }, [filtersState]);

  const getFormValue = (field: keyof D): LogicalFilter => {
    const defaultValue = {
      field: field as string,
      value: '',
      operator: 'contains',
    } satisfies LogicalFilter;

    if (form[field]?.operator === 'contains') {
      return form[field as string] ?? defaultValue;
    }

    return defaultValue;
  };

  const setFormValue = (field: keyof D, newValue: LogicalFilter) => {
    setForm((prevForm) => {
      return {
        ...prevForm,
        [field as string]: newValue,
      };
    });
  };

  const handleSubmitForm = (submitCallback: (filters: CrudFilters) => void) => {
    let preparedFilters = filtersApplied;

    for (const field of searchByFields) {
      if (!form[field]?.value) {
        preparedFilters = clearFilter(preparedFilters, field as string);
      } else {
        const logicalFilter = form[field as string];

        if (logicalFilter) {
          preparedFilters = updateFilter(preparedFilters, logicalFilter);
        }
      }
    }

    submitCallback(preparedFilters);
  };

  const resetForm = (submitCallback: (filters: CrudFilters) => void) => {
    const preparedFilters = filtersApplied.filter((f) =>
      'field' in f ? !searchByFields.includes(f.field) : true,
    );

    submitCallback(preparedFilters);
  };

  return { setFormValue, getFormValue, handleSubmitForm, resetForm };
};
