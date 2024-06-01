import dayjs from 'dayjs';

import type { Dayjs } from 'dayjs';

function dateOrNull(v: string) {
  return v ? dayjs(v) : null;
}

export const useValues = (rawValues: unknown): [Dayjs | null, Dayjs | null] => {
  if (Array.isArray(rawValues)) {
    const [v1, v2] = rawValues;

    return [dateOrNull(v1), dateOrNull(v2)];
  }

  return [dateOrNull(rawValues as string), null];
};
