import { HStack, Radio, RadioGroup } from '@chakra-ui/react';
import * as React from 'react';

import type { XFilterProps } from '../../utils/types';

export const BooleanFilter: React.FC<XFilterProps> = ({
  logicalFilter,
  onChange,
}) => {
  return (
    <RadioGroup
      value={logicalFilter.value?.toString()}
      onChange={(value) =>
        onChange({
          operator: 'eq',
          field: logicalFilter.field,
          value: value == 'true',
        })
      }
    >
      <HStack gap="6">
        <Radio value="true">True</Radio>
        <Radio value="false">False</Radio>
      </HStack>
    </RadioGroup>
  );
};
