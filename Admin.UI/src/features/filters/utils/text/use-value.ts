import { useEffect, useState } from 'react';

import type { LogicalFilter } from '@refinedev/core';

const SEPARATOR = ' ';

const useInputValue = (logicalFilter: LogicalFilter) => {
  const getDefaultInputValue = () => {
    if (Array.isArray(logicalFilter.value)) {
      return logicalFilter.value.filter(Boolean).join(SEPARATOR);
    }
    return logicalFilter.value ?? null;
  };

  const [inputValue, setInputValue] = useState<string | undefined>(
    getDefaultInputValue(),
  );

  return { inputValue, setInputValue };
};

const useArrayValue = (
  inputValue: string | undefined,
  isInOperator: boolean,
) => {
  const [arrayValue, setArrayValue] = useState<string[]>([]);

  useEffect(() => {
    if (!isInOperator) {
      return;
    }

    setArrayValue(inputValue?.split(SEPARATOR)?.filter(Boolean) ?? []);
  }, [inputValue, isInOperator]);

  return arrayValue;
};

export const useValue = (logicalFilter: LogicalFilter) => {
  const isInOperator = logicalFilter.operator === 'in';

  const { inputValue, setInputValue } = useInputValue(logicalFilter);

  const arrayValue = useArrayValue(inputValue, isInOperator);

  const valueToEmit = isInOperator ? arrayValue : inputValue;

  return { inputValue, setInputValue, valueToEmit, arrayValue };
};
