import { Component, Input, Output, EventEmitter, AfterViewInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Note } from '../../../models/note-model';

// Declare bootstrap for TypeScript
declare var bootstrap: any;

@Component({
  selector: 'app-note-item',
  standalone: true,
  // Add DatePipe to the imports array to make the 'date' pipe available in the template
  imports: [CommonModule, DatePipe],
  templateUrl: './note-item.component.html',
  styleUrls: ['./note-item.component.css']
})
export class NoteItemComponent implements AfterViewInit {
  @Input() note: Note;
  @Output() delete = new EventEmitter<number>();
  @Output() edit = new EventEmitter<Note>();
  @Output() complete = new EventEmitter<void>();

  ngAfterViewInit(): void {
    // Initialize Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
      return new bootstrap.Tooltip(tooltipTriggerEl);
    });
  }

  onDelete(): void {
    // Prevent default anchor behavior and emit the event
    event.preventDefault();
    this.delete.emit(this.note.id);
  }

  onEdit(): void {
    event?.preventDefault();
    this.edit.emit(this.note);
  }

  onComplete(): void {
    event.preventDefault();
    this.complete.emit();
  }
}
