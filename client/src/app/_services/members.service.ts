import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  members: Member[] = [];
  apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMembers() {
    if (this.members.length > 0) return of(this.members)
    return this.http.get<Member[]>(this.apiUrl + 'users').pipe(
      map(Members => {
        this.members = Members;
        return Members;
      })
    )
  }

  getMember(username: string) {
    let find = this.members.find(user => user.username === username);
    if (find != undefined) return of(find);
    return this.http.get<Member>(this.apiUrl + 'users/' + username)
  }

  updateMember(member: Member) {
    return this.http.put(this.apiUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        if (index >= 0)
          this.members[index] = member;
      })
    )
  }
}
