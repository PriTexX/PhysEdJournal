import {
  Badge,
  Button,
  Popover,
  PopoverArrow,
  PopoverContent,
  PopoverHeader,
  PopoverTrigger,
  Portal,
  useOutsideClick,
} from '@chakra-ui/react';
import * as React from 'react';

import { useApplyFiltersHelpers } from '../utils/use-apply-filter-helpers';
import { useFiltersState } from '../utils/use-filters-state';
import { FilterPopoverHeader } from './filter-popover-header';
import { FiltersList } from './filters-list';
import { SelectedFilter } from './selected-filter';
import SelectedFilterControls from './selected-filter-controls';

import type { DataGridFilter } from '../utils/types';
import type { BaseRecord, CrudFilters, LogicalFilter } from '@refinedev/core';

type FilterProps<D> = {
  filtersList: DataGridFilter<D>[];
  filtersApplied: CrudFilters;
  onApplyFilters: (filters: CrudFilters) => void;
};

function Filter<D extends BaseRecord>({
  filtersApplied,
  filtersList,
  onApplyFilters,
}: FilterProps<D>) {
  const [selectedFilter, setSelectedFilter] =
    React.useState<DataGridFilter<D> | null>(null);
  const [logicalFilter, setLogicalFilter] =
    React.useState<LogicalFilter | null>(null);

  const [isOpen, setIsOpen] = React.useState(false);

  const triggerRef = React.useRef<HTMLButtonElement | null>(null);
  const popoverRef = React.useRef<HTMLElement | null>(null);
  useOutsideClick({
    ref: popoverRef,
    handler(e) {
      if (!isOpen || e.target == triggerRef.current) {
        return;
      }

      setIsOpen(false);
    },
  });

  const filtersState = useFiltersState(filtersApplied);

  const appliedFiltersAmount = filtersApplied.length;

  const { clearFilter, updateFilter } = useApplyFiltersHelpers();

  return (
    <Popover isOpen={isOpen}>
      <PopoverTrigger>
        <Button
          onClick={() => setIsOpen(!isOpen)}
          variant={appliedFiltersAmount ? 'solid' : 'outline'}
          ref={triggerRef}
          display="flex"
          alignItems="center"
          gap="0.5rem"
        >
          Filter
          {appliedFiltersAmount > 0 ? (
            <Badge colorScheme="blue">{appliedFiltersAmount}</Badge>
          ) : null}
        </Button>
      </PopoverTrigger>
      <Portal>
        <PopoverContent ref={popoverRef} p="2" gap="1" w="64" maxH="600px">
          <PopoverArrow />
          <PopoverHeader
            justifyContent="center"
            display="flex"
            fontSize="small"
            fontWeight="bold"
            py="1"
            position="relative"
          >
            <FilterPopoverHeader
              onReturnBack={() => {
                setSelectedFilter(null);
                setLogicalFilter(null);
              }}
              selectedFilter={selectedFilter}
            />
          </PopoverHeader>

          {selectedFilter == null && (
            <>
              <FiltersList
                filters={filtersList}
                filtersState={filtersState}
                setLogicalFilter={setLogicalFilter}
                setSelectedFilter={setSelectedFilter}
              />
              {appliedFiltersAmount > 0 && (
                <Button
                  flexShrink={0}
                  onClick={() => {
                    onApplyFilters([]);
                  }}
                  mt="3"
                >
                  Reset all
                </Button>
              )}
            </>
          )}

          {selectedFilter != null && logicalFilter != null && (
            <form>
              <SelectedFilter
                logicalFilter={logicalFilter}
                selectedFilter={selectedFilter}
                setLogicalFilter={setLogicalFilter}
              />

              <SelectedFilterControls
                onReset={() => {
                  const newFilters = clearFilter(
                    filtersApplied,
                    logicalFilter.field,
                  );

                  onApplyFilters(newFilters);

                  setSelectedFilter(null);
                  setLogicalFilter(null);
                }}
                onSave={() => {
                  const newFilters = updateFilter(
                    filtersApplied,
                    logicalFilter,
                  );

                  onApplyFilters(newFilters);

                  setSelectedFilter(null);
                  setLogicalFilter(null);
                }}
              />
            </form>
          )}
        </PopoverContent>
      </Portal>
    </Popover>
  );
}

export default Filter;
