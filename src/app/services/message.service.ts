import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export type MessageType = 'success' | 'error' | 'info';

export interface AppMessage {
  id: number;
  text: string;
  type: MessageType;
}

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private readonly messagesSubject = new BehaviorSubject<AppMessage[]>([]);
  private nextId = 1;

  readonly messages$: Observable<AppMessage[]> = this.messagesSubject.asObservable();

  add(text: string, type: MessageType = 'info'): void {
    const current = this.messagesSubject.getValue();
    this.messagesSubject.next([...current, { id: this.nextId++, text, type }]);
  }

  clear(): void {
    this.messagesSubject.next([]);
  }

  remove(id: number): void {
    const current = this.messagesSubject.getValue();
    const filtered = current.filter((message) => message.id !== id);
    this.messagesSubject.next(filtered);
  }
}
