import { Input, Select } from '@chakra-ui/react';

import { Checkbox } from '@/shared/components/checkbox';
import { NumberInput } from '@/shared/components/number-input';
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

  formHelper.createField('fullName', {
    name: 'ФИО',
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

  formHelper.createField('course', {
    name: 'Курс',
    render({ control, name }) {
      return <NumberInput control={control} name={name} />;
    },
  }),

  formHelper.createField('visits', {
    name: 'Посещения',
    render({ control, name }) {
      return <NumberInput disabled control={control} name={name} />;
    },
  }),

  formHelper.createField('pointsForStandards', {
    name: 'Баллы за нормативы',
    render({ control, name }) {
      return <NumberInput disabled control={control} name={name} />;
    },
  }),

  formHelper.createField('additionalPoints', {
    name: 'Доп. баллы',
    render({ control, name }) {
      return <NumberInput disabled control={control} name={name} />;
    },
  }),

  formHelper.createField('archivedVisitValue', {
    name: 'Стоимость посещения (если есть долг)',
    render({ register }) {
      return <Input {...register({ valueAsNumber: true })} />;
    },
  }),

  formHelper.createField('hasDebtFromPreviousSemester', {
    name: 'Долг',
    render({ control, name }) {
      return <Checkbox control={control} name={name} />;
    },
  }),

  formHelper.createField('hadDebtInSemester', {
    name: 'Был ли долг',
    render({ control, name }) {
      return <Checkbox control={control} name={name} />;
    },
  }),

  formHelper.createField('isActive', {
    name: 'Активен',
    render({ control, name }) {
      return <Checkbox control={control} name={name} />;
    },
  }),

  formHelper.createField('healthGroup', {
    name: 'Группа здоровья',
    render({ register }) {
      return (
        <Select {...register({ required: true })}>
          <option key="None" value="None">
            Нет
          </option>
          <option key="Basic" value="Basic">
            Базовая
          </option>
          <option key="Preparatory" value="Preparatory">
            Подготовительная
          </option>
          <option key="Special" value="Special">
            Специальная
          </option>
          <option key="HealthLimitations" value="HealthLimitations">
            Ограниченная
          </option>
        </Select>
      );
    },
  }),

  formHelper.createField('department', {
    name: 'Кафедра',
    render({ register }) {
      return <Input disabled {...register()} />;
    },
  }),

  formHelper.createField('currentSemesterName', {
    name: 'Текущий семестр',
    render({ register }) {
      return <Input disabled {...register()} />;
    },
  }),
];

export const StudentEditPage: React.FC<IResourceComponentsProps> = () => {
  return <Form fields={fields} type="edit" />;
};
