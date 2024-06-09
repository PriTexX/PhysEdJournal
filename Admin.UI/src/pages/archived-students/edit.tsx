import { Input } from '@chakra-ui/react';

import { JsonEditor } from '@/shared/components/json-editor';
import { NumberInput } from '@/shared/components/number-input';
import { createFormHelper, Form } from '@/widgets/form';

import type { ArchivedStudent } from './types';

const formHelper = createFormHelper<ArchivedStudent>();

const fields = [
  formHelper.createField('studentGuid', {
    name: 'Гуид студента',
    render({ register }) {
      return <Input disabled {...register()} />;
    },
  }),

  formHelper.createField('fullName', {
    name: 'ФИО',
    render({ register }) {
      return <Input disabled {...register()} />;
    },
  }),

  formHelper.createField('groupNumber', {
    name: 'Группа',
    render({ register }) {
      return <Input disabled {...register()} />;
    },
  }),

  formHelper.createField('semesterName', {
    name: 'Семестер',
    render({ register }) {
      return <Input disabled {...register()} />;
    },
  }),

  formHelper.createField('visits', {
    name: 'Группа',
    render({ control, name }) {
      return <NumberInput disabled control={control} name={name} />;
    },
  }),

  formHelper.createField('visitsHistory', {
    name: 'Группа',
    render({ control, name }) {
      return <JsonEditor control={control} name={name} />;
    },
  }),
];

export const ArchivedStudentEditPage = () => {
  return <Form fields={fields} type="edit" />;
};
