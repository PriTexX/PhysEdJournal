import { useFormControlContext } from '@chakra-ui/react';

export const useIsDisabled = () => {
  const { isDisabled } = useFormControlContext();
  return isDisabled;
};
