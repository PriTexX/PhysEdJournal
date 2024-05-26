import dayjs from 'dayjs';

import type { Dayjs } from 'dayjs';

export const fromISO = (isoString: string | null | undefined) => {
  if (!isoString) {
    return null;
  }

  const date = dayjs(isoString);
  if (date.isValid()) {
    return date;
  }

  return null;
};

export const toISO = (date: Dayjs | null) => date?.toISOString();
