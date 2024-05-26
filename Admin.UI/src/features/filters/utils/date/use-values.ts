import { fromISO } from '@/shared/utils/dates';

import type { Dayjs } from 'dayjs';

export const useValues = (rawValues: unknown): [Dayjs | null, Dayjs | null] => {
  if (Array.isArray(rawValues)) {
    const [v1, v2] = rawValues;
    return [fromISO((v1 as string) ?? null), fromISO((v2 as string) ?? null)];
  }

  return [fromISO((rawValues as string) ?? null), null];
};
