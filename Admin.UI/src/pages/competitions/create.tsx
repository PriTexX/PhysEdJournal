import { Input } from '@chakra-ui/react';

import { createFormHelper, Form } from '@/widgets/form';

import type { Competition } from './types';

const formHelper = createFormHelper<Competition>();

const fields = [
  formHelper.createField('competitionName', {
    name: 'Название соревнования',
    render({ register }) {
      return <Input {...register({ required: true })} />;
    },
  }),
];

export const CompetitionCreatePage = () => {
  return <Form fields={fields} type="create" />;
};
