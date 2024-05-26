import { toISO } from '@/shared/utils/dates';

import { filterOptionToOperatorMap } from './date-operators';

import type { DateFilterOperation, DateFilterOption } from './date-operators';
import type { Dayjs } from 'dayjs';

export const getValueAndOperator = (
  inputValues: [Dayjs | null, Dayjs | null],
  option: DateFilterOption,
): { value: unknown; operator: DateFilterOperation } => {
  const operator = filterOptionToOperatorMap[option];

  const [firstValue, secondValue] = inputValues;

  if (option === 'Equals' || option === 'Not equals') {
    return {
      value: [
        toISO(firstValue?.startOf('minute') ?? null),
        toISO(firstValue?.endOf('minute') ?? null),
      ],
      operator: operator,
    };
  }

  if (option === 'Between') {
    return {
      value: [
        toISO(firstValue?.startOf('minute') ?? null),
        toISO(secondValue?.endOf('minute') ?? null),
      ],
      operator,
    };
  }

  if (option === 'Less than or equals' || option === 'Greater than') {
    const date = firstValue?.endOf('minute') ?? null;
    return { value: toISO(date), operator };
  }

  if (option === 'Greater than or equals' || option === 'Less than') {
    const date = firstValue?.startOf('minute') ?? null;
    return { value: toISO(date), operator };
  }

  return { value: toISO(firstValue), operator };
};
