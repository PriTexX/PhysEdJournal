import { Box, Button, HStack, IconButton, Tooltip } from '@chakra-ui/react';
import { usePagination } from '@refinedev/chakra-ui';
import { IconChevronLeft, IconChevronRight } from '@tabler/icons-react';
import * as React from 'react';

import type { ResponsiveValue } from '@chakra-ui/react';

type PaginationProps = {
  current: number;
  pageCount: number;
  setCurrent: (page: number) => void;
  justifyContent?: 'flex-end' | 'center';
};

export const Pagination: React.FC<PaginationProps> = ({
  current,
  pageCount,
  setCurrent,
  justifyContent = 'flex-end',
}) => {
  const pagination = usePagination({
    current,
    pageCount,
  });

  React.useEffect(() => {
    window.scrollTo({ top: 0, left: 0, behavior: 'smooth' });
  }, [current]);

  return (
    <Box display="flex" justifyContent={justifyContent}>
      <HStack my="3" spacing="1">
        {pagination?.prev && (
          <Tooltip label="Previous">
            <IconButton
              aria-label="Previous"
              onClick={() => setCurrent(current - 1)}
              disabled={!pagination?.prev}
              variant="outline"
            >
              <IconChevronLeft size="18" />
            </IconButton>
          </Tooltip>
        )}

        {pagination?.items.map((page, index, array) => {
          if (typeof page === 'string')
            return (
              <span key={`delimiter-after-${array.at(index - 1)}`}>...</span>
            );

          return (
            <Button
              key={page}
              onClick={() => setCurrent(page)}
              variant={page === current ? 'solid' : 'outline'}
            >
              {page}
            </Button>
          );
        })}
        {pagination?.next && (
          <Tooltip label="Next">
            <IconButton
              aria-label="Next"
              onClick={() => setCurrent(current + 1)}
              variant="outline"
            >
              <IconChevronRight size="18" />
            </IconButton>
          </Tooltip>
        )}
      </HStack>
    </Box>
  );
};
