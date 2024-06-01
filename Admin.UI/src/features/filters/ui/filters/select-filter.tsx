import { Select, VStack } from '@chakra-ui/react';

import type { XFilterProps } from '../../utils/types';

export const SelectFilter: React.FC<
  XFilterProps<{ value: string; label?: string }[]>
> = ({ logicalFilter, onChange, additionalInfo }) => {
  return (
    <VStack w="full">
      <Select
        value={logicalFilter.value ?? undefined}
        onChange={(e) => {
          const isEmpty = !e.target.value;

          onChange({
            ...logicalFilter,
            value: isEmpty ? undefined : e.target.value,
            operator: 'eq',
          });
        }}
      >
        <option value="">Все</option>

        {additionalInfo?.map((o) => {
          return (
            <option value={o.value} key={o.value}>
              {o.label ?? o.value}
            </option>
          );
        })}
      </Select>
    </VStack>
  );
};
