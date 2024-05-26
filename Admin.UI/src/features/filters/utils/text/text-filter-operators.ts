import type { AllowedFilterOperation } from '@/app/utils/data-provider';

export const textFilterOperations = [
  'contains',
  'eq',
  'ne',
  'in',
] satisfies AllowedFilterOperation[];

export const DEFAULT_OPERATOR: TextFilterOperation = 'contains';

export type TextFilterOperation = (typeof textFilterOperations)[number];

export const readableFilterOperations: Record<TextFilterOperation, string> = {
  contains: 'Contains',
  eq: 'Equals',
  ne: 'Not equals',
  in: 'In',
};
