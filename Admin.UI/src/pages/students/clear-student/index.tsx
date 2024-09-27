import { Button } from '@chakra-ui/react';
import { useModal } from '@refinedev/core';

import { ClearStudentModal } from './modal';

export const ClearStudentButton = () => {
  const { close, show, visible } = useModal();

  return (
    <>
      <Button variant="outline" onClick={show}>
        Обнулить студента
      </Button>

      <ClearStudentModal isOpen={visible} onClose={close} />
    </>
  );
};
