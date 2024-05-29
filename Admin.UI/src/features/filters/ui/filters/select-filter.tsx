import { Select, VStack } from '@chakra-ui/react';

import type { XFilterProps } from '../../utils/types';

export const SelectFilter: React.FC<XFilterProps<string[]>> = ({
  logicalFilter,
  onChange,
  additionalInfo,
}) => {
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
        <option value="">all</option>

        {additionalInfo?.map((o) => {
          if (typeof o === 'string') {
            return (
              <option value={o} key={o}>
                {o}
              </option>
            );
          }
        })}
      </Select>
    </VStack>
  );
};
