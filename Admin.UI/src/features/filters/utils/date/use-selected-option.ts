import dayjs from 'dayjs';
import { useState } from 'react';

import { filterOptionToOperatorMap } from './date-operators';

import type { DateFilterOption } from './date-operators';
import type { LogicalFilter } from '@refinedev/core';

const DEFAULT_OPTION = 'Equals' as const satisfies DateFilterOption;

const getInitialOption = (logicalFilter: LogicalFilter) => {
  const isSameMinute = (value: [string, string]) => {
    const v1 = dayjs(value[0]);
    const v2 = dayjs(value[1]);

    return v1.isSame(v2, 'minute');
  };

  const isBetween =
    logicalFilter.operator === 'between' &&
    Array.isArray(logicalFilter.value) &&
    !isSameMinute(logicalFilter.value as [string, string]);

  const getOptionByOperator = (): DateFilterOption => {
    for (const option of Object.keys(
      filterOptionToOperatorMap,
    ) as DateFilterOption[]) {
      if (filterOptionToOperatorMap[option] === logicalFilter.operator) {
        if (option === 'Between') {
          continue;
        }

        return option;
      }
    }

    return DEFAULT_OPTION;
  };

  return isBetween ? 'Between' : getOptionByOperator();
};

export const useSelectedOption = (logicalFilter: LogicalFilter) => {
  const [selectedOption, setSelectedOption] = useState<DateFilterOption>(
    getInitialOption(logicalFilter),
  );

  return { selectedOption, setSelectedOption };
};
