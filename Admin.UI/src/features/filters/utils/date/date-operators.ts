import type { AllowedFilterOperation } from '@/app/utils/data-provider';

export const dateFilterOperators = [
  'gt',
  'gte',
  'lt',
  'lte',
  'between',
  'nbetween',
] satisfies AllowedFilterOperation[];

export type DateFilterOperation = (typeof dateFilterOperators)[number];

export const filterOptionToOperatorMap = {
  Equals: 'between', // between start and the end of minute
  'Not equals': 'nbetween', // not between start and end of minute
  Between: 'between',
  'Greater than': 'gt',
  'Greater than or equals': 'gte',
  'Less than': 'lt',
  'Less than or equals': 'lte',
} as const satisfies Record<string, DateFilterOperation>;

export type DateFilterOption = keyof typeof filterOptionToOperatorMap;
