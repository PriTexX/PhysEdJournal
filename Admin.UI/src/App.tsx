import { Center, ChakraProvider, extendTheme, Spinner } from '@chakra-ui/react';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import {
  ErrorComponent,
  RefineThemes,
  useNotificationProvider,
} from '@refinedev/chakra-ui';
import { Authenticated, Refine } from '@refinedev/core';
import { RefineKbar, RefineKbarProvider } from '@refinedev/kbar';
import routerBindings, {
  CatchAllNavigate,
  DocumentTitleHandler,
  NavigateToResource,
  UnsavedChangesNotifier,
} from '@refinedev/react-router-v6';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import { BrowserRouter, Outlet, Route, Routes } from 'react-router-dom';

import { getAuthProvider } from '@/app/utils/auth-provider';
import { getDataProvider } from '@/app/utils/data-provider';

// import AppIcon from '@/assets/app-icon.svg';

import { ThemedLayoutV2 } from './app/layout/refine-layout';
import { Header } from './app/layout/refine-layout/header';
import { ThemedTitleV2 } from './app/layout/refine-layout/title';
import { CurrentTimezoneProvider } from './app/utils/current-timezone-provider/current-timezone-provider';
import { handleDocumentTitle } from './app/utils/handle-document-title';
import { TableSchemasProvider } from './features/modify-table/utils/table-schemas-provider';
import { Login } from './pages/login';
import { StudentEdit } from './pages/students/edit';
import { StudentList } from './pages/students/list';

import type { ResourceProps } from '@refinedev/core';

dayjs.extend(utc);

const dataProvider = getDataProvider(import.meta.env.VITE_API_PATH);
const authProvider = getAuthProvider(import.meta.env.VITE_API_PATH);

const theme = extendTheme(RefineThemes.Blue, {
  semanticTokens: {
    colors: {
      tableHover: {
        _dark: 'blackAlpha.300',
        default: 'blackAlpha.50',
      },
    },
  },
});

const resources: ResourceProps[] = [
  {
    name: 'student',
    list: '/student',
    edit: '/student/:id',
    meta: {
      label: 'Студенты',
    },
  },
];

function App() {
  return (
    <BrowserRouter>
      <RefineKbarProvider>
        <ChakraProvider theme={theme}>
          <Refine
            dataProvider={dataProvider}
            notificationProvider={useNotificationProvider}
            routerProvider={routerBindings}
            authProvider={authProvider}
            resources={resources}
            options={{
              syncWithLocation: true,
              warnWhenUnsavedChanges: true,
              useNewQueryKeys: true,
              disableTelemetry: true,
            }}
          >
            <LocalizationProvider
              dateAdapter={AdapterDayjs}
              adapterLocale="en-gb"
            >
              <CurrentTimezoneProvider>
                <TableSchemasProvider>
                  <Routes>
                    <Route
                      element={
                        <Authenticated
                          key="authenticated-inner"
                          fallback={<CatchAllNavigate to="/login" />}
                          loading={
                            <Center h="100vh">
                              <Spinner />
                            </Center>
                          }
                        >
                          <ThemedLayoutV2
                            Header={() => <Header sticky />}
                            Title={({ collapsed }) => (
                              <ThemedTitleV2
                                collapsed={collapsed}
                                text="PhysEdJournal Admin"
                                // icon={<AppIcon />}
                              />
                            )}
                          >
                            <Outlet />
                          </ThemedLayoutV2>
                        </Authenticated>
                      }
                    >
                      <Route
                        index
                        element={<NavigateToResource resource="student" />}
                      />
                      <Route path="/student">
                        <Route index element={<StudentList />} />
                        <Route path=":id" element={<StudentEdit />} />
                      </Route>
                      <Route path="*" element={<ErrorComponent />} />
                    </Route>
                    <Route
                      element={
                        <Authenticated
                          key="authenticated-outer"
                          fallback={<Outlet />}
                        >
                          <NavigateToResource />
                        </Authenticated>
                      }
                    >
                      <Route path="/login" element={<Login />} />
                    </Route>
                  </Routes>
                </TableSchemasProvider>
              </CurrentTimezoneProvider>
            </LocalizationProvider>

            <RefineKbar />
            <UnsavedChangesNotifier />
            <DocumentTitleHandler handler={handleDocumentTitle} />
          </Refine>
        </ChakraProvider>
      </RefineKbarProvider>
    </BrowserRouter>
  );
}

export default App;
