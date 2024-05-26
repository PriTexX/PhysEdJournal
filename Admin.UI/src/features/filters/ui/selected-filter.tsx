import { VStack } from '@chakra-ui/react';
import React, { useMemo } from 'react';

import { combineFilterWithNullFilter } from '../utils/combine-with-null-filter';
import {
  BooleanFilter,
  DateFilter,
  NullFilter,
  NumberFilter,
  SelectFilter,
  TextFilter,
} from './filters';

import type {
  DataGridFilter,
  DataGridFilterType,
  XFilterProps,
} from '@/features/filters/utils/types';
import type { LogicalFilter } from '@refinedev/core';
import type { Dispatch, SetStateAction } from 'react';

const filterComponentMap: Record<
  DataGridFilterType,
  React.JSXElementConstructor<XFilterProps>
> = {
  boolean: BooleanFilter,
  text: TextFilter,
  date: DateFilter,
  number: NumberFilter,
  select: SelectFilter,
  null: NullFilter,
};

interface SelectedFilterProps<D> {
  selectedFilter: DataGridFilter<D>;
  logicalFilter: LogicalFilter;
  setLogicalFilter: Dispatch<SetStateAction<LogicalFilter | null>>;
}

export const SelectedFilter = <D,>({
  selectedFilter,
  logicalFilter,
  setLogicalFilter,
}: SelectedFilterProps<D>) => {
  const FilterComponent = useMemo(() => {
    let Filter = selectedFilter
      ? filterComponentMap[selectedFilter.type]
      : null;

    if (selectedFilter.withNullChecking && Filter) {
      Filter = combineFilterWithNullFilter(Filter);
    }

    return Filter;
  }, [selectedFilter]);

  return (
    <VStack mt="1" gap="3">
      {FilterComponent && (
        <FilterComponent
          logicalFilter={logicalFilter}
          onChange={setLogicalFilter}
          additionalInfo={
            selectedFilter.type === 'select'
              ? selectedFilter.options
              : undefined
          }
        />
      )}
    </VStack>
  );
};
