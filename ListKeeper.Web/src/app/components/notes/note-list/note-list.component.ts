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
  
  notes: Note[];
  modalInstance: any;
  editModalInstance: any;
  deleteModalInstance: any;
  currentEditingNote: Note | null = null;
  currentDeletingNote: Note | null = null;
  currentUser: User | null = null;

  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription;
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
        this.refreshNotes(this.currentSearchTerm, this.statusForm.value);
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
    this.userSubscription?.unsubscribe();
  }
  
  private setupSearchSubscription(): void {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      filter(term => term.length >= 3 || term.length === 0),
      tap(searchTerm => this.refreshNotes(searchTerm, this.statusForm.value))
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
    
    this.refreshNotes(this.currentSearchTerm, this.statusForm.value);
  }
  
  onDropdownClick(event: MouseEvent): void {
    event.stopPropagation();
  }
  
  public refreshNotes(searchTerm: string = '', statuses?: any): void {
    console.log('Refreshing notes with filters:', { searchTerm, statuses });
    
    // Get all notes from the service
    let allNotes = this.noteService.getNotes();
    
    // Apply search filter if searchTerm is provided
    if (searchTerm && searchTerm.length > 0) {
      allNotes = allNotes.filter(note => 
        note.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        note.content.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
    
    // Apply status filters if statuses are provided
    if (statuses) {
      const today = new Date();
      today.setHours(0, 0, 0, 0); // Set to start of day for accurate comparison
      
      let filteredNotes: Note[] = [];
      
      // Check which statuses are selected
      const selectedStatuses = Object.keys(statuses).filter(key => statuses[key]);
      
      // If 'All' is selected or no specific statuses are selected, show all notes
      if (selectedStatuses.includes('All') || selectedStatuses.length === 0) {
        filteredNotes = allNotes;
      } else {
        // Filter based on selected statuses
        allNotes.forEach(note => {
          const noteDate = new Date(note.dueDate);
          noteDate.setHours(0, 0, 0, 0); // Set to start of day for accurate comparison
          
          let shouldInclude = false;
          
          // Check each selected status
          selectedStatuses.forEach(status => {
            switch (status) {
              case 'Upcoming':
                // Upcoming: due date is greater than today and not completed
                if (noteDate > today && !note.isCompleted) {
                  shouldInclude = true;
                }
                break;
                
              case 'Past Due':
                // Past Due: due date is less than or equal to today and not completed
                if (noteDate <= today && !note.isCompleted) {
                  shouldInclude = true;
                }
                break;
                
              case 'Completed':
                // Completed: isCompleted is true regardless of date
                if (note.isCompleted) {
                  shouldInclude = true;
                }
                break;
            }
          });
          
          if (shouldInclude) {
            filteredNotes.push(note);
          }
        });
      }
      
      this.notes = filteredNotes;
    } else {
      this.notes = allNotes;
    }
    
    console.log('Filtered notes:', this.notes.length, 'out of', this.noteService.getNotes().length);
  }

  public openAddNoteModal(): void {
    this.modalInstance?.show();
  }

  addNote(): void {
    this.noteFormComponent.addNote();
    this.refreshNotes(this.currentSearchTerm, this.statusForm.value);
    this.modalInstance?.hide();
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
      this.noteService.deleteNote(this.currentDeletingNote.id);
      this.refreshNotes(this.currentSearchTerm, this.statusForm.value);
      this.deleteModalInstance?.hide();
      this.currentDeletingNote = null;
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
      this.refreshNotes(this.currentSearchTerm, this.statusForm.value);
      this.editModalInstance?.hide();
      this.currentEditingNote = null;
    }
  }

  completeNote(id: number): void {
    console.log('Completing note:', id);
  }
}
