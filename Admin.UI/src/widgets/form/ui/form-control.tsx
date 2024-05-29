import {
  FormControl as ChakraFormControl,
  FormErrorMessage,
  FormLabel,
} from '@chakra-ui/react';
import React from 'react';

import type { FC, PropsWithChildren } from 'react';

export interface FormControlProps extends PropsWithChildren {
  error?: string;
  isDisabled: boolean;
  isInvalid: boolean;
  fieldName: string;
}

export const FormControl: FC<FormControlProps> = ({
  fieldName,
  isInvalid,
  isDisabled,
  error,
  children,
}) => {
  return (
    <ChakraFormControl mb="3" isInvalid={isInvalid} isDisabled={isDisabled}>
      <FormLabel>{fieldName}</FormLabel>

      {children}

      <FormErrorMessage>{error}</FormErrorMessage>
    </ChakraFormControl>
  );
};
