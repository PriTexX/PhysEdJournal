import {
  NumberInput as ChakraNumberInput,
  NumberDecrementStepper,
  NumberIncrementStepper,
  NumberInputField,
  NumberInputStepper,
} from '@chakra-ui/react';
import { useController } from 'react-hook-form';

import { useValue } from './use-value';

import type { FieldRules } from '@/shared/types';
import type { Control, FieldValues, Path } from 'react-hook-form';

const useValueToDisplay = (v?: string | number | null) => {
  if (v === null || v === undefined || Number.isNaN(v)) {
    return '';
  }

  return v;
};

export interface NumberInputProps<T extends FieldValues> {
  control: Control<T>;
  name: Path<T>;
  rules?: FieldRules<T>;
  returnType?: 'string' | 'number';
  disabled?: boolean;
}

export const NumberInput = <T extends FieldValues>({
  control,
  name,
  rules,
  returnType,
  disabled,
}: NumberInputProps<T>) => {
  const {
    fieldState: { error },
    field: { onChange, value: _value },
  } = useController({ name, control, rules });

  const { value, setValue } = useValue(_value ?? null);
  const valueToDisplay = useValueToDisplay(value);

  return (
    <ChakraNumberInput
      value={valueToDisplay}
      onChange={(stringValue, numericValue) => {
        if (Number.isNaN(numericValue)) {
          return setValue(null);
        }

        if (returnType === 'string') {
          return setValue(stringValue);
        } else {
          return setValue(numericValue);
        }
      }}
      onBlur={() => onChange(value)}
      colorScheme={error ? 'red' : undefined}
    >
      <NumberInputField disabled={disabled} />

      {!disabled && (
        <NumberInputStepper>
          <NumberIncrementStepper />
          <NumberDecrementStepper />
        </NumberInputStepper>
      )}
    </ChakraNumberInput>
  );
};
