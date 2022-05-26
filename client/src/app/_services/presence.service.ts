import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();


  constructor(private toastr: ToastrService, private router: Router) { }

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build()

    this.hubConnection
      .start()
      .catch(err => console.log(err));

    this.hubConnection.on('UserOnline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: usernames => {
          this.onlineUsersSource.next([...usernames, username])
        }
      })
    })

    this.hubConnection.on('UserOffline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: usernames => {
          this.onlineUsersSource.next([...usernames.filter(x => x != username)])
        }
      })
    })

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsersSource.next(usernames);
    })

    this.hubConnection.on('NewMessageReceived', (result) => {
      this.toastr.info('New message from  ' + (result.pseudo ? result.pseudo : result.username) + ' !')
        .onTap
        .pipe(take(1))
        .subscribe({
          next: () => this.router.navigateByUrl('/members/' + result.username + '?tab=3')
        });
    })
  }

  destroyHubConnection() {
    this.hubConnection
      .stop()
      .catch(err => console.log(err));
  }


}
