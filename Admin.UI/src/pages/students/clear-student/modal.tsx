import {
  Button,
  Heading,
  Modal,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Text,
} from '@chakra-ui/react';
import { useNotification, useResource, useShow } from '@refinedev/core';
import { useMutation } from '@tanstack/react-query';
import ky from 'ky';

import type { FC } from 'react';

export const ClearStudentModal: FC<{
  isOpen: boolean;
  onClose: VoidFunction;
}> = ({ isOpen, onClose }) => {
  const { open } = useNotification();

  const {
    queryResult: { data },
  } = useShow();

  const { isPending, mutate } = useMutation({
    mutationKey: ['clear-student'],
    mutationFn: async () => {
      if (!data?.data) {
        return;
      }

      await ky.post(`student/${data.data.studentGuid}`, {
        prefixUrl: import.meta.env.VITE_API_PATH,
      });
    },
  });

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="3xl">
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>
          <Heading size="lg">Обнуление студента</Heading>
          <Text fontSize="md" mt={3}>
            Все баллы студента и его долг обнуляться и студент будет переведен в
            текущий семестр.
          </Text>
        </ModalHeader>
        <ModalCloseButton />
        <ModalFooter>
          <Button
            isLoading={isPending}
            colorScheme="blue"
            onClick={() => mutate()}
          >
            Обнулить
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
};
