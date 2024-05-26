import { useEffect, useMemo } from 'react';

const findDuplicateIndexes = (values: string[]) => {
  const elementIndexMap = new Map<string, number[]>();

  values.forEach((value, index) => {
    if (!elementIndexMap.has(value)) {
      elementIndexMap.set(value, [index]);
    } else {
      elementIndexMap.get(value)?.push(index);
    }
  });

  const duplicatesIndexes: number[] = [];

  for (const indexes of elementIndexMap.values()) {
    if (indexes.length > 1) {
      duplicatesIndexes.push(...indexes);
    }
  }

  return duplicatesIndexes.length > 0 ? duplicatesIndexes : null;
};

export const useCheckValuesDuplicates = (
  fieldsValues: string[],
  options: {
    onError: (duplicatesIndexes: number[]) => void;
    onNoErrors: VoidFunction;
  },
) => {
  const duplicates = useMemo(() => {
    return findDuplicateIndexes(fieldsValues);
  }, [fieldsValues]);

  useEffect(() => {
    if (duplicates) {
      options.onError(duplicates);
    } else {
      options.onNoErrors();
    }
  }, [duplicates]);
};
