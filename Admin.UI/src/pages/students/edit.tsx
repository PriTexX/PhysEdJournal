import { Input } from '@chakra-ui/react';

import { createFormHelper, Form } from '@/widgets/form';

import type { Student } from './types';
import type { IResourceComponentsProps } from '@refinedev/core';

const formHelper = createFormHelper<Student>();

const fields = [
  formHelper.createField('studentGuid', {
    name: 'Гуид студента',
    render({ register }) {
      return <Input disabled {...register()} />;
    },
  }),

  formHelper.createField('groupNumber', {
    name: 'Группа',
    render({ register }) {
      return <Input {...register()} />;
    },
  }),
];

export const StudentEdit: React.FC<IResourceComponentsProps> = () => {
  return <Form fields={fields} type="edit" />;
};
