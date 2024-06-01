import { filterOptionToOperatorMap } from './date-operators';

import type { DateFilterOperation, DateFilterOption } from './date-operators';
import type { Dayjs } from 'dayjs';

export const getValueAndOperator = (
  inputValues: [Dayjs | null, Dayjs | null],
  option: DateFilterOption,
): { value: unknown; operator: DateFilterOperation } => {
  const operator = filterOptionToOperatorMap[option];

  const [firstValue, secondValue] = inputValues;

  return {
    value: secondValue
      ? [firstValue?.format('MM/DD/YYYY'), secondValue.format('MM/DD/YYYY')]
      : firstValue?.format('MM/DD/YYYY'),
    operator,
  };
};
