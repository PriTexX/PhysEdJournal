import {
  Button,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Text,
  useToast,
} from '@chakra-ui/react';
import { useCallback, useEffect, useState } from 'react';

import { useDownloadCsv } from '../utils/use-dowload-csv';
import { useFetchList } from '../utils/use-load-data';

import type { BaseRecord, CrudFilters, CrudSorting } from '@refinedev/core';
import type { ColumnDef } from '@tanstack/react-table';

const useErrorNotification = (show: boolean) => {
  const toast = useToast();

  useEffect(() => {
    if (!show) {
      return;
    }

    toast({
      title: 'Что-то пошло не так',
      description:
        'Размер данных возможно слишком велик, попробуйте уменьшить его путем использования фильтров',
      status: 'error',
      duration: 5000,
      isClosable: true,
    });
  }, [show, toast]);
};

const useAutoCloseModal = (close: boolean, handleClose: VoidFunction) => {
  useEffect(() => {
    if (close) {
      handleClose();
    }
  }, [close, handleClose]);
};

export interface ExportCsvModalProps<D extends BaseRecord> {
  isOpen: boolean;
  onClose: VoidFunction;
  rowsAmount: number;
  allColumns: ColumnDef<D, any>[];
  defaultColumnsKeys: (keyof D)[];
  filters: CrudFilters;
  sorters?: CrudSorting;
  customCsvMapper?: (items: D, teachersMap: Map<string, string>) => object;
}

export const ExportCsvModal = <D extends BaseRecord>({
  isOpen,
  onClose,
  rowsAmount,
  allColumns,
  defaultColumnsKeys,
  customCsvMapper,
  filters,
  sorters,
}: ExportCsvModalProps<D>) => {
  const [loadingEnabled, setLoadingEnabled] = useState(false);

  const { data, isFetchError, isFetching, teachersMap } = useFetchList({
    enabled: loadingEnabled,
    amountToLoad: rowsAmount,
    filters,
    sorters,
  });

  const { status: downloadStatus, downloadCsv } = useDownloadCsv({
    enabled: loadingEnabled,
    allColumns,
    defaultColumnsKeys,
    customCsvMapper,
  });

  const handleClose = useCallback(() => {
    onClose();
    setLoadingEnabled(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useAutoCloseModal(downloadStatus === 'success', handleClose);

  useErrorNotification(downloadStatus === 'error' || isFetchError);

  useEffect(() => {
    const alreadyDownloaded = downloadStatus === 'success';

    if (data && !alreadyDownloaded) {
      downloadCsv(data as object[], teachersMap);
    }
  }, [data, downloadCsv, downloadStatus, teachersMap]);

  const exportCsv = () => {
    setLoadingEnabled(true);
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose}>
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>Экспорт в CSV</ModalHeader>
        <ModalCloseButton />
        <ModalBody>
          <Text>
            CSV будет содержать <b>{rowsAmount}</b> строк
          </Text>
        </ModalBody>

        <ModalFooter>
          <Button isLoading={isFetching} colorScheme="blue" onClick={exportCsv}>
            Загрузить
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
};
