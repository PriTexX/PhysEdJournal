export type PointsHistory = {
  id: number;
  date: Date;
  points: number;
  teacherGuid: string;
  workType: WorkType;
  comment: string | null;
  studentGuid: string;
};

export const workTypes = [
  'ExternalFitness',
  'GTO',
  'Science',
  'OnlineWork',
  'InternalTeam',
  'Activist',
  'Competition',
] as const;

export type WorkType = (typeof workTypes)[number];

export const workTypeRus: Record<string, string> = {
  ExternalFitness: 'Внешний фитнес',
  GTO: 'ГТО',
  Science: 'Наука',
  OnlineWork: 'СДО',
  InternalTeam: 'Сборная',
  Activist: 'Активист',
  Competition: 'Соревнования',
};
