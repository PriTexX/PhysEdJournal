import { IconButton, Tooltip } from '@chakra-ui/react';
import { IconChevronLeft } from '@tabler/icons-react';
import React from 'react';

import type { DataGridFilter } from '../utils/types';

interface PopoverHeaderProps<D> {
  selectedFilter: DataGridFilter<D> | null;
  onReturnBack: VoidFunction;
}

export const FilterPopoverHeader = <D,>({
  onReturnBack,
  selectedFilter,
}: PopoverHeaderProps<D>) => {
  return (
    <>
      {selectedFilter != null && (
        <Tooltip label="Go Back">
          <IconButton
            aria-label="Go Back"
            variant="ghost"
            position="absolute"
            top="50%"
            left="0"
            minW="6"
            h="6"
            transform="auto"
            translateY="-50%"
            icon={<IconChevronLeft size="16" />}
            onClick={onReturnBack}
          />
        </Tooltip>
      )}
      {selectedFilter ? selectedFilter.name ?? selectedFilter.column : 'Filter'}
    </>
  );
};
