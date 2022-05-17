import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable, of, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';

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
    let httpParams = new HttpParams();
    httpParams = httpParams.append('pageNumber', userParams.pageNumber.toString())
                 .append('pageSize', userParams.pageSize.toString())
                 .append('gender', userParams.gender)
                 .append('orderBy', userParams.orderBy)
                 .append('maxAge', userParams.maxAge.toString())
                 .append('minAge', userParams.minAge.toString());

    return this.getPaginatedResult<Member[]>(this.apiUrl + 'users', httpParams);
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

  getLikes(predicate: string) {
    return this.http.get(this.apiUrl + 'likes?predicate=' + predicate);
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

  private getPaginatedResult<T>(url: string, httpParams: HttpParams) {
    let paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

    return this.http.get<T>(url, {observe: 'response', params: httpParams}).pipe(
      map(response => {
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') != null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    )
  }
}
