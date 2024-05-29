export type StandardHistory = {
  id: number;
  date: Date;
  points: number;
  teacherGuid: string;
  standardType: '';
  comment: string | null;
  studentGuid: string;
};

export const standardTypes = [
  'Tilts',
  'Jumps',
  'PullUps',
  'Squats',
  'JumpingRopeJumps',
  'TorsoLifts',
  'FlexionAndExtensionOfArms',
  'ShuttleRun',
  'Other',
] as const;

export type StandardType = (typeof standardTypes)[number];

export const standardTypeRus: Record<string, string> = {
  Tilts: 'Наклоны',
  Jumps: 'Прыжки',
  PullUps: 'Подтягивания',
  Squats: 'Приседания',
  JumpingRopeJumps: 'Прыжки через скакалку',
  TorsoLifts: 'Поднимания туловища',
  FlexionAndExtensionOfArms: 'Сгибания и разгибания рук',
  ShuttleRun: 'Челночный бег',
  Other: 'Другое',
};
