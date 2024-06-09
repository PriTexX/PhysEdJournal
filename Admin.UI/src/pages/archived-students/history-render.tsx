import { Table, Tbody, Td, Th, Thead, Tr } from '@chakra-ui/react';
import dayjs from 'dayjs';

import { TeacherCell } from '@/shared/components/teacher-cell';

import { workTypeRus } from '../points/types';
import { standardTypeRus } from '../standards/types';

import type { PointsHistory, StandardsHistory, VisitsHistory } from './types';
import type { FC } from 'react';

type HistoryRendererProps<T> = {
  data: T[];
  headers: string[];
  renderItem: FC<{ data: T }>;
};

function HistoryRenderer<T>({
  data,
  headers,
  renderItem: RenderItem,
}: HistoryRendererProps<T>) {
  return (
    <Table>
      <Thead>
        <Tr>
          {headers.map((h, i) => (
            <Th key={i}>{h}</Th>
          ))}
        </Tr>
      </Thead>
      <Tbody>
        {data.map((d, i) => (
          <Tr key={i}>
            <RenderItem data={d} />
          </Tr>
        ))}
      </Tbody>
    </Table>
  );
}

const VisitRender: FC<{ data: VisitsHistory }> = ({ data }) => {
  return (
    <>
      <Td>{dayjs(data.date).format('DD-MM-YYYY')}</Td>
      <Td>{data.points}</Td>
      <Td>
        <TeacherCell guid={data.teacherGuid} />
      </Td>
    </>
  );
};

export const VisitsHistoryRender: FC<{ data: VisitsHistory[] }> = ({
  data,
}) => {
  return (
    <HistoryRenderer
      data={data}
      headers={['Дата', 'Баллы', 'Преподаватель']}
      renderItem={VisitRender}
    />
  );
};

const StandardRender: FC<{ data: StandardsHistory }> = ({ data }) => {
  return (
    <>
      <Td>{dayjs(data.date).format('DD-MM-YYYY')}</Td>
      <Td>{data.points}</Td>
      <Td>{standardTypeRus[data.standardType]}</Td>
      <Td>
        <TeacherCell guid={data.teacherGuid} />
      </Td>
      <Td>{data.comment ?? ''}</Td>
    </>
  );
};

export const StandarsHistoryRender: FC<{ data: StandardsHistory[] }> = ({
  data,
}) => {
  return (
    <HistoryRenderer
      data={data}
      headers={['Дата', 'Баллы', 'Норматив', 'Преподаватель', 'Комментарий']}
      renderItem={StandardRender}
    />
  );
};

const PointRender: FC<{ data: PointsHistory }> = ({ data }) => {
  return (
    <>
      <Td>{dayjs(data.date).format('DD-MM-YYYY')}</Td>
      <Td>{data.points}</Td>
      <Td>{workTypeRus[data.workType]}</Td>
      <Td>
        <TeacherCell guid={data.teacherGuid} />
      </Td>
      <Td>{data.comment ?? ''}</Td>
    </>
  );
};

export const PointsHistoryRender: FC<{ data: PointsHistory[] }> = ({
  data,
}) => {
  return (
    <HistoryRenderer
      data={data}
      headers={['Дата', 'Баллы', 'Тип работ', 'Преподаватель', 'Комментарий']}
      renderItem={PointRender}
    />
  );
};
