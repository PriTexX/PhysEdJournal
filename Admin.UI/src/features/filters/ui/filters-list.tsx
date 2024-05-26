import { Button, VStack } from '@chakra-ui/react';
import { IconChevronRight } from '@tabler/icons-react';

import type { DataGridFilter } from '../utils/types';
import type { LogicalFilter } from '@refinedev/core';
import type { Dispatch, SetStateAction } from 'react';

export interface FiltersListProps<D> {
  filters: DataGridFilter<D>[];
  filtersState: Record<string, LogicalFilter | undefined>;
  setSelectedFilter: Dispatch<SetStateAction<DataGridFilter<D> | null>>;
  setLogicalFilter: Dispatch<SetStateAction<LogicalFilter | null>>;
}

export const FiltersList = <D,>({
  filters,
  filtersState,
  setLogicalFilter,
  setSelectedFilter,
}: FiltersListProps<D>) => {
  return (
    <VStack w="full" alignItems="stretch" overflowY="auto">
      {filters.map((filter) => (
        <Button
          key={filter.column}
          variant={filtersState[filter.column] ? 'solid' : 'outline'}
          rightIcon={<IconChevronRight size="20" />}
          justifyContent="space-between"
          alignItems="center"
          fontSize="sm"
          fontWeight="normal"
          flexShrink="0"
          onClick={() => {
            setSelectedFilter(filter);
            setLogicalFilter(
              filtersState[filter.column] ??
                ({
                  field: filter.column,
                  operator: 'contains',
                  value: undefined,
                } satisfies LogicalFilter),
            );
          }}
        >
          {filter.name ?? filter.column}
        </Button>
      ))}
    </VStack>
  );
};
