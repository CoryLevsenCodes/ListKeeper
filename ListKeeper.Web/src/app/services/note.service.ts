import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Note } from '../models/note-model';
import { environment } from '../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class NoteService {
  private baseApiUrl = environment.baseApiUrl;
  constructor(private http: HttpClient) { }

  getAll(): Observable<Note[]> {
    // The backend returns { notes: Note[] }, so we need to extract the notes array
    return this.http.get<{ notes: Note[] }>(`${this.baseApiUrl}/notes`)
      .pipe(map(response => response.notes));
  }
  // getAllBySearchCriteria(searchCriteria: SearchCriteria): Observable<Note[]> {
  //   // The backend returns { notes: Note[] }, so we need to extract the notes array
  //   return this.http.post<{ notes: Note[] }>(`${this.baseApiUrl}/notes/search`, searchCriteria)
  //     .pipe(map(response => response.notes));
  // }
  getById(id: number): Observable<Note> {
    return this.http.get<Note>(`${this.baseApiUrl}/notes/${id}`);
  }
  create(note: Note): Observable<Note> {
    return this.http.post<Note>(`${this.baseApiUrl}/notes`, note);
  }
  update(note: Note): Observable<Note> {
    return this.http.put<Note>(`${this.baseApiUrl}/notes/${note.id}`, note);
  }
  delete(id: number): Observable<{ success: boolean; message: string }> {
    // The backend returns { success: boolean, message: string } for delete operations
    return this.http.delete<{ success: boolean; message: string }>(`${this.baseApiUrl}/notes/${id}`);
  }
}