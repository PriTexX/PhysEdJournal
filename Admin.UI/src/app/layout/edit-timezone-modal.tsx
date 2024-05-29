import {
  Button,
  Link,
  LinkBox,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Select,
} from '@chakra-ui/react';
import { useForm } from 'react-hook-form';

import { TIMEZONES } from '@/shared/utils/timezones/timezones';
import { useCurrentTimezone } from '@/shared/utils/timezones/use-current-timezone';

import { DEFAULT_TIMEZONE } from '../utils/current-timezone-provider/timezones';

import type { FC } from 'react';

export interface EditTimezoneModalProps {
  isOpen: boolean;
  onClose: VoidFunction;
}

export const EditTimezoneModal: FC<EditTimezoneModalProps> = ({
  isOpen,
  onClose,
}) => {
  const { timezone, setTimezone } = useCurrentTimezone();

  const { register, handleSubmit, setValue } = useForm({
    defaultValues: { timezone },
  });

  return (
    <Modal isOpen={isOpen} onClose={onClose}>
      <ModalOverlay />
      <ModalContent
        as="form"
        onSubmit={handleSubmit(({ timezone }) => {
          setTimezone(timezone);
          onClose();
        })}
      >
        <ModalHeader>Edit timezone</ModalHeader>
        <ModalCloseButton />
        <ModalBody>
          <p>
            The selected timezone will be applied to the dates displayed and
            those being modified, as well as used in date filters.
          </p>
          <Select {...register('timezone')} mt={4}>
            {TIMEZONES.map((o) => (
              <option key={o} value={o}>
                GMT{o}
              </option>
            ))}
          </Select>

          <LinkBox mt={1}>
            <Link
              textDecoration="underline"
              onClick={() => {
                setValue('timezone', DEFAULT_TIMEZONE);
              }}
              _dark={{ color: 'blue.200' }}
              _light={{ color: 'blue.700' }}
            >
              Set to default
            </Link>
          </LinkBox>
        </ModalBody>

        <ModalFooter>
          <Button type="submit" colorScheme="blue">
            Save
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
};
