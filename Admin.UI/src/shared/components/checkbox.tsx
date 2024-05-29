import { Checkbox as ChakraCheckbox } from '@chakra-ui/react';
import { useController } from 'react-hook-form';

import type { Control, FieldValues, Path } from 'react-hook-form';

export interface CheckboxProps<T extends FieldValues> {
  control: Control<T>;
  name: Path<T>;
  disabled?: boolean;
}

export const Checkbox = <T extends FieldValues>({
  control,
  name,
  disabled,
}: CheckboxProps<T>) => {
  const {
    field: { value, ...props },
  } = useController({ name, control, disabled });

  return <ChakraCheckbox isChecked={value} {...props} />;
};
