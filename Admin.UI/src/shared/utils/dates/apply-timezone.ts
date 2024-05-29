import type { Timezone } from '../timezones/timezones';
import type { Dayjs } from 'dayjs';

const timezoneRegex = /^([+-])(0\d|1[0-4]):(00|30|45)$/;

const parseTimezone = (timezone: Timezone) => {
  const match = timezone.match(timezoneRegex);

  if (!match) {
    return null;
  }

  const [, sign, hours, minutes] = match;

  return {
    sign: sign as '+' | '-',
    hours: Number(hours),
    minutes: Number(minutes),
  };
};

export const applyTimezone = (date: Dayjs | null, timezone: Timezone) => {
  const parsedTimezone = parseTimezone(timezone);

  if (!date || !parsedTimezone) {
    return null;
  }

  const { sign, hours, minutes } = parsedTimezone;

  if (sign === '+') {
    return date.add(hours, 'hours').add(minutes, 'minutes');
  }

  return date.subtract(hours, 'hours').subtract(minutes, 'minutes');
};

export const clearTimezone = (date: Dayjs | null, timezone: Timezone) => {
  const parsedTimezone = parseTimezone(timezone);

  if (!date || !parsedTimezone) {
    return null;
  }

  const { sign, hours, minutes } = parsedTimezone;

  if (sign === '+') {
    return date.subtract(hours, 'hours').subtract(minutes, 'minutes');
  }

  return date.add(hours, 'hours').add(minutes, 'minutes');
};
