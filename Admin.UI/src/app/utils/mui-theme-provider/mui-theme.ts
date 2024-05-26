import { createTheme } from '@mui/material';

export const muiTheme = createTheme();

const font =
  "apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif";

const lightTheme = createTheme({
  palette: {
    mode: 'light',
  },
  typography: {
    fontFamily: font,
  },
});

const darkTheme = createTheme({
  palette: {
    mode: 'dark',
  },
  typography: {
    fontFamily: font,
  },
});

export { lightTheme, darkTheme };
