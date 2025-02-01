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
import { GroupEditPage, GroupListPage } from './pages/groups';
import { LoginPage } from './pages/login';
import { PointsListPage } from './pages/points';
import { SemesterCreatePage, SemesterListPage } from './pages/semesters';
import { StandardsListPage } from './pages/standards';
import { StudentEditPage, StudentListPage } from './pages/students';
import { TeacherEditPage, TeacherListPage } from './pages/teachers';
import { VisitsListPage } from './pages/visits';

import 'dayjs/locale/en-gb';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

import { ArchivedStudentListPage } from './pages/archived-students';

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
    name: 'students',
    list: '/students',
    edit: '/students/:id',
    meta: {
      label: 'Студенты',
    },
  },
  {
    name: 'competitions',
    list: '/competitions',
    create: '/competitions/create',
    meta: {
      canDelete: true,
      label: 'Соревнования',
    },
  },
  {
    name: 'groups',
    list: '/groups',
    edit: '/groups/:id',
    meta: {
      label: 'Группы',
    },
  },
  {
    name: 'points',
    list: '/points',
    meta: {
      label: 'Доп. баллы',
    },
  },
  {
    name: 'semesters',
    list: '/semesters',
    create: '/semesters/create',
    meta: {
      label: 'Семестры',
    },
  },
  {
    name: 'standards',
    list: '/standards',
    meta: {
      label: 'Нормативы',
    },
  },
  {
    name: 'teachers',
    list: '/teachers',
    edit: '/teachers/:id',
    meta: {
      label: 'Преподаватели',
      canDelete: true,
    },
  },
  {
    name: 'visits',
    list: '/visits',
    meta: {
      label: 'Посещения',
    },
  },
  {
    name: 'archived-students',
    list: '/archived-students',
    meta: {
      label: 'Архив',
    },
  },
];

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 0,
    },
  },
});

function App() {
  return (
    <BrowserRouter basename="/physedjournal/admin">
      <RefineKbarProvider>
        <ChakraProvider theme={theme}>
          <QueryClientProvider client={queryClient}>
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
                reactQuery: {
                  clientConfig: {
                    defaultOptions: {
                      queries: {
                        refetchOnWindowFocus: false,
                        keepPreviousData: true,
                        retry: 0,
                      },
                    },
                  },
                },
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
                          <Route path=":id" element={<GroupEditPage />} />
                        </Route>
                        <Route path="/points">
                          <Route index element={<PointsListPage />} />
                        </Route>
                        <Route path="/semesters">
                          <Route index element={<SemesterListPage />} />
                          <Route
                            path="create"
                            element={<SemesterCreatePage />}
                          />
                        </Route>
                        <Route path="/standards">
                          <Route index element={<StandardsListPage />} />
                        </Route>
                        <Route path="/teachers">
                          <Route index element={<TeacherListPage />} />
                          <Route path=":id" element={<TeacherEditPage />} />
                        </Route>
                        <Route path="/visits">
                          <Route index element={<VisitsListPage />} />
                        </Route>
                        <Route path="/archived-students">
                          <Route index element={<ArchivedStudentListPage />} />
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
          </QueryClientProvider>
        </ChakraProvider>
      </RefineKbarProvider>
    </BrowserRouter>
  );
}

export default App;
