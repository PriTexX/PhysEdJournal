import type { BaseRecord, Field } from '../ui/form';
import type { FormGroup } from './form-group';
import type { Path } from 'react-hook-form';

const createFieldHelper =
  <D extends BaseRecord>() =>
  <K extends Path<D>>(
    path: K,
    options: Omit<Field<D, K>, 'path' | 'type'>,
  ): Field<D, K> => ({
    path,
    ...options,
    type: 'field',
  });

const createGroupHelper = <D extends BaseRecord>(
  fieldHelper: ReturnType<typeof createFieldHelper<D>>,
) => {
  type FieldHelperParams = Parameters<typeof fieldHelper>;
  return (
    groupName: string,
    ...fields: ({ path: FieldHelperParams[0] } & FieldHelperParams[1])[]
  ): FormGroup<D> => ({
    type: 'group',
    fields: fields.map(({ path, name, render }) =>
      fieldHelper(path, { name, render }),
    ),
    name: groupName,
  });
};

/** Provides functions to build forms/groups inside create/update pages */
export const createFormHelper = <D extends BaseRecord>() => {
  const createField = createFieldHelper<D>();
  const createGroup = createGroupHelper<D>(createField);

  return {
    createField,
    createGroup,
  };
};
