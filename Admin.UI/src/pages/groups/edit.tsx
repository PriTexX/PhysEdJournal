import { Input, Select } from '@chakra-ui/react';

import { NumberInput } from '@/shared/components/number-input';
import { createFormHelper, Form } from '@/widgets/form';

import type { Group } from './types';

const formHelper = createFormHelper<Group>();

const fields = [
  formHelper.createField('groupName', {
    name: 'Группа',
    render({ register }) {
      return <Input disabled {...register()} />;
    },
  }),

  formHelper.createField('visitValue', {
    name: 'Стоимость посещения',
    render({ control, name }) {
      return <NumberInput control={control} name={name} returnType="number" />;
    },
  }),

  formHelper.createField('curatorGuid', {
    name: 'Гуид куратора',
    render({ register }) {
      return <Input {...register()} />;
    },
  }),
];

export const GroupEditPage = () => {
  return <Form fields={fields} type="edit" />;
};
