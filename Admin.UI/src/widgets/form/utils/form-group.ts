import type { BaseRecord, Field } from '../ui/form';

export type FormGroup<D extends BaseRecord> = {
  type: 'group';
  name?: string;
  fields: Field<D, any>[];
};

export const isFormGroup = <T extends BaseRecord>(
  value: FormGroup<T> | Field<T, any>,
): value is FormGroup<T> => {
  return 'fields' in value;
};
