import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { getPaginatedHeaders, getPaginatedResult } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginatedHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>(this.apiUrl + 'messages', params, this.http);
  }

  getMessagesThread(username: string) {
    return this.http.get<Message[]>(this.apiUrl + 'messages/thread/' + username);
  }

  sendMessage(username: string, content: string) {
    return this.http.post<Message>(this.apiUrl + 'messages', {recipientUsername: username, content});
  }

  deleteMessage(id: number) {
    return this.http.delete(this.apiUrl + 'messages/' + id);
  }
}
