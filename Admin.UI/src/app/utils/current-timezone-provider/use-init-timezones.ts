import { useLocalStorage } from 'usehooks-ts';

import { TIMEZONES } from '@/shared/utils/timezones/timezones';

import { DEFAULT_TIMEZONE } from './timezones';

import type { Timezone } from '@/shared/utils/timezones/timezones';

const STORAGE_KEY = 'currentTimezone';

export const useInitTimezones = () => {
  const [timezone, setTimezone] = useLocalStorage(
    STORAGE_KEY,
    DEFAULT_TIMEZONE,
    {
      serializer(value) {
        return value;
      },

      deserializer(value): Timezone {
        if (TIMEZONES.includes(value as never)) {
          return value as Timezone;
        }

        return DEFAULT_TIMEZONE;
      },
    },
  );

  return {
    timezone,
    setTimezone,
  };
};
