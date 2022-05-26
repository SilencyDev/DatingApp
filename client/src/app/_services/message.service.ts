import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HttpTransportType, HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Group } from '../_models/group';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { getPaginatedHeaders, getPaginatedResult } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  apiUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient) { }

  CreateConnectionHub(user: User, otherUsername: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build()

     this.hubConnection
      .start()
      .catch(err => console.log(err));

      this.hubConnection.on("ReceiveMessageThread", messages => {
        this.messageThreadSource.next(messages);
      })

      this.hubConnection.on("NewMessage", message => {
        this.messageThread$.pipe(take(1)).subscribe({
          next: messages => this.messageThreadSource.next([...messages, message])
        })
      })

      this.hubConnection.on("UpdatedGroup", (group: Group) => {
        if (group.connections.some(x => x.username == otherUsername)) {
          this.messageThread$.pipe(take(1)).subscribe({
            next: messages => {
              messages.forEach(message => {
                if (!message.dateRead) {
                  message.dateRead = new Date(Date.now()) // Doubtfull
                }
              })
              this.messageThreadSource.next([...messages]);
            }
          })
        }
      });
  }

  DestroyHubConnection() {
    if (this.hubConnection)
      this.hubConnection.stop();
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginatedHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>(this.apiUrl + 'messages', params, this.http);
  }

  getMessagesThread(username: string) {
    return this.http.get<Message[]>(this.apiUrl + 'messages/thread/' + username);
  }

  async sendMessage(username: string, content: string) {
    return this.hubConnection.invoke('SendMessage' , {recipientUsername: username, content})
      .catch(err => console.log(err));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.apiUrl + 'messages/' + id);
  }
}
