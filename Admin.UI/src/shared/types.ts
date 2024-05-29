import type { FieldValues, Path, RegisterOptions } from 'react-hook-form';

export type FieldRules<T extends FieldValues> =
  | Omit<
      RegisterOptions<T, Path<T>>,
      'valueAsNumber' | 'valueAsDate' | 'setValueAs' | 'disabled'
    >
  | undefined;
