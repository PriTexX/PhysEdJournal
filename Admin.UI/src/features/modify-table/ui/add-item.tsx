import { HStack, IconButton } from '@chakra-ui/react';
import { IconPlus } from '@tabler/icons-react';

import type { FC } from 'react';

interface AddItemProps {
  onAdd: VoidFunction;
}

export const AddItem: FC<AddItemProps> = ({ onAdd }) => {
  return (
    <HStack w="100%" justifyContent="flex-end">
      <IconButton onClick={onAdd} aria-label="remove-cell">
        <IconPlus size={20} />
      </IconButton>
    </HStack>
  );
};
