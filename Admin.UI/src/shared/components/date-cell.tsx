import { Box } from '@chakra-ui/react';

import { fromISO } from '@/shared/utils/dates';

export const DateCell: React.FC<{ date: string | null }> = ({ date }) => {
  const valueToDisplay = fromISO(date);

  if (!valueToDisplay) {
    return null;
  }

  return (
    <Box>
      <Box mb={1}>{valueToDisplay.format('HH:mm')}</Box>
      <Box>{valueToDisplay.format('DD/MM/YYYY')}</Box>
    </Box>
  );
};
