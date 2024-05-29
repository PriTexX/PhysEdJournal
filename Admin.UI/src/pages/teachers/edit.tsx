import { Input, Select } from '@chakra-ui/react';

import { Checkbox } from '@/shared/components/checkbox';
import { NumberInput } from '@/shared/components/number-input';
import { createFormHelper, Form } from '@/widgets/form';

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
    render({ register }) {
      return (
        <Select {...register({ required: true })}>
          <option key="DefaultAccess" value="DefaultAccess">
            Обычный
          </option>
          <option key="OnlineCourseAccess" value="OnlineCourseAccess">
            СДО
          </option>
          <option key="SecretaryAccess" value="SecretaryAccess">
            Секретарь
          </option>
        </Select>
      );
    },
  }),
];

export const TeacherEditPage = () => {
  return <Form fields={fields} type="edit" />;
};
