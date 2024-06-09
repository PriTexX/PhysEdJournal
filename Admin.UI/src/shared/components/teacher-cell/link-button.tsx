import { Box, IconButton } from '@chakra-ui/react';
import { IconLink } from '@tabler/icons-react';

import { useTeacherLink } from './use-teacher-link';

import type { FC } from 'react';

type LinkButtonProps = {
  guid: string;
};

export const LinkButton: FC<LinkButtonProps> = ({ guid }) => {
  const { Link, teacherUrl } = useTeacherLink(guid);

  return (
    <IconButton as={Link} to={teacherUrl} size="sm" aria-label="goto teacher">
      <IconLink size={20} />
    </IconButton>
  );
};
