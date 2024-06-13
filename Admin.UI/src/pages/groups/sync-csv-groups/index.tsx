import { Button } from '@chakra-ui/react';
import { useModal } from '@refinedev/core';

import { SyncGroupsModal } from './sync-groups-modal';

export const SyncGroupsButton = () => {
  const { close, show, visible } = useModal();

  return (
    <>
      <Button variant="outline" onClick={show}>
        Синхронизация групп
      </Button>

      <SyncGroupsModal isOpen={visible} onClose={close} />
    </>
  );
};
