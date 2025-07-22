export enum NoteStatus {
  All = 0,
  Upcoming = 1,
  PastDue = 2,
  Completed = 3
}

export const NOTE_STATUS_LABELS = {
  [NoteStatus.All]: 'All',
  [NoteStatus.Upcoming]: 'Upcoming',
  [NoteStatus.PastDue]: 'Past Due',
  [NoteStatus.Completed]: 'Completed'
};

export const STATUS_ARRAY = ['All', 'Upcoming', 'Past Due', 'Completed'];
