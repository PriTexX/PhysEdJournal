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
import {
  CompetitionCreatePage,
  CompetitionListPage,
} from './pages/competitions';
import { GroupListPage } from './pages/groups';
import { LoginPage } from './pages/login';
import { StudentEditPage, StudentListPage } from './pages/students';

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
    list: '/students',
    edit: '/students/:id',
    meta: {
      label: 'Студенты',
    },
  },
  {
    name: 'competition',
    list: '/competitions',
    create: '/competitions/create',
    meta: {
      canDelete: true,
      label: 'Соревнования',
    },
  },
  {
    name: 'group',
    list: '/groups',
    meta: {
      label: 'Группы',
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
                                text="Журнал | Админ"
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
                      <Route path="/students">
                        <Route index element={<StudentListPage />} />
                        <Route path=":id" element={<StudentEditPage />} />
                      </Route>
                      <Route path="/competitions">
                        <Route index element={<CompetitionListPage />} />
                        <Route
                          path="create"
                          element={<CompetitionCreatePage />}
                        />
                      </Route>
                      <Route path="/groups">
                        <Route index element={<GroupListPage />} />
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
                      <Route path="/login" element={<LoginPage />} />
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
