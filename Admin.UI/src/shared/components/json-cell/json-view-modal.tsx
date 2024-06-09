import {
  Box,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalHeader,
  ModalOverlay,
} from '@chakra-ui/react';

import type { FC, ReactElement } from 'react';

export type JsonViewModalProps = {
  isOpen: boolean;
  onClose: VoidFunction;
  header: string;
  content: ReactElement;
};

export const JsonViewModal: FC<JsonViewModalProps> = ({
  isOpen,
  onClose,
  header,
  content,
}) => {
  return (
    <Modal isOpen={isOpen} onClose={onClose} size="4xl">
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>{header}</ModalHeader>
        <ModalCloseButton />
        <ModalBody>{content}</ModalBody>
      </ModalContent>
    </Modal>
  );
};
