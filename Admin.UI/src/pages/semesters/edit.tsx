import { Input } from '@chakra-ui/react';

import { Checkbox } from '@/shared/components/checkbox';
import { createFormHelper, Form } from '@/widgets/form';

import type { Semester } from './types';

const formHelper = createFormHelper<Semester>();

const fields = [
  formHelper.createField('name', {
    name: 'Название семестра',
    render({ register }) {
      return <Input disabled {...register()} />;
    },
  }),
  formHelper.createField('isCurrent', {
    name: 'Текущий',
    render({ control, name }) {
      return <Checkbox control={control} name={name} />;
    },
  }),
];

export const SemesterEditPage = () => {
  return <Form fields={fields} type="edit" />;
};
