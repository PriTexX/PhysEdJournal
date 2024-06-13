import { Box } from '@chakra-ui/react';

import type { ChangeEvent } from 'react';

export const FileUploader = ({
  onFileSelect,
}: {
  onFileSelect: (f: File) => Promise<void>;
}) => {
  const handleFileInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files && e.target.files[0];

    if (file) {
      onFileSelect(file);
    }
  };

  return (
    <Box>
      <input type="file" onChange={handleFileInputChange} />
    </Box>
  );
};
