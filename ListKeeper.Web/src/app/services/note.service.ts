import { Injectable } from '@angular/core';
import { Note } from '../models/note-model';

@Injectable({
  providedIn: 'root',
})
export class NoteService {

  private notes: Note[] = [
  {
    id: 1,
    title: 'Finalize quarterly report',
    content: 'Compile sales data and performance metrics for the Q2 report. Draft slides for the presentation on Friday.',
    dueDate: new Date('2025-07-15T17:00:00Z'),
    isCompleted: true,
    color: '#D1E7DD', // Light Green
  },
  {
    id: 2,
    title: 'Grocery Shopping',
    content: 'Milk, bread, eggs, chicken breast, spinach, and coffee beans.',
    dueDate: new Date('2025-06-23T18:00:00Z'),
    isCompleted: false,
    color: '#F8D7DA', // Light Red
  },
  {
    id: 3,
    title: 'Schedule dentist appointment',
    content: 'Call Dr. Smith\'s office to schedule a routine check-up and cleaning.',
    dueDate: new Date('2025-06-20T12:00:00Z'), // Past due
    isCompleted: true,
    color: '#FFF3CD', // Light Yellow
  },
  {
    id: 4,
    title: 'Pay monthly credit card bill',
    content: 'Due by the 25th. Check statement for any unusual charges.',
    dueDate: new Date('2025-06-25T23:59:00Z'),
    isCompleted: false,
    color: '#D1E7DD',
  },
  {
    id: 5,
    title: 'Plan weekend trip to Canmore',
    content: 'Book hotel/Airbnb, check hiking trail conditions, and make dinner reservations.',
    dueDate: new Date('2025-08-01T12:00:00Z'),
    isCompleted: false,
    color: '#CFF4FC', // Light Blue
  },
  {
    id: 6,
    title: 'Renew driver\'s license',
    content: 'License expires in August. Gather necessary documents and visit the registry.',
    dueDate: new Date('2025-07-30T09:00:00Z'),
    isCompleted: false,
    color: '#F8D7DA',
  },
  {
    id: 7,
    title: 'Project Phoenix Kick-off Meeting',
    content: 'Prepare agenda and introductory slides. Room 3B at 10 AM.',
    dueDate: new Date('2025-06-24T10:00:00Z'),
    isCompleted: false,
    color: '#D1E7DD',
  },
  {
    id: 8,
    title: 'Pick up dry cleaning',
    content: 'Ticket number is 452. Ready for pickup after 3 PM on Wednesday.',
    dueDate: new Date('2025-06-18T15:00:00Z'), // Past due
    isCompleted: true,
    color: '#FFF3CD',
  },
  {
    id: 9,
    title: 'Research new CRMs',
    content: 'Evaluate options for a new customer relationship management system. Focus on Salesforce, HubSpot, and Zoho.',
    dueDate: new Date('2025-07-18T17:00:00Z'),
    isCompleted: false,
    color: '#CFF4FC',
  },
  {
    id: 10,
    title: 'Call mom for her birthday',
    content: 'Her birthday is on the 28th. Don\'t forget!',
    dueDate: new Date('2025-06-28T14:00:00Z'),
    isCompleted: false,
    color: '#F8D7DA',
  },
  {
    id: 11,
    title: 'Fix leaky faucet in kitchen',
    content: 'Buy a new washer kit from Canadian Tire. Watch YouTube tutorial.',
    dueDate: new Date('2025-06-15T12:00:00Z'), // Past due
    isCompleted: false,
    color: '#FFF3CD',
  },
  {
    id: 12,
    title: 'Submit expense report',
    content: 'Include receipts from the Calgary business trip. Deadline is EOD Friday.',
    dueDate: new Date('2025-06-27T17:00:00Z'),
    isCompleted: false,
    color: '#D1E7DD',
  },
  {
    id: 13,
    title: 'Gym Session - Legs',
    content: 'Squats, deadlifts, leg press, and calf raises.',
    dueDate: new Date('2025-06-22T19:00:00Z'), // Today
    isCompleted: false,
    color: '#CFF4FC',
  },
  {
    id: 14,
    title: 'Read "Atomic Habits"',
    content: 'Finish chapter 5. Take notes on the concept of habit stacking.',
    dueDate: new Date('2025-06-29T21:00:00Z'),
    isCompleted: false,
    color: '#E2D9F3', // Light Purple
  },
  {
    id: 15,
    title: 'Water the plants',
    content: 'Ferns in the living room and the succulents on the balcony.',
    dueDate: new Date('2025-06-22T08:00:00Z'), // Today
    isCompleted: true,
    color: '#D1E7DD',
  },
  {
    id: 16,
    title: 'Update LinkedIn Profile',
    content: 'Add new skills and recent project accomplishments.',
    dueDate: new Date('2025-07-05T11:00:00Z'),
    isCompleted: false,
    color: '#CFF4FC',
  },
  {
    id: 17,
    title: 'Organize garage',
    content: 'Sort tools, donate old items, and sweep the floor. It\'s a mess!',
    dueDate: new Date('2025-07-12T10:00:00Z'),
    isCompleted: false,
    color: '#FFF3CD',
  },
  {
    id: 18,
    title: 'Buy concert tickets for July Talk',
    content: 'Tickets go on sale Friday at 10 AM. Set a reminder.',
    dueDate: new Date('2025-06-27T10:00:00Z'),
    isCompleted: false,
    color: '#F8D7DA',
  },
  {
    id: 19,
    title: 'Review pull request from Sarah',
    content: 'Check the new API endpoint logic in the `feature/user-auth` branch.',
    dueDate: new Date('2025-06-23T16:00:00Z'),
    isCompleted: false,
    color: '#E2D9F3',
  },
  {
    id: 20,
    title: 'Book oil change for car',
    content: 'Due for a service. Call the dealership to schedule for next week.',
    dueDate: new Date('2025-06-20T12:00:00Z'), // Past due
    isCompleted: true,
    color: '#FFF3CD',
  }
];

  private nextId = 21;

  getNotes(): Note[] {
    return this.notes;
  }

  addNote(noteData: { title: string, content: string, dueDate: string, isCompleted: boolean, color: string }): void {
    // A more robust way to generate a new ID than just using the array length.
    const newId = this.notes.length > 0 ? Math.max(...this.notes.map(n => n.id)) + 1 : 1;
    
    const newNote: Note = {
      id: newId,
      title: noteData.title,
      content: noteData.content,
      // The form provides a date string (e.g., "2025-06-29"), so it needs to be converted to a Date object.
      dueDate: new Date(noteData.dueDate), 
      isCompleted: noteData.isCompleted,
      color: noteData.color
    };

    this.notes.push(newNote);
  }


  deleteNote(id: number): void {
    this.notes = this.notes.filter(note => note.id !== id);
  }

  updateNote(updatedNote: Note): void {
    const index = this.notes.findIndex(note => note.id === updatedNote.id);
    if (index !== -1) {
      this.notes[index] = updatedNote;
    }
  }
}
