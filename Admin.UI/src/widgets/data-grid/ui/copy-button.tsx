import { IconButton } from '@chakra-ui/react';
import { IconCheck, IconCopy } from '@tabler/icons-react';
import React, { useEffect, useState } from 'react';
import { useCopyToClipboard } from 'usehooks-ts';

import type { FC, MouseEventHandler } from 'react';

const useCopy = (value: string) => {
  const [, copy] = useCopyToClipboard();

  const [showCopied, setShowCopied] = useState(false);

  const SHOW_DURATION = 1000;

  const handleCopy: MouseEventHandler = (event) => {
    event.stopPropagation();
    event.preventDefault();

    void copy(value).then(() => {
      setShowCopied(true);
    });
  };

  useEffect(() => {
    let timerId: NodeJS.Timeout;

    if (showCopied) {
      timerId = setTimeout(() => {
        setShowCopied(false);
      }, SHOW_DURATION);
    }

    return () => clearTimeout(timerId);
  }, [showCopied]);

  return { handleCopy, showCopied };
};

export const CopyButton: FC<{ value: string }> = ({ value }) => {
  const { handleCopy, showCopied } = useCopy(value);

  return (
    <IconButton onClick={handleCopy} size="sm" aria-label="copy value">
      {showCopied ? <IconCheck size={20} /> : <IconCopy size={20} />}
    </IconButton>
  );
};
