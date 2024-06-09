import { IconButton } from '@chakra-ui/react';
import { useModal } from '@refinedev/core';
import { IconEye } from '@tabler/icons-react';

import { JsonViewModal } from './json-view-modal';

import type { FC, ReactElement } from 'react';

export type JsonCellProps = {
  header: string;
  content: ReactElement;
};

export const JsonCell: FC<JsonCellProps> = ({ header, content }) => {
  const { close, show, visible } = useModal();

  return (
    <>
      <IconButton onClick={show} variant="outline" aria-label="show-json">
        <IconEye />
      </IconButton>

      <JsonViewModal
        isOpen={visible}
        onClose={close}
        header={header}
        content={content}
      />
    </>
  );
};
