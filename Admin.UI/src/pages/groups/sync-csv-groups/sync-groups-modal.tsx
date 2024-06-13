import {
  Box,
  Button,
  Heading,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Table,
  Tbody,
  Td,
  Text,
  Th,
  Thead,
  Tr,
  VStack,
} from '@chakra-ui/react';
import { useDataProvider, useNotification } from '@refinedev/core';
import { useMutation } from '@tanstack/react-query';
import { csv2json } from 'json-2-csv';
import { useCallback, useState } from 'react';

import { Pagination } from '@/features/pagination';
import { useTeachersMap } from '@/features/use-teachers-map';

import { FileUploader } from './file-uploader';

import type { FC } from 'react';

type SyncGroupData = {
  group: string;
  visitValue: number;
  curator: string | null;
};

function extractCsvGroupFields(csvGroup: any): SyncGroupData | null {
  const group = csvGroup['Группа'] as string;
  const visitValue = csvGroup['Стоимость посещения'] as number;
  const curator = csvGroup['Куратор'] as string;

  if (
    group === undefined ||
    visitValue === undefined ||
    curator === undefined
  ) {
    return null;
  }

  return { group, visitValue, curator: curator != '' ? curator : null };
}

export type SyncGroupsModalProps = {
  isOpen: boolean;
  onClose: VoidFunction;
};

export const SyncGroupsModal: FC<SyncGroupsModalProps> = ({
  isOpen,
  onClose,
}) => {
  const PAGE_SIZE = 20;

  const { teacherNamesMap, teacherGuidsMap } = useTeachersMap();

  const { open } = useNotification();

  const getDataProvider = useDataProvider();
  const dataProvider = getDataProvider();

  const { mutate, isPending } = useMutation({
    mutationKey: ['group-sync'],
    mutationFn: async (groups: SyncGroupData[]) => {
      return Promise.all(
        groups.map((g) =>
          dataProvider.update({
            resource: 'groups',
            id: g.group,
            variables: { visitValue: g.visitValue, curatorGuid: g.curator },
          }),
        ),
      );
    },
    onSuccess() {
      open?.({
        type: 'success',
        message: 'Успех',
        description: 'Группы успешно синхронизированны',
      });
    },
    onError() {
      open?.({
        type: 'error',
        message: 'Ошибка',
        description: 'Неизвестная ошибка при синхронизации групп',
      });
    },
  });

  const [syncData, setSyncData] = useState<SyncGroupData[]>([]);
  const [currentPage, setCurrentPage] = useState(1);

  const pageCount = Math.ceil(syncData.length / PAGE_SIZE);

  const paginatedData = syncData.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE,
  );

  const onFileSelect = useCallback(
    async (file: File) => {
      const csv = await file.text();

      const data = csv2json(csv) as any[];

      const parsedData: SyncGroupData[] = [];

      for (const el of data) {
        const parsedEl = extractCsvGroupFields(el);

        if (parsedEl) {
          const teacherGuid =
            parsedEl.curator && (teacherGuidsMap.get(parsedEl.curator) ?? null);

          parsedData.push({ ...parsedEl, curator: teacherGuid });
        }
      }

      setSyncData(parsedData);
    },
    [teacherGuidsMap],
  );

  const syncGroups = useCallback(() => {
    mutate(syncData);
  }, [syncData]);

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="3xl">
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>
          <Heading size="lg">Синхронизация групп с CSV</Heading>
          <Text fontSize="md" mt={3}>
            Для синхронизации групп загрузите файл CSV в правильном формате.
            Формат можно посмотреть экспортировав таблицу 'Группы'.
          </Text>
        </ModalHeader>
        <ModalCloseButton />
        <ModalBody mt={6}>
          <VStack spacing={3}>
            {paginatedData.length > 0 ? (
              <Table>
                <Thead>
                  <Tr>
                    <Th textAlign="center">Группа</Th>
                    <Th textAlign="center">Стоимость посещения</Th>
                    <Th textAlign="center">Куратор</Th>
                  </Tr>
                </Thead>
                <Tbody>
                  {paginatedData.map((d) => (
                    <Tr key={d.group}>
                      <Td textAlign="center">{d.group}</Td>
                      <Td textAlign="center">{d.visitValue}</Td>
                      <Td textAlign="center">
                        {(d.curator && teacherNamesMap.get(d.curator)) ?? '-'}
                      </Td>
                    </Tr>
                  ))}
                </Tbody>
              </Table>
            ) : (
              <Text>Выберите файл с CSV</Text>
            )}
          </VStack>
          {pageCount > 1 && (
            <Pagination
              current={currentPage}
              pageCount={pageCount}
              setCurrent={setCurrentPage}
              justifyContent="center"
            />
          )}
        </ModalBody>
        <ModalFooter>
          <FileUploader onFileSelect={onFileSelect} />
          <Button isLoading={isPending} colorScheme="blue" onClick={syncGroups}>
            Начать синхронизацию
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
};
