import {
  Button,
  FormControl,
  FormHelperText,
  FormLabel,
  Heading,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Select,
} from '@chakra-ui/react';
import { useDataProvider, useNotification } from '@refinedev/core';
import { AsyncSelect } from 'chakra-react-select';
import ky from 'ky';
import { useState } from 'react';

import type { FC } from 'react';

export type CreateTeacherModalProps = {
  isOpen: boolean;
  onClose: VoidFunction;
};

async function loadStaff(filter: string) {
  const data = await ky
    .get(`staff?filter=${filter}`, {
      prefixUrl: import.meta.env.VITE_API_PATH,
    })
    .json<any>();

  return data.employees.map((e: { fullName: string; guid: string }) => ({
    value: JSON.stringify(e),
    label: e.fullName,
  }));
}

export const CreateTeacherModal: FC<CreateTeacherModalProps> = ({
  isOpen,
  onClose,
}) => {
  const { open } = useNotification();

  const getDataProvider = useDataProvider();
  const dataProvider = getDataProvider();

  const [teacher, setTeacher] = useState('');
  const [permission, setPermission] = useState('DefaultAccess');

  const createTeacher = async () => {
    console.log(teacher);
    const { fullName, guid } = JSON.parse(teacher);

    try {
      await dataProvider.create({
        resource: 'teachers',
        variables: { fullName, teacherGuid: guid, permissions: permission },
      });

      open?.({
        type: 'success',
        message: 'Успех',
        description: 'Преподаватель добавлен',
      });
    } catch {
      open?.({
        type: 'error',
        message: 'Ошибка',
        description: 'Неизвестная ошибка при добавлении преподавателя',
      });
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="3xl">
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>
          <Heading size="lg">Добавление преподавателя</Heading>
        </ModalHeader>
        <ModalCloseButton />
        <ModalBody mt={6}>
          <FormControl>
            <FormLabel>ФИО</FormLabel>
            <AsyncSelect
              onChange={(v) => setTeacher((v as any).value)}
              loadOptions={loadStaff}
            />
            <FormHelperText>Начните вводить ФИО преподавателя</FormHelperText>
          </FormControl>
          <FormControl mt={3}>
            <FormLabel>Права</FormLabel>
            <Select
              onChange={(v) => setPermission(v.target.value)}
              defaultValue={'DefaultAccess'}
            >
              <option key="DefaultAccess" value="DefaultAccess">
                Обычный
              </option>
              <option key="OnlineCourseAccess" value="OnlineCourseAccess">
                СДО
              </option>
              <option key="SecretaryAccess" value="SecretaryAccess">
                Секретарь
              </option>
            </Select>
          </FormControl>
        </ModalBody>
        <ModalFooter>
          <Button colorScheme="blue" onClick={createTeacher}>
            Добавить
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
};
