import { Button } from '@chakra-ui/react';
import { useModal } from '@refinedev/core';

import { CreateTeacherModal } from './create-teacher-modal';

export const CreateTeacherButton = () => {
  const { close, show, visible } = useModal();

  return (
    <>
      <Button variant="outline" onClick={show}>
        Добавить преподавателя
      </Button>

      <CreateTeacherModal isOpen={visible} onClose={close} />
    </>
  );
};
