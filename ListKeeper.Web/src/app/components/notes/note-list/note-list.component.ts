import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, tap } from 'rxjs/operators';
import { NoteService } from '../../../services/note.service';
import { UserService } from '../../../services/user.service';
import { Note } from '../../../models/note-model';
import { User } from '../../../models/user.model';
import { SearchCriteria } from '../../../models/search-criteria';
import { NoteStatus } from '../../../models/note-status';
import { NoteItemComponent } from '../note-item/note-item.component';
import { NoteFormComponent } from '../note-form/note-form.component';

// Declare bootstrap for TypeScript
declare var bootstrap: any;

@Component({
    selector: 'app-note-list',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, NoteItemComponent, NoteFormComponent],
    templateUrl: './note-list.component.html',
    styleUrls: ['./note-list.component.css']
})
export class NoteListComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild(NoteFormComponent) noteFormComponent: NoteFormComponent;
  @ViewChild('editNoteForm') editNoteFormComponent: NoteFormComponent;
  
  notes: Note[] = [];
  modalInstance: any;
  editModalInstance: any;
  deleteModalInstance: any;
  currentEditingNote: Note | null = null;
  currentDeletingNote: Note | null = null;
  currentUser: User | null = null;

  private searchSubject = new Subject<string>();
  private statusSubject = new Subject<any>();
  private searchSubscription: Subscription;
  private statusSubscription: Subscription;
  private userSubscription: Subscription;
  private currentSearchTerm: string = '';
  
  statusForm: FormGroup;
  statuses: string[] = ['All', 'Upcoming', 'Past Due', 'Completed'];

  constructor(
    private noteService: NoteService,
    private userService: UserService,
    private router: Router,
    private fb: FormBuilder
  ) {
    // Initialize the status filter form with 'Upcoming' as default
    const statusControls = this.statuses.reduce((acc, status) => {
      // Set 'Upcoming' as the default selected status
      acc[status] = [status === 'Upcoming']; 
      return acc;
    }, {});
    this.statusForm = this.fb.group(statusControls);
  }

  ngOnInit(): void {
    // Subscribe to user changes and setup notes functionality
    this.userSubscription = this.userService.currentUser.subscribe(user => {
      this.currentUser = user;
      if (user) {
        // User is logged in, setup notes functionality
        this.setupSearchSubscription();
        this.setupStatusSubscription();
        this.performSearch(this.currentSearchTerm, this.statusForm.value);
      } else {
        // No user logged in, redirect to home page
        this.router.navigate(['']);
      }
    });
  }

  ngAfterViewInit() {
    // Initialize the add note modal
    const addNoteModalElement = document.getElementById('addNoteModal');
    if (addNoteModalElement) {
      this.modalInstance = new bootstrap.Modal(addNoteModalElement);
    }

    // Initialize the edit note modal
    const editNoteModalElement = document.getElementById('editNoteModal');
    if (editNoteModalElement) {
      this.editModalInstance = new bootstrap.Modal(editNoteModalElement);
    }

    // Initialize the delete confirmation modal
    const deleteNoteModalElement = document.getElementById('deleteNoteModal');
    if (deleteNoteModalElement) {
      this.deleteModalInstance = new bootstrap.Modal(deleteNoteModalElement);
    }
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
    this.statusSubscription?.unsubscribe();
    this.userSubscription?.unsubscribe();
  }
  
  private setupSearchSubscription(): void {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      filter(term => term.length >= 3 || term.length === 0),
      tap(searchTerm => this.performSearch(searchTerm, this.statusForm.value))
    ).subscribe();
  }

  private setupStatusSubscription(): void {
    this.statusSubscription = this.statusSubject.pipe(
      debounceTime(100), // Short debounce for status changes
      tap(statusValues => this.performSearch(this.currentSearchTerm, statusValues))
    ).subscribe();
  }

  onSearch(event: Event): void {
    const searchTerm = (event.target as HTMLInputElement).value;
    this.currentSearchTerm = searchTerm;
    this.searchSubject.next(searchTerm);
  }

  onStatusChange(changedStatus: string): void {
    const allControl = this.statusForm.get('All');
    const otherStatuses = this.statuses.filter(c => c !== 'All');

    if (changedStatus === 'All') {
      const isAllSelected = allControl.value;
      if (isAllSelected) {
        // If All is being selected, select all other statuses
        const patchValue = otherStatuses.reduce((acc, status) => {
          acc[status] = true;
          return acc;
        }, {});
        this.statusForm.patchValue(patchValue, { emitEvent: false });
      } else {
        // If All is being unselected, unselect all EXCEPT Upcoming
        const patchValue = otherStatuses.reduce((acc, status) => {
          acc[status] = status === 'Upcoming'; // Only keep Upcoming selected
          return acc;
        }, {});
        this.statusForm.patchValue(patchValue, { emitEvent: false });
      }
    } else {
      // Handle changes to individual status items
      
      // Special handling for Upcoming status
      if (changedStatus === 'Upcoming' && !this.statusForm.get('Upcoming').value) {
        // If user is trying to unselect Upcoming, check if other statuses are selected
        const otherSelectedStatuses = otherStatuses.filter(status => 
          status !== 'Upcoming' && !!this.statusForm.get(status).value
        );
        
        // If no other statuses are selected, don't allow unselecting Upcoming
        if (otherSelectedStatuses.length === 0) {
          this.statusForm.get('Upcoming').setValue(true, { emitEvent: false });
          return; // Exit early since we're keeping Upcoming selected
        }
        // If other statuses ARE selected, allow unselecting Upcoming (continue with normal flow)
      }
      
      // If a specific status is being unchecked and All is currently selected, uncheck All
      if (!this.statusForm.get(changedStatus).value && allControl.value) {
        allControl.setValue(false, { emitEvent: false });
      } else {
        // Check if all other statuses are now selected to auto-select All
        const allOthersSelected = otherStatuses.every(status => !!this.statusForm.get(status).value);
        if (allControl.value !== allOthersSelected) {
          allControl.setValue(allOthersSelected, { emitEvent: false });
        }
      }
      
      // Ensure at least Upcoming is always selected (fallback safety check)
      const anyStatusSelected = otherStatuses.some(status => !!this.statusForm.get(status).value);
      if (!anyStatusSelected) {
        this.statusForm.get('Upcoming').setValue(true, { emitEvent: false });
      }
    }
    
    // Trigger the status search after all form updates
    this.statusSubject.next(this.statusForm.value);
  }
  
  onDropdownClick(event: MouseEvent): void {
    event.stopPropagation();
  }
  

  private performSearch(searchTerm: string = '', statuses?: any): void {
    const selectedStatuses = statuses ? Object.keys(statuses).filter(k => statuses[k]) : ['All'];
    const isAllSelected = selectedStatuses.includes('All');

    // Create search criteria
    const searchCriteria: SearchCriteria = {
      searchText: searchTerm && searchTerm.length > 0 ? searchTerm : undefined,
      statuses: this.mapStatusesToNumbers(selectedStatuses)
    };

    // Use the search service which will handle "All" case internally
    this.noteService.getAllBySearchCriteria(searchCriteria).subscribe({
      next: (data: Note[]) => {
        this.notes = data;
      },
      error: (error) => {
        console.error('Error loading notes with search criteria:', error);
        this.notes = [];
      }
    });
  }

  private mapStatusesToNumbers(statuses: string[]): number[] {
    return statuses.map(status => {
      switch (status) {
        case 'All':
          return NoteStatus.All;
        case 'Upcoming':
          return NoteStatus.Upcoming;
        case 'Past Due':
          return NoteStatus.PastDue;
        case 'Completed':
          return NoteStatus.Completed;
        default:
          return NoteStatus.All;
      }
    });
  }

  // Keep the old method for backward compatibility with other calls
  public refreshNotes(searchTerm: string = '', statuses?: any): void {
    this.performSearch(searchTerm, statuses);
  }


  public openAddNoteModal(): void {
    this.modalInstance?.show();
  }

  addNote(): void {
    this.noteFormComponent.addNote();
  }

  onNoteAdded(note: Note): void {
    this.refreshNotes(this.currentSearchTerm, this.statusForm.value);
    this.modalInstance?.hide();
  }

  onNoteUpdated(note: Note): void {
    this.refreshNotes(this.currentSearchTerm, this.statusForm.value);
    this.editModalInstance?.hide();
    this.currentEditingNote = null;
  }

  deleteNote(id: number): void {
    // Find the note to delete and show custom confirmation modal
    const noteToDelete = this.notes.find(note => note.id === id);
    if (noteToDelete) {
      this.currentDeletingNote = noteToDelete;
      this.deleteModalInstance?.show();
    }
  }

  confirmDelete(): void {
    if (this.currentDeletingNote) {
      this.noteService.delete(this.currentDeletingNote.id).subscribe({
        next: (response) => {
          console.log('Note deleted successfully:', response.message);
          this.refreshNotes(this.currentSearchTerm, this.statusForm.value);
          this.deleteModalInstance?.hide();
          this.currentDeletingNote = null;
        },
        error: (error) => {
          console.error('Error deleting note:', error);
          this.deleteModalInstance?.hide();
          this.currentDeletingNote = null;
        }
      });
    }
  }

  cancelDelete(): void {
    this.deleteModalInstance?.hide();
    this.currentDeletingNote = null;
  }

  editNote(note: Note): void {
    this.currentEditingNote = note;
    this.editModalInstance?.show();
  }

  updateNote(): void {
    if (this.currentEditingNote && this.editNoteFormComponent) {
      this.editNoteFormComponent.saveNote();
    }
  }

  completeNote(id: number): void {
    const noteToComplete = this.notes.find(note => note.id === id);
    if (noteToComplete) {
      const updatedNote: Note = {
        ...noteToComplete,
        isCompleted: !noteToComplete.isCompleted
      };
      
      this.noteService.update(updatedNote).subscribe({
        next: (updated) => {
          console.log('Note completion status updated:', updated);
          this.refreshNotes(this.currentSearchTerm, this.statusForm.value);
        },
        error: (error) => {
          console.error('Error updating note completion status:', error);
        }
      });
    }
  }
}
