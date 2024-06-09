import { Box, HStack } from '@chakra-ui/react';

import { useTeachersMap } from '@/features/use-teachers-map';

import { LinkButton } from './link-button';

import type { FC } from 'react';

export type TeacherCellProps = {
  guid: string;
};

export const TeacherCell: FC<TeacherCellProps> = ({ guid }) => {
  const teachersMap = useTeachersMap();

  const displayTeacherName = teachersMap.get(guid) ?? guid;

  return (
    <HStack>
      <Box>{displayTeacherName}</Box>
      <Box display="inline-block">
        <LinkButton guid={guid} />
      </Box>
    </HStack>
  );
};
