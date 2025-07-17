import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Note } from '../models/note-model';
import { environment } from '../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class NoteService {
  private baseApiUrl = environment.baseApiUrl;
  constructor(private http: HttpClient) { }
  getAll(): Observable<Note[]> {
    return this.http.get<Note[]>(`${this.baseApiUrl}/notes`);
  }
  getById(id: number): Observable<Note> {
    return this.http.get<Note>(`${this.baseApiUrl}/notes/${id}`);
  }
  create(note: Note): Observable<Note> {
    return this.http.post<Note>(`${this.baseApiUrl}/notes`, note);
  }
  update(note: Note): Observable<Note> {
    return this.http.put<Note>(`${this.baseApiUrl}/notes/${note.id}`, note);
  }
  delete(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseApiUrl}/notes/${id}`);
  }
}