import { Input } from '@chakra-ui/react';

import { createFormHelper, Form } from '@/widgets/form';

import type { Semester } from './types';

const formHelper = createFormHelper<Semester>();

const fields = [
  formHelper.createField('name', {
    name: 'Название семестра',
    render({ register }) {
      return <Input {...register({ required: true })} />;
    },
  }),
];

export const SemesterCreatePage = () => {
  return <Form fields={fields} type="create" />;
};
