import type { AllowedFilterOperation } from '@/app/utils/data-provider';

export const dateFilterOperators = [
  'eq',
  'ne',
  'gt',
  'lt',
  'between',
] satisfies AllowedFilterOperation[];

export type DateFilterOperation = (typeof dateFilterOperators)[number];

export const filterOptionToOperatorMap = {
  Соответствует: 'eq',
  'Не Соответствует': 'ne',
  Между: 'between',
  После: 'gt',
  До: 'lt',
} as const satisfies Record<string, DateFilterOperation>;

export type DateFilterOption = keyof typeof filterOptionToOperatorMap;
