import { HStack, IconButton, Tooltip } from '@chakra-ui/react';
import { IconCheck, IconTrash } from '@tabler/icons-react';
import React from 'react';

import type { FC } from 'react';

interface SelectedFilterControlsProps {
  onReset: VoidFunction;
  onSave: VoidFunction;
}

const SelectedFilterControls: FC<SelectedFilterControlsProps> = ({
  onReset,
  onSave,
}) => {
  return (
    <HStack mt="3" spacing="1" alignSelf="flex-start">
      <Tooltip label="Reset">
        <IconButton
          type="reset"
          aria-label="Reset"
          size="sm"
          colorScheme="red"
          onClick={onReset}
        >
          <IconTrash size={18} />
        </IconButton>
      </Tooltip>
      <Tooltip label="Save">
        <IconButton
          type="submit"
          aria-label="Save"
          size="sm"
          onClick={onSave}
          colorScheme="green"
        >
          <IconCheck size={18} />
        </IconButton>
      </Tooltip>
    </HStack>
  );
};

export default SelectedFilterControls;
