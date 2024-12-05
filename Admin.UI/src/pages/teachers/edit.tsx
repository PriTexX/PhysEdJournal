import { Input, Select } from '@chakra-ui/react';

import { createFormHelper, Form } from '@/widgets/form';

import { MultiSelect } from './ui/MultiSelect';

import type { Teacher } from './types';

const formHelper = createFormHelper<Teacher>();

const fields = [
  formHelper.createField('teacherGuid', {
    name: 'Гуид преподавателя',
    render({ register }) {
      return <Input disabled {...register()} />;
    },
  }),

  formHelper.createField('fullName', {
    name: 'ФИО',
    render({ register }) {
      return <Input {...register()} />;
    },
  }),

  formHelper.createField('permissions', {
    name: 'Права',
    render(props) {
      return (
        <MultiSelect
          {...props}
          options={[
            { value: 'DefaultAccess', label: 'Обычный' },
            { value: 'SecretaryAccess', label: 'Секретарь' },
            { value: 'OnlineCourseAccess', label: 'СДО' },
            { value: 'CompetitionAccess', label: 'Соревнования' },
          ]}
        />
      );
    },
  }),
];

export const TeacherEditPage = () => {
  return <Form fields={fields} type="edit" />;
};
