import { createContext } from 'react';

import { DEFAULT_TIMEZONE } from './timezones';
import { useInitTimezones } from './use-init-timezones';

import type { Timezone } from '@/shared/utils/timezones/timezones';
import type { FC, PropsWithChildren } from 'react';

export const CurrentTimezoneContext = createContext<{
  timezone: Timezone;
  setTimezone: (newValue: Timezone) => void;
}>({
  timezone: DEFAULT_TIMEZONE,
  setTimezone() {},
});

export const CurrentTimezoneProvider: FC<PropsWithChildren> = ({
  children,
}) => {
  const { timezone, setTimezone } = useInitTimezones();

  return (
    <CurrentTimezoneContext.Provider value={{ timezone, setTimezone }}>
      {children}
    </CurrentTimezoneContext.Provider>
  );
};
