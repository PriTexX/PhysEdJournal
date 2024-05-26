import { IconButton, Tooltip } from '@chakra-ui/react';
import { useModal } from '@refinedev/core';
import { IconWorld } from '@tabler/icons-react';
import React from 'react';

import { useCurrentTimezone } from '@/shared/utils/timezones/use-current-timezone';

import { DEFAULT_TIMEZONE } from '../utils/current-timezone-provider/timezones';
import { EditTimezoneModal } from './edit-timezone-modal';

export const TimezoneButton = () => {
  const {
    close: closeTimezoneModal,
    show: openTimezoneModal,
    visible: isTimezoneModalOpen,
  } = useModal();

  const { timezone } = useCurrentTimezone();

  const isDefaultTimezone = timezone === DEFAULT_TIMEZONE;

  return (
    <>
      <EditTimezoneModal
        isOpen={isTimezoneModalOpen}
        onClose={closeTimezoneModal}
      />

      <Tooltip label="Edit timezone">
        <IconButton
          onClick={() => openTimezoneModal()}
          variant={isDefaultTimezone ? 'ghost' : 'solid'}
          aria-label="Edit current timezone"
        >
          <IconWorld size={24} />
        </IconButton>
      </Tooltip>
    </>
  );
};
