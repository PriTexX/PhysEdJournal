import { useEffect, useState } from 'react';

export const useValue = (externalValue: string | number | null) => {
  const [value, setValue] = useState<string | number | null>(externalValue);

  useEffect(() => {
    setValue(externalValue);
  }, [externalValue]);

  return { value, setValue };
};
