import { HStack, Radio, RadioGroup } from '@chakra-ui/react';
import * as React from 'react';

import type { XFilterProps } from '../../utils/types';
import type { LogicalFilter } from '@refinedev/core';

const getValueOfNullFilter = (logicalFilter: LogicalFilter) => {
  if (logicalFilter.operator === 'null') {
    return 'true';
  }

  if (logicalFilter.operator === 'nnull') {
    return 'false';
  }
};

export const NullFilter: React.FC<XFilterProps> = ({
  logicalFilter,
  onChange,
}) => {
  const value = getValueOfNullFilter(logicalFilter);

  return (
    <RadioGroup
      value={value ?? ''}
      onChange={(value) =>
        onChange({
          operator: value === 'true' ? 'null' : 'nnull',
          field: logicalFilter.field,
          value: '',
        })
      }
    >
      <HStack gap="6">
        <Radio value="true">Null</Radio>
        <Radio value="false">Not null</Radio>
      </HStack>
    </RadioGroup>
  );
};
