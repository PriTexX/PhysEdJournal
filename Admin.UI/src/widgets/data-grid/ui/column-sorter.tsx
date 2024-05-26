import { IconButton, Tooltip } from '@chakra-ui/react';
import { IconChevronDown, IconSelector } from '@tabler/icons-react';
import * as React from 'react';

import type { SortDirection } from '@tanstack/react-table';

type ColumnSorterProps = {
  isSorted: false | SortDirection;
  onToggleSort: undefined | ((event: unknown) => void);
};

const ColumnSorter: React.FC<ColumnSorterProps> = ({
  isSorted,
  onToggleSort,
}) => {
  return (
    <Tooltip label="Sort">
      <IconButton
        aria-label="Sort"
        size="xs"
        onClick={onToggleSort}
        variant="ghost"
        _dark={{ color: 'var(--chakra-colors-gray-400);' }}
        _light={{ color: 'var(--chakra-colors-gray-600)' }}
        icon={
          <>
            {!isSorted && <IconSelector size={18} />}
            {isSorted && (
              <span
                style={{
                  transition:
                    'transform var(--chakra-transition-duration-faster) ease-in-out',
                  transform: `rotate(${isSorted === 'asc' ? '180' : '0'}deg)`,
                }}
              >
                <IconChevronDown size={18} />
              </span>
            )}
          </>
        }
      ></IconButton>
    </Tooltip>
  );
};

export default ColumnSorter;
