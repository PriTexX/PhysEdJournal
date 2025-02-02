import { Input, Select } from '@chakra-ui/react';

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
    render({ register }) {
      return (
        <Select {...register({ required: true })}>
          <option key={0} value={2.0}>
            2.0
          </option>
          <option key={0} value={2.5}>
            2.5
          </option>
          <option key={0} value={3.0}>
            3.0
          </option>
          <option key={0} value={3.5}>
            3.5
          </option>
          <option key={0} value={4.0}>
            4.0
          </option>
        </Select>
      );
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
