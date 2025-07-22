export interface SearchCriteria {
  searchText?: string;
  showOnlyCompleted?: boolean;
  statuses: number[]; // Array of status values: 0=All, 1=Upcoming, 2=Past Due, 3=Completed
}
