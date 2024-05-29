import { Box } from '@chakra-ui/react';
import React from 'react';

import type { FC, PropsWithChildren } from 'react';

export const FormGroupWrapper: FC<PropsWithChildren> = ({ children }) => {
  return <Box mb="5">{children}</Box>;
};
