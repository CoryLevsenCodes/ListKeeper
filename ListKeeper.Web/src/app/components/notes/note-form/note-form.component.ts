import { Component, OnInit, OnChanges, SimpleChanges, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NoteService } from '../../../services/note.service';
import { Note } from '../../../models/note-model';

@Component({
  selector: 'app-note-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './note-form.component.html',
  // We will create and add the CSS file next
  styleUrls: ['./note-form.component.css']
})
export class NoteFormComponent implements OnInit, OnChanges {
  @Input() noteToEdit: Note | null = null;
  @Input() isEditMode: boolean = false;
  
  noteForm: FormGroup;

  // Define the beautiful, soft color palette for the user to choose from.
  availableColors: string[] = [
    '#FFF3CD', // Light Yellow
    '#D1E7DD', // Light Green
    '#CFF4FC', // Light Blue
    '#F8D7DA', // Light Red
    '#E2D9F3', // Light Purple
  ];

  constructor(
    private fb: FormBuilder,
    private noteService: NoteService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Re-initialize the form when noteToEdit or isEditMode changes
    if (changes['noteToEdit'] || changes['isEditMode']) {
      if (this.noteForm) {
        this.initializeForm();
      }
    }
  }

  private initializeForm(): void {
    if (this.isEditMode && this.noteToEdit) {
      // Initialize form with existing note data for editing
      this.noteForm = this.fb.group({
        title: [this.noteToEdit.title, Validators.required],
        content: [this.noteToEdit.content, Validators.required],
        dueDate: [this.formatDateForInput(this.noteToEdit.dueDate), Validators.required],
        isCompleted: [this.noteToEdit.isCompleted],
        color: [this.noteToEdit.color, Validators.required]
      });
    } else {
      // Initialize form for adding new note
      this.noteForm = this.fb.group({
        title: ['', Validators.required],
        content: ['', Validators.required],
        dueDate: [this.getFutureDateString(7), Validators.required],
        isCompleted: [false],
        color: [this.availableColors[0], Validators.required] 
      });
    }
  }

  private formatDateForInput(date: Date): string {
    const d = new Date(date);
    return d.toISOString().split('T')[0];
  }

  /**
   * Sets the value of the 'color' form control when a user clicks a swatch.
   * @param color The hex code of the selected color.
   */
  selectColor(color: string): void {
    this.noteForm.patchValue({ color: color });
  }

  private getFutureDateString(daysToAdd: number): string {
    const date = new Date();
    date.setDate(date.getDate() + daysToAdd);
    return date.toISOString().split('T')[0];
  }

  public addNote(): void {
    if (this.noteForm.valid) {
      // this.noteService.addNote(this.noteForm.value);
      this.resetForm();
    } else {
      this.noteForm.markAllAsTouched();
    }
  }

  public updateNote(): void {
    if (this.noteForm.valid && this.noteToEdit) {
      const updatedNote: Note = {
        ...this.noteToEdit,
        ...this.noteForm.value,
        dueDate: new Date(this.noteForm.value.dueDate)
      };
      // this.noteService.updateNote(updatedNote);
    } else {
      this.noteForm.markAllAsTouched();
    }
  }

  public saveNote(): void {
    if (this.isEditMode) {
      this.updateNote();
    } else {
      this.addNote();
    }
  }

  private resetForm(): void {
    this.noteForm.reset({
      title: '',
      content: '',
      dueDate: this.getFutureDateString(7),
      isCompleted: false,
      color: this.availableColors[0]
    });
  }
}
