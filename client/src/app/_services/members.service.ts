import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable, of, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { LikeParams } from '../_models/likeParams';
import { Member } from '../_models/member';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginatedHeaders, getPaginatedResult } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  members: Member[] = [];
  apiUrl = environment.apiUrl;
  userParams: UserParams;
  user: User;

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    })
  }

  getMembers(userParams: UserParams) {
    let httpParams = getPaginatedHeaders(userParams.pageNumber, userParams.pageSize);
    httpParams = httpParams.append('gender', userParams.gender)
              .append('orderBy', userParams.orderBy)
              .append('maxAge', userParams.maxAge.toString())
              .append('minAge', userParams.minAge.toString());

    return getPaginatedResult<Member[]>(this.apiUrl + 'users', httpParams, this.http);
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user);
  }

  like(username: string) {
    return this.http.post(this.apiUrl + 'likes/' + username, {});
  }

  getLikes(likeParams: LikeParams) {
    let httpParams = getPaginatedHeaders(likeParams.pageNumber, likeParams.pageSize);
    httpParams = httpParams.append('predicate', likeParams.predicate);
    return getPaginatedResult<Partial<Member[]>>(
      this.apiUrl + 'likes?predicate=' + likeParams.predicate, httpParams, this.http);
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

  deletePhoto(photoId: number) {
    return this.http.delete(this.apiUrl + 'users/delete-photo/' + photoId);
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.apiUrl + 'users/set-main-photo/' + photoId, {});
  }
}
