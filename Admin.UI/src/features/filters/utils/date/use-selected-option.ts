import { useState } from 'react';

import { filterOptionToOperatorMap } from './date-operators';

import type { DateFilterOption } from './date-operators';
import type { LogicalFilter } from '@refinedev/core';

const DEFAULT_OPTION = 'Соответствует' as const satisfies DateFilterOption;

const getInitialOption = (logicalFilter: LogicalFilter) => {
  for (const option of Object.keys(
    filterOptionToOperatorMap,
  ) as DateFilterOption[]) {
    if (filterOptionToOperatorMap[option] === logicalFilter.operator) {
      return option;
    }
  }

  return DEFAULT_OPTION;
};

export const useSelectedOption = (logicalFilter: LogicalFilter) => {
  const [selectedOption, setSelectedOption] = useState<DateFilterOption>(
    getInitialOption(logicalFilter),
  );

  return { selectedOption, setSelectedOption };
};
