import { useColorMode } from '@chakra-ui/react';
import { ThemeProvider } from '@mui/material';

import { darkTheme, lightTheme } from './mui-theme';

import type { FC, PropsWithChildren } from 'react';

/** Wrapper for MUI components, uses chakra dark/light theme */
export const MuiThemeProvider: FC<PropsWithChildren> = ({ children }) => {
  const { colorMode } = useColorMode();
  const theme = colorMode === 'dark' ? darkTheme : lightTheme;

  return <ThemeProvider theme={theme}>{children}</ThemeProvider>;
};
