import { Box } from '@chakra-ui/react';
import dayjs from 'dayjs';

export const DateCell: React.FC<{ date: string | Date }> = ({ date }) => {
  return <Box>{dayjs(date).format('DD-MM-YYYY')}</Box>;
};
