import {
  NumberDecrementStepper,
  NumberIncrementStepper,
  NumberInput,
  NumberInputField,
  NumberInputStepper,
  Select,
  VStack,
} from '@chakra-ui/react';

import { allowedFilterOperations } from '@/app/utils/data-provider';

import type { XFilterProps } from '../../utils/types';
import type { AllowedFilterOperation } from '@/app/utils/data-provider';

const numberFilterOperators = [
  'eq',
  'ne',
  'gt',
  'gte',
  'lt',
  'lte',
] satisfies AllowedFilterOperation[];

const DEFAULT_OPERATOR: (typeof numberFilterOperators)[number] = 'eq';

type NumberFilterOperation = (typeof numberFilterOperators)[number];

const readableFilterOperations: Record<NumberFilterOperation, string> = {
  eq: 'Equals',
  ne: 'Not equals',
  gt: 'Greater than',
  gte: 'Greater than or equals',
  lt: 'Lesser than',
  lte: 'Lesser than or equals',
};

export const NumberFilter: React.FC<XFilterProps> = ({
  logicalFilter,
  onChange,
}) => {
  return (
    <VStack w="full">
      <Select
        flexShrink={0}
        size="sm"
        value={logicalFilter.operator}
        onChange={(e) =>
          onChange({
            ...logicalFilter,
            operator: e.target.value as NumberFilterOperation,
          })
        }
      >
        {numberFilterOperators.map((filter) => (
          <option key={filter} value={filter}>
            {readableFilterOperations[filter]}
          </option>
        ))}
      </Select>
      <NumberInput
        size="sm"
        borderRadius="md"
        w="100%"
        value={logicalFilter.value ?? ''}
        onChange={(e) => {
          const numericValue = Number(e);
          onChange({
            ...logicalFilter,
            value: Number.isNaN(numericValue) ? numericValue : e,
            operator: allowedFilterOperations.includes(
              logicalFilter.operator as never,
            )
              ? logicalFilter.operator
              : DEFAULT_OPERATOR,
          });
        }}
      >
        <NumberInputField borderRadius="md" />
        <NumberInputStepper>
          <NumberIncrementStepper />
          <NumberDecrementStepper />
        </NumberInputStepper>
      </NumberInput>
    </VStack>
  );
};
